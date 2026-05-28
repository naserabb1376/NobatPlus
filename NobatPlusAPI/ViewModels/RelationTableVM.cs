namespace NobatPlusDATA.ViewModels
{
    public class PaymentBookingVM
    {
        public long PaymentID { get; set; }
        public long BookingID { get; set; }
        public DateTime BookingDate { get; set; }
        public long CustomerID { get; set; }
        public long StylistID { get; set; }
        public string CustomerName { get; set; }
        public string StylistName { get; set; }
        public string PaymentStatus { get; set; }
        public decimal AllPaymentAmount { get; set; }
    }

    public class BookingServiceOptionValueVM
    {
        public long BookingID { get; set; }
        public long ServiceManagementID { get; set; }
        public long ServiceOptionValueID { get; set; }
        public string ServiceName { get; set; }
        public long ServiceOptionID { get; set; }
        public string OptionName { get; set; }
        public string ValueName { get; set; }
    }

    public class StylistServicePriceVariantOptionValueVM
    {
        public long StylistServicePriceVariantID { get; set; }
        public long ServiceOptionValueID { get; set; }
        public long StylistID { get; set; }
        public long ServiceManagementID { get; set; }
        public decimal Price { get; set; }
        public TimeSpan Duration { get; set; }
        public long ServiceOptionID { get; set; }
        public string OptionName { get; set; }
        public string ValueName { get; set; }
    }

    public class PaymentDetailOptionValueVM
    {
        public long PaymentDetailID { get; set; }
        public long ServiceOptionValueID { get; set; }
        public long PaymentID { get; set; }
        public long BookingID { get; set; }
        public long ServiceManagementID { get; set; }
        public string ServiceName { get; set; }
        public long ServiceOptionID { get; set; }
        public string OptionName { get; set; }
        public string ValueName { get; set; }
    }
}
