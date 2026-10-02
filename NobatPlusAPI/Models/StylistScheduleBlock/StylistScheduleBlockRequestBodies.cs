using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.StylistScheduleBlock
{
    public class GetStylistScheduleBlockListRequestBody : GetListRequestBody
    {
        [Range(1, long.MaxValue)] public long StylistID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public long ServiceManagementID { get; set; }
        public long BookingTagID { get; set; }
        public long CustomerID { get; set; }
        public long DiscountID { get; set; }
        public string Status { get; set; } = "";
    }

    public class AddStylistScheduleBlocksRequestBody
    {
        [Required, MinLength(1)] public List<StylistScheduleBlockItemRequestBody> Blocks { get; set; } = new();
    }

    public class StylistScheduleBlockItemRequestBody
    {
        public long ID { get; set; }
        [Range(1, long.MaxValue)] public long StylistID { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public long? ServiceManagementID { get; set; }
        public long? StylistServicePriceVariantID { get; set; }
        public long? BookingTagID { get; set; }
        public string Title { get; set; } = "";
        public decimal? PriceOverride { get; set; }
        public int? DepositPercentOverride { get; set; }
        public bool IsBookable { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public string RowVersion { get; set; } = "";
    }
}
