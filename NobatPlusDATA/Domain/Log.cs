using NobatPlusDATA.Tools;

namespace Domains
{
    public class Log : BaseEntity
    {
        public Log()
        {
            LogTime = DateTime.Now.ToShamsi();
        }

        public DateTime LogTime { get; set; }
        public string ActionName { get; set; } = "";
        public string LogType { get; set; } = "INFO";
        public string LogLayer { get; set; } = "Application";
    }
}
