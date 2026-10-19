using Domains;

namespace NobatPlusDATA.ViewModels
{
    public class BookingTagVM : BaseEntity
    {
        public long WorkerID { get; set; }
        public string Title { get; set; } = "";
        public string Color { get; set; } = "";
        public int SortOrder { get; set; }
    }
}
