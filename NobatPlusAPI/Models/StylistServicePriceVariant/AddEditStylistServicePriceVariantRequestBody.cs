using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.StylistServicePriceVariant
{
    public class AddEditStylistServicePriceVariantRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "آرایشگر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long StylistID { get; set; }

        [Display(Name = "سرویس")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long ServiceManagementID { get; set; }

        public decimal Price { get; set; }
        public TimeSpan Duration { get; set; }
        public int DepositPercent { get; set; }
        public bool IsActive { get; set; } = true;
        public List<long> OptionValueIDs { get; set; } = new List<long>();
        public string? Description { get; set; }
    }
}
