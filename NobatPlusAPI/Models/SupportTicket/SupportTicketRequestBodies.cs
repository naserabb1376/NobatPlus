using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.SupportTicket
{
    public class GetSupportTicketListRequestBody : GetListRequestBody
    {
        public long PersonID { get; set; }
        public string Status { get; set; } = "";
        public string Priority { get; set; } = "";
        public string Category { get; set; } = "";
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class AddSupportTicketRequestBody
    {
        public long PersonID { get; set; }

        [Required(ErrorMessage = "لطفا عنوان تیکت را وارد کنید")]
        public string Title { get; set; } = "";

        public string Category { get; set; } = "";
        public string Priority { get; set; } = "normal";
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "لطفا متن پیام را وارد کنید")]
        public string Message { get; set; } = "";
    }

    public class AddSupportTicketMessageRequestBody
    {
        [Range(1, long.MaxValue, ErrorMessage = "شناسه تیکت معتبر نیست")]
        public long TicketID { get; set; }

        [Required(ErrorMessage = "لطفا متن پیام را وارد کنید")]
        public string Message { get; set; } = "";

        public string NextStatus { get; set; } = "";
    }

    public class UpdateSupportTicketStatusRequestBody
    {
        [Range(1, long.MaxValue, ErrorMessage = "شناسه تیکت معتبر نیست")]
        public long TicketID { get; set; }

        [Required(ErrorMessage = "لطفا وضعیت را وارد کنید")]
        public string Status { get; set; } = "";
    }
}
