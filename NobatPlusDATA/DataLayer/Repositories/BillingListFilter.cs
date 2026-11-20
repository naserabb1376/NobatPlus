namespace NobatPlusDATA.DataLayer.Repositories;

public class BillingListFilter
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortQuery { get; set; } = "";
    public string SearchText { get; set; } = "";
    public long BillingAccountID { get; set; }
    public long BusinessID { get; set; }
    public long WorkerID { get; set; }
    public long PaymentID { get; set; }
    public long BookingID { get; set; }
    public long BillingInvoiceID { get; set; }
    public long BillingPaymentID { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
