using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.WorkerServicePriceVariant
{
    public class AddEditWorkerServicePriceVariantRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "خدمات دهنده")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long WorkerID { get; set; }

        [Display(Name = "سرویس")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long ServiceManagementID { get; set; }

        public decimal Price { get; set; }
        public TimeSpan Duration { get; set; }
        public int DepositPercent { get; set; }
        public bool IsActive { get; set; } = true;
        public long? BookingTagID { get; set; }
        public List<long> OptionValueIDs { get; set; } = new List<long>();
        public string? Description { get; set; }
    }
}
