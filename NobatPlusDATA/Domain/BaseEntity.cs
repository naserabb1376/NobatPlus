using System.ComponentModel.DataAnnotations;

namespace Domains
{
    public class BaseEntity
    {
        [Key]
        [Display(Name = "آیدی")]
        public long ID { get; set; }

        [Display(Name = "تاریخ ساخت")]
        public DateTime? CreateDate { get; set; }

        [Display(Name = "تاریخ ساخت")]
        public DateTime? UpdateDate { get; set; }

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }
    }
}