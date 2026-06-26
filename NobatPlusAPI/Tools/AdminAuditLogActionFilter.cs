using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.Tools;
using System.Text.Json;

namespace NobatPlusAPI.Tools
{
    public class AdminAuditLogActionFilter : IAsyncActionFilter
    {
        private static readonly HashSet<string> IgnoredControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "AdminAuditLog",
            "Log"
        };

        private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "password",
            "passwordHash",
            "token",
            "refreshToken",
            "accessToken",
            "securityStamp",
            "concurrencyStamp"
        };

        private readonly IAdminAuditLogRep _adminAuditLogRep;

        public AdminAuditLogActionFilter(IAdminAuditLogRep adminAuditLogRep)
        {
            _adminAuditLogRep = adminAuditLogRep;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executedContext = await next();

            if (!ShouldAudit(context))
            {
                return;
            }

            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var controllerName = descriptor?.ControllerName ?? "";
            var actionName = descriptor?.ActionName ?? "";
            var statusCode = executedContext.HttpContext.Response.StatusCode;
            var exception = executedContext.Exception;

            var auditLog = new AdminAuditLog
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                OccurredAt = DateTime.Now.ToShamsi(),
                ActorPersonID = context.HttpContext.User.GetCurrentUserId(),
                ActorFullName = BuildActorName(context),
                ActionName = actionName,
                ControllerName = controllerName,
                EntityName = controllerName,
                HttpMethod = context.HttpContext.Request.Method,
                RequestPath = context.HttpContext.Request.Path.Value ?? "",
                TargetId = ExtractTargetId(context.ActionArguments),
                RequestSummary = BuildRequestSummary(context.ActionArguments),
                StatusCode = statusCode,
                Succeeded = exception == null && statusCode < 400,
                ErrorMessage = exception?.Message,
                IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.HttpContext.Request.Headers.UserAgent.ToString(),
                Description = $"{controllerName}.{actionName}"
            };

            await _adminAuditLogRep.AddAdminAuditLogAsync(auditLog);
        }

        private static bool ShouldAudit(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            if (user?.Identity?.IsAuthenticated != true || user.GetCurrentRoleId() != 4)
            {
                return false;
            }

            var method = context.HttpContext.Request.Method;
            if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method))
            {
                return false;
            }

            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var controllerName = descriptor?.ControllerName ?? "";
            if (IgnoredControllers.Contains(controllerName))
            {
                return false;
            }

            return true;
        }

        private static string BuildActorName(ActionExecutingContext context)
        {
            var firstName = context.HttpContext.User.FindFirst("FirstName")?.Value ?? "";
            var lastName = context.HttpContext.User.FindFirst("LastName")?.Value ?? "";
            var fullName = $"{firstName} {lastName}".Trim();
            return string.IsNullOrWhiteSpace(fullName) ? $"Person #{context.HttpContext.User.GetCurrentUserId()}" : fullName;
        }

        private static string? ExtractTargetId(IDictionary<string, object?> actionArguments)
        {
            foreach (var argument in actionArguments.Values)
            {
                if (argument == null) continue;
                var type = argument.GetType();
                foreach (var name in new[] { "ID", "Id", "id", "PersonID", "PersonId", "SettlementRequestID", "PaymentID", "BookingID" })
                {
                    var property = type.GetProperty(name);
                    var value = property?.GetValue(argument);
                    if (value != null && value.ToString() != "0")
                    {
                        return value.ToString();
                    }
                }
            }
            return null;
        }

        private static string BuildRequestSummary(IDictionary<string, object?> actionArguments)
        {
            var sanitized = actionArguments.ToDictionary(
                pair => pair.Key,
                pair => SanitizeValue(pair.Key, pair.Value));

            var json = JsonSerializer.Serialize(sanitized, new JsonSerializerOptions
            {
                WriteIndented = false,
                MaxDepth = 5
            });

            const int maxLength = 4000;
            return json.Length <= maxLength ? json : json[..maxLength];
        }

        private static object? SanitizeValue(string key, object? value)
        {
            if (value == null) return null;
            if (SensitiveKeys.Any(sensitiveKey => key.Contains(sensitiveKey, StringComparison.OrdinalIgnoreCase)))
            {
                return "***";
            }

            if (value is string text)
            {
                return text.Length <= 500 ? text : text[..500];
            }

            var type = value.GetType();
            if (type.IsPrimitive || value is decimal || value is DateTime || value is Guid)
            {
                return value;
            }

            var result = new Dictionary<string, object?>();
            foreach (var property in type.GetProperties().Where(p => p.CanRead))
            {
                if (SensitiveKeys.Any(sensitiveKey => property.Name.Contains(sensitiveKey, StringComparison.OrdinalIgnoreCase)))
                {
                    result[property.Name] = "***";
                    continue;
                }

                var propertyValue = property.GetValue(value);
                result[property.Name] = propertyValue is string propertyText && propertyText.Length > 500 ? propertyText[..500] : propertyValue;
            }
            return result;
        }
    }
}
