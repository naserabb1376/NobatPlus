using System.ComponentModel.DataAnnotations;

namespace Domains
{
    public class BaseEntity
    {
        [Key]
        [Display(Name = "آیدی")]
        public int ID { get; set; }

        [Display(Name = "تاریخ ساخت")]
        public DateTime? CreateDate { get; set; }

        [Display(Name = "تاریخ ساخت")]
        public DateTime? UpdateDate { get; set; }

        [Display(Name = "توضیحات")]
        public String? Description { get; set; }
    }
}