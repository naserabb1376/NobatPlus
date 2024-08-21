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
        public string ActionName { get; set; }
    }
}