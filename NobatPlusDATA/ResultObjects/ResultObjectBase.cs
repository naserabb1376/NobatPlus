using System.Diagnostics;

namespace NobatPlusDATA.ResultObjects
{
    public abstract class ResultObjectBase
    {
        private bool _status = true;

        public bool Status
        {
            get => _status;
            set
            {
                _status = value;
                if (!value && string.IsNullOrWhiteSpace(ErrorOriginLayer))
                    CaptureErrorOrigin();
            }
        }

        // These fields are removed by GlobalExceptionHandlingMiddleware before
        // the response is sent. They carry controlled-error origin information.
        public string? ErrorOriginLayer { get; set; }
        public string? ErrorOriginAction { get; set; }
        public string? ErrorOriginStackTrace { get; set; }

        private void CaptureErrorOrigin()
        {
            var frames = new StackTrace(1, true).GetFrames() ?? Array.Empty<StackFrame>();
            var repositoryFrame = frames.FirstOrDefault(frame =>
                GetLogicalType(frame)?.Namespace?.Contains(".DataLayer.Services", StringComparison.Ordinal) == true);

            if (repositoryFrame != null)
            {
                ErrorOriginLayer = "ServiceRepository";
                ErrorOriginAction = GetActionName(repositoryFrame);
            }
            else
            {
                var controllerFrame = frames.FirstOrDefault(frame =>
                    GetLogicalType(frame)?.Namespace?.Contains(".Controllers", StringComparison.Ordinal) == true);

                if (controllerFrame != null)
                {
                    ErrorOriginLayer = "Controller";
                    ErrorOriginAction = GetActionName(controllerFrame);
                }
                else
                {
                    ErrorOriginLayer = "Application";
                    ErrorOriginAction = "Unknown";
                }
            }

            ErrorOriginStackTrace = string.Join(
                Environment.NewLine,
                frames
                    .Where(frame =>
                    {
                        var typeNamespace = GetLogicalType(frame)?.Namespace ?? string.Empty;
                        return typeNamespace.StartsWith("NobatPlus", StringComparison.Ordinal) ||
                               typeNamespace.StartsWith("AITech", StringComparison.Ordinal);
                    })
                    .Take(30)
                    .Select(frame => $"{GetLogicalType(frame)?.FullName ?? "Unknown"}.{GetMethodName(frame)}"));
        }

        private static Type? GetLogicalType(StackFrame frame)
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

        private static string GetActionName(StackFrame frame)
        {
            return $"{GetLogicalType(frame)?.Name ?? "Unknown"}.{GetMethodName(frame)}";
        }

        private static string GetMethodName(StackFrame frame)
        {
            var method = frame.GetMethod();
            var generatedType = method?.DeclaringType;
            if (generatedType != null &&
                generatedType.Name.StartsWith("<", StringComparison.Ordinal) &&
                generatedType.Name.Contains('>'))
            {
                return generatedType.Name[1..generatedType.Name.IndexOf('>')];
            }

            return method?.Name ?? "Unknown";
        }
    }
}
