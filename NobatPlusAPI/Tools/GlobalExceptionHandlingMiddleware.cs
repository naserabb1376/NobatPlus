using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using NobatPlusDATA.DataLayer;
using NobatPlusDATA.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NobatPlusAPI.Tools
{
    public class InfrastructureExceptionHandlingMiddleware
    {
        private const string ErrorLogIdItemKey = "NobatPlus.ErrorLogId";
        private readonly RequestDelegate _next;
        private readonly ILogger<InfrastructureExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;
        private readonly IServiceScopeFactory _scopeFactory;

        public InfrastructureExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<InfrastructureExceptionHandlingMiddleware> logger,
            IHostEnvironment environment,
            IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unhandled infrastructure exception while processing {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                var logId = context.Items.TryGetValue(ErrorLogIdItemKey, out var existingLogId) &&
                            existingLogId is long id
                    ? id
                    : await SaveInfrastructureErrorAsync(context, exception);

                if (context.Response.HasStarted)
                {
                    ExceptionDispatchInfo.Capture(exception).Throw();
                    return;
                }

                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var errorResponse = new JObject
                {
                    ["status"] = false,
                    ["errorMessage"] = _environment.IsDevelopment()
                        ? exception.GetBaseException().Message
                        : "خطای داخلی سرور رخ داد."
                };

                if (logId > 0)
                    errorResponse["logId"] = logId;

                await context.Response.WriteAsync(errorResponse.ToString(Formatting.None));
            }
        }

        private async Task<long> SaveInfrastructureErrorAsync(HttpContext context, Exception exception)
        {
            try
            {
                var descriptor = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
                var origin = GlobalExceptionHandlingMiddleware.GetExceptionOrigin(
                    exception,
                    descriptor,
                    context);
                await using var scope = _scopeFactory.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<NobatPlusContext>();
                var now = DateTime.Now.ToShamsi();
                var log = new Domains.Log
                {
                    CreateDate = now,
                    UpdateDate = now,
                    LogTime = now,
                    ActionName = origin.ActionName,
                    LogType = "ERROR",
                    LogLayer = origin.Layer,
                    Description = GlobalExceptionHandlingMiddleware.BuildExceptionDescription(
                        context,
                        exception,
                        origin),
                    IsActive = true
                };

                await dbContext.Logs.AddAsync(log);
                await dbContext.SaveChangesAsync();
                context.Items[ErrorLogIdItemKey] = log.ID;
                return log.ID;
            }
            catch (Exception loggingException)
            {
                _logger.LogError(loggingException, "Could not persist infrastructure error log");
                return 0;
            }
        }
    }

    public class GlobalExceptionHandlingMiddleware
    {
        internal const string RequestInputItemKey = "NobatPlus.ErrorRequestInput";
        internal const string RepositoryInvocationsItemKey = "NobatPlus.RepositoryInvocations";
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger,
            IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await CaptureRequestInputAsync(context);
            var descriptor = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
            var originalBody = context.Response.Body;

            await using var responseBuffer = new MemoryStream();
            context.Response.Body = responseBuffer;

            try
            {
                await _next(context);
                await ProcessResponseAsync(context, descriptor, responseBuffer, originalBody);
            }
            catch (Exception ex)
            {
                context.Response.Body = originalBody;
                _logger.LogDebug(
                    ex,
                    "Passing unhandled endpoint exception to the outer exception middleware");
                ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }

        private async Task ProcessResponseAsync(
            HttpContext context,
            ControllerActionDescriptor? descriptor,
            MemoryStream responseBuffer,
            Stream originalBody)
        {
            if (descriptor == null ||
                (!IsJsonResponse(context.Response.ContentType) && context.Response.StatusCode < 400))
            {
                responseBuffer.Position = 0;
                context.Response.Body = originalBody;
                await responseBuffer.CopyToAsync(originalBody);
                return;
            }

            responseBuffer.Position = 0;
            string responseText;
            using (var reader = new StreamReader(responseBuffer, Encoding.UTF8, false, leaveOpen: true))
            {
                responseText = await reader.ReadToEndAsync();
            }

            var output = responseText;
            var responseChanged = false;
            if (descriptor != null && TryGetControlledError(context.Response.StatusCode, responseText, out var errorMessage, out var responseJson))
            {
                var controlledOrigin = ExtractControlledErrorOrigin(responseJson, descriptor, context);
                responseChanged = RemoveInternalErrorOrigin(responseJson);
                var existingLogId = GetProperty(responseJson, "logId")?.Value<long?>() ?? 0;
                var logId = existingLogId > 0
                    ? existingLogId
                    : await SaveErrorLogAsync(
                        controlledOrigin.ActionName,
                        controlledOrigin.Layer,
                        BuildControlledErrorDescription(
                            context,
                            errorMessage,
                            controlledOrigin));

                if (logId > 0)
                {
                    responseJson["logId"] = logId;
                    responseChanged = true;
                }

                output = responseJson.ToString(Formatting.None);
            }
            else if (TryParseObject(responseText, out var successfulResponse) &&
                     RemoveInternalErrorOrigin(successfulResponse))
            {
                output = successfulResponse.ToString(Formatting.None);
                responseChanged = true;
            }

            context.Response.Body = originalBody;
            if (responseChanged)
            {
                context.Response.ContentType = "application/json";
                context.Response.ContentLength = Encoding.UTF8.GetByteCount(output);
                await context.Response.WriteAsync(output);
                return;
            }

            responseBuffer.Position = 0;
            await responseBuffer.CopyToAsync(originalBody);
        }

        private async Task<long> SaveErrorLogAsync(string actionName, string layer, string description)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<NobatPlusContext>();
                var now = DateTime.Now.ToShamsi();
                var log = new Domains.Log
                {
                    CreateDate = now,
                    UpdateDate = now,
                    LogTime = now,
                    ActionName = actionName,
                    LogType = "ERROR",
                    LogLayer = layer,
                    Description = description,
                    IsActive = true
                };

                await dbContext.Logs.AddAsync(log);
                await dbContext.SaveChangesAsync();
                return log.ID;
            }
            catch (Exception loggingException)
            {
                _logger.LogError(loggingException, "Could not persist application error log");
                return 0;
            }
        }

        private static async Task CaptureRequestInputAsync(HttpContext context)
        {
            if (context.Items.ContainsKey(RequestInputItemKey))
                return;

            JToken input = JValue.CreateNull();
            if (context.Request.ContentLength > 0 &&
                IsJsonResponse(context.Request.ContentType))
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(
                    context.Request.Body,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                if (!string.IsNullOrWhiteSpace(body))
                {
                    try
                    {
                        input = JToken.Parse(body);
                    }
                    catch (JsonReaderException)
                    {
                        input = body;
                    }
                }
            }

            context.Items[RequestInputItemKey] = SanitizeForLogging(input);
        }

        private static bool TryGetControlledError(
            int statusCode,
            string responseText,
            out string errorMessage,
            out JObject responseJson)
        {
            errorMessage = string.Empty;
            responseJson = new JObject();

            JToken? token = null;
            if (!string.IsNullOrWhiteSpace(responseText))
            {
                try
                {
                    token = JToken.Parse(responseText);
                }
                catch (JsonReaderException)
                {
                    token = null;
                }
            }

            if (token is JObject jsonObject)
            {
                responseJson = jsonObject;
                var statusToken = GetProperty(jsonObject, "status");
                var hasFailedStatus = statusToken != null &&
                                      statusToken.Type == JTokenType.Boolean &&
                                      !statusToken.Value<bool>();

                if (statusCode < 400 && !hasFailedStatus)
                    return false;

                errorMessage = GetProperty(jsonObject, "errorMessage")?.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(errorMessage))
                    errorMessage = FlattenValidationErrors(GetProperty(jsonObject, "errors"));
            }
            else
            {
                if (statusCode < 400)
                    return false;

                errorMessage = token?.Type == JTokenType.String
                    ? token.Value<string>() ?? string.Empty
                    : responseText;

                responseJson = new JObject
                {
                    ["status"] = false
                };
            }

            if (string.IsNullOrWhiteSpace(errorMessage))
                errorMessage = $"درخواست با کد وضعیت HTTP {statusCode} ناموفق بود.";

            if (GetProperty(responseJson, "errorMessage") == null)
                responseJson["errorMessage"] = errorMessage;

            return true;
        }

        private static JToken? GetProperty(JObject json, string propertyName)
        {
            return json.Properties()
                .FirstOrDefault(x => string.Equals(x.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                ?.Value;
        }

        private static bool TryParseObject(string responseText, out JObject responseJson)
        {
            responseJson = new JObject();
            if (string.IsNullOrWhiteSpace(responseText))
                return false;

            try
            {
                responseJson = JObject.Parse(responseText);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }

        private static (string Layer, string ActionName, string StackTrace) ExtractControlledErrorOrigin(
            JObject responseJson,
            ControllerActionDescriptor descriptor,
            HttpContext context)
        {
            var layer = GetProperty(responseJson, "errorOriginLayer")?.ToString();
            var actionName = GetProperty(responseJson, "errorOriginAction")?.ToString();
            var stackTrace = GetProperty(responseJson, "errorOriginStackTrace")?.ToString() ?? string.Empty;

            return (
                string.IsNullOrWhiteSpace(layer) ? "Controller" : layer,
                string.IsNullOrWhiteSpace(actionName) ? GetActionName(descriptor, context) : actionName,
                stackTrace);
        }

        private static bool RemoveInternalErrorOrigin(JObject responseJson)
        {
            var internalProperties = responseJson.Properties()
                .Where(property =>
                    string.Equals(property.Name, "errorOriginLayer", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(property.Name, "errorOriginAction", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(property.Name, "errorOriginStackTrace", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var property in internalProperties)
                property.Remove();

            return internalProperties.Count > 0;
        }

        private static bool IsJsonResponse(string? contentType)
        {
            return !string.IsNullOrWhiteSpace(contentType) &&
                   (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase) ||
                    contentType.Contains("+json", StringComparison.OrdinalIgnoreCase));
        }

        private static string FlattenValidationErrors(JToken? errors)
        {
            if (errors is not JObject errorsObject)
                return string.Empty;

            return string.Join(
                " | ",
                errorsObject.Properties()
                    .SelectMany(property => property.Value.Type == JTokenType.Array
                        ? property.Value.Values<string>()
                        : new[] { property.Value.ToString() })
                    .Where(message => !string.IsNullOrWhiteSpace(message)));
        }

        internal static string GetActionName(ControllerActionDescriptor? descriptor, HttpContext context)
        {
            return descriptor == null
                ? $"{context.Request.Method} {context.Request.Path}"
                : $"{descriptor.ControllerName}.{descriptor.ActionName}";
        }

        internal static (string Layer, string ActionName) GetExceptionOrigin(
            Exception exception,
            ControllerActionDescriptor? descriptor,
            HttpContext context)
        {
            var frames = EnumerateExceptionFrames(exception).ToList();

            var repositoryFrame = frames.FirstOrDefault(IsServiceRepositoryFrame);
            if (repositoryFrame != null)
                return ("ServiceRepository", GetFrameActionName(repositoryFrame));

            // Some runtime/optimization combinations expose only the generated async
            // MoveNext method through StackFrame. The formatted stack still contains
            // the logical repository method, so use it as a second source of truth.
            var repositoryAction = GetRepositoryActionFromExceptionText(exception);
            if (repositoryAction != null)
                return ("ServiceRepository", repositoryAction);

            var backgroundJobFrame = frames.FirstOrDefault(frame =>
                GetFrameType(frame)?.Name.Contains("JobManager", StringComparison.Ordinal) == true);
            if (backgroundJobFrame != null)
                return ("BackgroundJob", GetFrameActionName(backgroundJobFrame));

            var controllerFrame = frames.FirstOrDefault(IsControllerFrame);
            if (controllerFrame != null)
                return ("Controller", GetActionName(descriptor, context));

            return ("Middleware", GetActionName(descriptor, context));
        }

        private static IEnumerable<StackFrame> EnumerateExceptionFrames(Exception exception)
        {
            for (var current = exception; current != null; current = current.InnerException)
            {
                foreach (var frame in new StackTrace(current, true).GetFrames() ?? Array.Empty<StackFrame>())
                    yield return frame;
            }
        }

        private static Type? GetFrameType(StackFrame frame)
        {
            var type = frame.GetMethod()?.DeclaringType;
            while (type?.DeclaringType != null &&
                   (type.Name.StartsWith("<", StringComparison.Ordinal) ||
                    type.Name.Contains("DisplayClass", StringComparison.Ordinal)))
            {
                type = type.DeclaringType;
            }

            return type;
        }

        private static bool IsServiceRepositoryFrame(StackFrame frame)
        {
            var type = GetFrameType(frame);
            var typeNamespace = type?.Namespace ?? string.Empty;
            var assemblyName = type?.Assembly.GetName().Name ?? string.Empty;
            var fileName = (frame.GetFileName() ?? string.Empty).Replace('\\', '/');

            return typeNamespace.StartsWith("NobatPlusDATA.DataLayer.Services", StringComparison.Ordinal) ||
                   (string.Equals(assemblyName, "NobatPlusDATA", StringComparison.OrdinalIgnoreCase) &&
                    type?.Name.EndsWith("Rep", StringComparison.Ordinal) == true) ||
                   fileName.Contains("/NobatPlusDATA/DataLayer/Services/", StringComparison.OrdinalIgnoreCase);
        }

        private static string? GetRepositoryActionFromExceptionText(Exception exception)
        {
            for (var current = exception; current != null; current = current.InnerException)
            {
                var exceptionText = current.ToString().Replace('\\', '/');
                if (!exceptionText.Contains("NobatPlusDATA.DataLayer.Services.", StringComparison.Ordinal) &&
                    !exceptionText.Contains("/NobatPlusDATA/DataLayer/Services/", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var match = Regex.Match(
                    exceptionText,
                    @"NobatPlusDATA\.DataLayer\.Services\.(?<type>[A-Za-z0-9_]+)\.(?<method>[A-Za-z0-9_]+)",
                    RegexOptions.CultureInvariant);

                if (match.Success)
                    return $"{match.Groups["type"].Value}.{match.Groups["method"].Value}";

                return "ServiceRepository.Unknown";
            }

            return null;
        }

        private static bool IsControllerFrame(StackFrame frame)
        {
            var type = GetFrameType(frame);
            var typeNamespace = type?.Namespace ?? string.Empty;
            var assemblyName = type?.Assembly.GetName().Name ?? string.Empty;

            return typeNamespace.StartsWith("NobatPlusAPI.Controllers", StringComparison.Ordinal) ||
                   (string.Equals(assemblyName, "NobatPlusAPI", StringComparison.OrdinalIgnoreCase) &&
                    type?.Name.EndsWith("Controller", StringComparison.Ordinal) == true);
        }

        private static string GetFrameActionName(StackFrame frame)
        {
            var method = frame.GetMethod();
            var generatedType = method?.DeclaringType;
            var logicalType = generatedType?.DeclaringType ?? generatedType;
            var methodName = method?.Name ?? "Unknown";

            if (generatedType != null &&
                generatedType.Name.StartsWith("<", StringComparison.Ordinal) &&
                generatedType.Name.Contains('>'))
            {
                methodName = generatedType.Name[1..generatedType.Name.IndexOf('>')];
            }

            return $"{logicalType?.Name ?? "Unknown"}.{methodName}";
        }

        private static string BuildControlledErrorDescription(
            HttpContext context,
            string errorMessage,
            (string Layer, string ActionName, string StackTrace) origin)
        {
            return new JObject
            {
                ["traceId"] = context.TraceIdentifier,
                ["occurredAt"] = DateTimeOffset.Now.ToString("O"),
                ["origin"] = new JObject
                {
                    ["layer"] = origin.Layer,
                    ["action"] = origin.ActionName
                },
                ["request"] = BuildRequestDetails(context, origin.ActionName),
                ["error"] = new JObject
                {
                    ["kind"] = "ControlledError",
                    ["message"] = errorMessage,
                    ["statusCode"] = context.Response.StatusCode,
                    ["capturedStackTrace"] = origin.StackTrace
                }
            }.ToString(Formatting.None);
        }

        internal static string BuildExceptionDescription(
            HttpContext context,
            Exception exception,
            (string Layer, string ActionName) origin)
        {
            var detectedFrames = string.Join(
                "\n",
                EnumerateExceptionFrames(exception)
                    .Select(frame =>
                    {
                        var method = frame.GetMethod();
                        var type = GetFrameType(frame);
                        return $"{type?.Assembly.GetName().Name ?? "?"} | " +
                               $"{type?.FullName ?? method?.DeclaringType?.FullName ?? "?"}." +
                               $"{method?.Name ?? "?"}";
                    })
                    .Take(30));

            return new JObject
            {
                ["traceId"] = context.TraceIdentifier,
                ["occurredAt"] = DateTimeOffset.Now.ToString("O"),
                ["origin"] = new JObject
                {
                    ["layer"] = origin.Layer,
                    ["action"] = origin.ActionName,
                    ["detectedFrames"] = detectedFrames
                },
                ["request"] = BuildRequestDetails(context, origin.ActionName),
                ["exception"] = BuildExceptionDetails(exception)
            }.ToString(Formatting.None);
        }

        internal static void SetActionArguments(HttpContext context, IDictionary<string, object?> arguments)
        {
            try
            {
                var serializer = JsonSerializer.Create(new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MaxDepth = 10,
                    Error = (_, args) => args.ErrorContext.Handled = true
                });
                var token = JToken.FromObject(arguments, serializer);
                context.Items[RequestInputItemKey] = SanitizeForLogging(token);
            }
            catch
            {
                // The request-body snapshot captured by middleware remains available.
            }
        }

        internal static void AddRepositoryInvocation(
            HttpContext context,
            string actionName,
            IReadOnlyDictionary<string, object?> arguments)
        {
            var argumentToken = ConvertToLogToken(arguments);
            lock (context.Items)
            {
                if (!context.Items.TryGetValue(RepositoryInvocationsItemKey, out var value) ||
                    value is not List<RepositoryInvocationSnapshot> invocations)
                {
                    invocations = new List<RepositoryInvocationSnapshot>();
                    context.Items[RepositoryInvocationsItemKey] = invocations;
                }

                invocations.Add(new RepositoryInvocationSnapshot(
                    actionName,
                    argumentToken,
                    DateTimeOffset.Now));
            }
        }

        private static JObject BuildRequestDetails(HttpContext context, string originActionName)
        {
            var routeValues = JObject.FromObject(
                context.Request.RouteValues.ToDictionary(x => x.Key, x => x.Value?.ToString()));
            var query = new JObject();
            foreach (var item in context.Request.Query)
                query[item.Key] = item.Value.Count <= 1
                    ? item.Value.ToString()
                    : new JArray(item.Value.Select(x => x));

            var inputModel = context.Items.TryGetValue(RequestInputItemKey, out var input) && input is JToken token
                ? token.DeepClone()
                : JValue.CreateNull();
            var functionInput = GetRepositoryFunctionInput(context, originActionName);

            return new JObject
            {
                ["method"] = context.Request.Method,
                ["path"] = context.Request.Path.Value,
                ["query"] = SanitizeForLogging(query),
                ["routeValues"] = SanitizeForLogging(routeValues),
                ["inputModel"] = inputModel,
                ["functionInput"] = functionInput,
                ["contentType"] = context.Request.ContentType,
                ["remoteIp"] = context.Connection.RemoteIpAddress?.ToString(),
                ["userId"] = context.User?.Identity?.IsAuthenticated == true
                    ? context.User.GetCurrentUserId()
                    : null
            };
        }

        private static JToken GetRepositoryFunctionInput(HttpContext context, string originActionName)
        {
            if (!context.Items.TryGetValue(RepositoryInvocationsItemKey, out var value) ||
                value is not List<RepositoryInvocationSnapshot> invocations)
            {
                return JValue.CreateNull();
            }

            RepositoryInvocationSnapshot? invocation;
            lock (context.Items)
            {
                invocation = invocations.LastOrDefault(x =>
                    string.Equals(x.ActionName, originActionName, StringComparison.OrdinalIgnoreCase));
                invocation ??= invocations.LastOrDefault();
            }

            if (invocation == null)
                return JValue.CreateNull();

            return new JObject
            {
                ["function"] = invocation.ActionName,
                ["parameters"] = invocation.Arguments.DeepClone(),
                ["capturedAt"] = invocation.CapturedAt.ToString("O")
            };
        }

        private static JObject BuildExceptionDetails(Exception exception)
        {
            var result = new JObject
            {
                ["type"] = exception.GetType().FullName,
                ["message"] = exception.Message,
                ["source"] = exception.Source,
                ["targetSite"] = exception.TargetSite?.ToString(),
                ["stackTrace"] = exception.StackTrace,
                ["hResult"] = exception.HResult
            };

            if (exception.Data.Count > 0)
            {
                var data = new JObject();
                foreach (System.Collections.DictionaryEntry item in exception.Data)
                    data[item.Key?.ToString() ?? "null"] = item.Value?.ToString();
                result["data"] = SanitizeForLogging(data);
            }

            if (exception.InnerException != null)
                result["innerException"] = BuildExceptionDetails(exception.InnerException);

            return result;
        }

        internal static JToken SanitizeForLogging(JToken token)
        {
            if (token is JObject obj)
            {
                foreach (var property in obj.Properties().ToList())
                {
                    if (IsSensitiveName(property.Name))
                        property.Value = "***";
                    else
                        property.Value = SanitizeForLogging(property.Value);
                }
            }
            else if (token is JArray array)
            {
                for (var index = 0; index < array.Count; index++)
                    array[index] = SanitizeForLogging(array[index]);
            }

            return token;
        }

        internal static JToken ConvertToLogToken(object? value)
        {
            if (value == null)
                return JValue.CreateNull();

            try
            {
                var serializer = JsonSerializer.Create(new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MaxDepth = 10,
                    Error = (_, args) => args.ErrorContext.Handled = true
                });
                return SanitizeForLogging(JToken.FromObject(value, serializer));
            }
            catch
            {
                return value.ToString() ?? value.GetType().FullName ?? "Unknown";
            }
        }

        private static bool IsSensitiveName(string name)
        {
            return new[]
            {
                "password", "passwordHash", "token", "refreshToken", "accessToken",
                "securityStamp", "authorization", "apiKey", "secret", "cardNumber", "cvv"
            }.Any(key => name.Contains(key, StringComparison.OrdinalIgnoreCase));
        }

        internal sealed record RepositoryInvocationSnapshot(
            string ActionName,
            JToken Arguments,
            DateTimeOffset CapturedAt);
    }

    public class RepositoryLoggingDispatchProxy : DispatchProxy
    {
        private object _target = null!;
        private IHttpContextAccessor _httpContextAccessor = null!;

        internal static object Create(
            Type serviceType,
            object target,
            IHttpContextAccessor httpContextAccessor)
        {
            var proxy = (RepositoryLoggingDispatchProxy)DispatchProxy.Create(
                serviceType,
                typeof(RepositoryLoggingDispatchProxy));
            proxy._target = target;
            proxy._httpContextAccessor = httpContextAccessor;
            return proxy;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod == null)
                throw new InvalidOperationException("Repository proxy could not resolve the invoked method.");

            var invocationArguments = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            var parameters = targetMethod.GetParameters();
            for (var index = 0; index < parameters.Length; index++)
            {
                invocationArguments[parameters[index].Name ?? $"arg{index}"] =
                    args != null && index < args.Length ? args[index] : null;
            }

            var implementationMethod = ResolveImplementationMethod(targetMethod);
            var actionName = $"{_target.GetType().Name}.{implementationMethod.Name}";
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                GlobalExceptionHandlingMiddleware.AddRepositoryInvocation(
                    httpContext,
                    actionName,
                    invocationArguments);
            }

            try
            {
                return implementationMethod.Invoke(_target, args);
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw;
            }
        }

        private MethodInfo ResolveImplementationMethod(MethodInfo interfaceMethod)
        {
            var parameterTypes = interfaceMethod.GetParameters()
                .Select(parameter => parameter.ParameterType)
                .ToArray();
            return _target.GetType().GetMethod(interfaceMethod.Name, parameterTypes) ?? interfaceMethod;
        }
    }

    public static class RepositoryLoggingServiceCollectionExtensions
    {
        public static IServiceCollection DecorateRepositoriesForErrorLogging(
            this IServiceCollection services)
        {
            var repositoryDescriptors = services
                .Where(descriptor =>
                    descriptor.ServiceType.IsInterface &&
                    descriptor.ServiceType.Name.EndsWith("Rep", StringComparison.Ordinal) &&
                    descriptor.ImplementationType != null)
                .ToList();

            foreach (var descriptor in repositoryDescriptors)
            {
                services.Remove(descriptor);
                services.Add(new ServiceDescriptor(
                    descriptor.ServiceType,
                    serviceProvider =>
                    {
                        var target = ActivatorUtilities.CreateInstance(
                            serviceProvider,
                            descriptor.ImplementationType!);
                        var accessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                        return RepositoryLoggingDispatchProxy.Create(
                            descriptor.ServiceType,
                            target,
                            accessor);
                    },
                    descriptor.Lifetime));
            }

            return services;
        }
    }

    public sealed class ApiExceptionLoggingFilter : IAsyncExceptionFilter, IAsyncActionFilter
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<ApiExceptionLoggingFilter> _logger;

        public ApiExceptionLoggingFilter(
            IServiceScopeFactory scopeFactory,
            IHostEnvironment environment,
            ILogger<ApiExceptionLoggingFilter> logger)
        {
            _scopeFactory = scopeFactory;
            _environment = environment;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            GlobalExceptionHandlingMiddleware.SetActionArguments(
                context.HttpContext,
                context.ActionArguments);
            await next();
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var origin = GlobalExceptionHandlingMiddleware.GetExceptionOrigin(
                context.Exception,
                descriptor,
                context.HttpContext);
            var logId = await SaveErrorAsync(context, origin);

            var response = new JObject
            {
                ["status"] = false,
                ["errorMessage"] = _environment.IsDevelopment()
                    ? context.Exception.GetBaseException().Message
                    : "خطای داخلی سرور رخ داد."
            };

            if (logId > 0)
                response["logId"] = logId;

            context.Result = new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            context.ExceptionHandled = true;
        }

        private async Task<long> SaveErrorAsync(
            ExceptionContext context,
            (string Layer, string ActionName) origin)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<NobatPlusContext>();
                var now = DateTime.Now.ToShamsi();
                var log = new Domains.Log
                {
                    CreateDate = now,
                    UpdateDate = now,
                    LogTime = now,
                    ActionName = origin.ActionName,
                    LogType = "ERROR",
                    LogLayer = origin.Layer,
                    Description = GlobalExceptionHandlingMiddleware.BuildExceptionDescription(
                        context.HttpContext,
                        context.Exception,
                        origin),
                    IsActive = true
                };

                await dbContext.Logs.AddAsync(log);
                await dbContext.SaveChangesAsync();
                return log.ID;
            }
            catch (Exception loggingException)
            {
                _logger.LogError(loggingException, "Could not persist MVC exception log");
                return 0;
            }
        }
    }

    public sealed class HangfireErrorLoggingFilter : JobFilterAttribute, IApplyStateFilter
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<HangfireErrorLoggingFilter> _logger;

        public HangfireErrorLoggingFilter(
            IServiceScopeFactory scopeFactory,
            ILogger<HangfireErrorLoggingFilter> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            if (context.NewState is not FailedState failedState)
                return;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<NobatPlusContext>();
                var now = DateTime.Now.ToShamsi();
                var job = context.BackgroundJob.Job;
                var actionName = $"{job.Type.Name}.{job.Method.Name}";
                var jobArguments = GlobalExceptionHandlingMiddleware.SanitizeForLogging(
                    JToken.FromObject(job.Args ?? Array.Empty<object>()));

                var log = new Domains.Log
                {
                    CreateDate = now,
                    UpdateDate = now,
                    LogTime = now,
                    ActionName = actionName,
                    LogType = "ERROR",
                    LogLayer = "BackgroundJob",
                    Description = new JObject
                    {
                        ["occurredAt"] = DateTimeOffset.Now.ToString("O"),
                        ["origin"] = new JObject
                        {
                            ["layer"] = "BackgroundJob",
                            ["action"] = actionName
                        },
                        ["job"] = new JObject
                        {
                            ["id"] = context.BackgroundJob.Id,
                            ["arguments"] = jobArguments,
                            ["reason"] = failedState.Reason
                        },
                        ["exception"] = JObject.FromObject(new
                        {
                            type = failedState.Exception.GetType().FullName,
                            message = failedState.Exception.Message,
                            stackTrace = failedState.Exception.StackTrace,
                            innerException = failedState.Exception.InnerException?.ToString()
                        })
                    }.ToString(Formatting.None),
                    IsActive = true
                };

                dbContext.Logs.Add(log);
                dbContext.SaveChanges();
            }
            catch (Exception loggingException)
            {
                _logger.LogError(
                    loggingException,
                    "Could not persist Hangfire error log for job {JobId}",
                    context.BackgroundJob.Id);
            }
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
        }
    }
}
