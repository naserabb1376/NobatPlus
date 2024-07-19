namespace Domains
{
    public class Log : BaseEntity
    {
        public Log()
        {
            LogTime = DateTime.Now;
        }

        public DateTime LogTime { get; set; }
        public string ActionName { get; set; }
    }
}