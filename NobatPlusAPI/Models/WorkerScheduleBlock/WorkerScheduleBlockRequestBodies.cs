using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.WorkerScheduleBlock
{
    public class GetWorkerScheduleBlockListRequestBody : GetListRequestBody
    {
        [Range(1, long.MaxValue)] public long WorkerID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public long ServiceManagementID { get; set; }
        public long BookingTagID { get; set; }
        public long CustomerID { get; set; }
        public long DiscountID { get; set; }
        public string Status { get; set; } = "";
    }

    public class AddWorkerScheduleBlocksRequestBody
    {
        [Required, MinLength(1)] public List<WorkerScheduleBlockItemRequestBody> Blocks { get; set; } = new();
    }

    public class WorkerScheduleBlockItemRequestBody
    {
        public long ID { get; set; }
        [Range(1, long.MaxValue)] public long WorkerID { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public long? ServiceManagementID { get; set; }
        public long? WorkerServicePriceVariantID { get; set; }
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
