using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IBookingServiceOptionValueRep
    {
        Task<ListResultObject<BookingServiceOptionValue>> GetAllBookingServiceOptionValuesAsync(long bookingId = 0, long serviceManagementId = 0, long serviceOptionValueId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "");
        Task<RowResultObject<BookingServiceOptionValue>> GetBookingServiceOptionValueByIdAsync(long bookingId, long serviceManagementId, long serviceOptionValueId);
        Task<BitResultObject> AddBookingServiceOptionValuesAsync(List<BookingServiceOptionValue> optionValues);
        Task<BitResultObject> EditBookingServiceOptionValuesAsync(List<BookingServiceOptionValue> optionValues);
        Task<BitResultObject> RemoveBookingServiceOptionValuesAsync(List<BookingServiceOptionValue> optionValues);
        Task<BitResultObject> RemoveBookingServiceOptionValuesAsync(List<(long BookingId, long ServiceManagementId, long ServiceOptionValueId)> ids);
        Task<BitResultObject> ExistBookingServiceOptionValueAsync(long bookingId, long serviceManagementId, long serviceOptionValueId);
    }
}
