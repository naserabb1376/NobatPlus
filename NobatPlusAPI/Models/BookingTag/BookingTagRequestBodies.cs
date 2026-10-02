using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.BookingTag
{
    public class GetBookingTagListRequestBody : GetListRequestBody
    {
        public long StylistID { get; set; }
        public int IsActive { get; set; } = -1;
    }

    public class AddEditBookingTagRequestBody
    {
        public long ID { get; set; }
        [Range(1, long.MaxValue)] public long StylistID { get; set; }
        [Required] public string Title { get; set; } = "";
        public string Color { get; set; } = "";
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
    }
}
