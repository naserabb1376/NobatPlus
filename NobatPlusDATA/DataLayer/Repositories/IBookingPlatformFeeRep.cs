using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Repositories;
public interface IBookingPlatformFeeRep
{
    Task<BitResultObject> ExistBookingPlatformFeeAsync(long id, long personId, bool isAdmin);
    Task<ListResultObject<BookingPlatformFee>> GetAllBookingPlatformFeesAsync(BillingListFilter filter, long personId, bool isAdmin);
    Task<RowResultObject<BookingPlatformFee>> GetBookingPlatformFeeByIdAsync(long id, long personId, bool isAdmin);
}

