using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IPaymentBookingRep
    {
        Task<ListResultObject<PaymentBooking>> GetAllPaymentBookingsAsync(long paymentId = 0, long bookingId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "");
        Task<RowResultObject<PaymentBooking>> GetPaymentBookingByIdAsync(long paymentId, long bookingId);
        Task<BitResultObject> AddPaymentBookingsAsync(List<PaymentBooking> paymentBookings);
        Task<BitResultObject> EditPaymentBookingsAsync(List<PaymentBooking> paymentBookings);
        Task<BitResultObject> RemovePaymentBookingsAsync(List<PaymentBooking> paymentBookings);
        Task<BitResultObject> RemovePaymentBookingsAsync(List<(long PaymentId, long BookingId)> paymentBookingIds);
        Task<BitResultObject> ExistPaymentBookingAsync(long paymentId, long bookingId);
    }
}
