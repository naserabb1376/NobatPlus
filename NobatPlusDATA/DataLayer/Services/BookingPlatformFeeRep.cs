using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Services;
public class BookingPlatformFeeRep : BillingReadRep<BookingPlatformFee>, IBookingPlatformFeeRep
{
    public BookingPlatformFeeRep(NobatPlusContext context) : base(context) { }
    public Task<BitResultObject> ExistBookingPlatformFeeAsync(long id, long personId, bool isAdmin) => ExistAsync(id, personId, isAdmin);
    public Task<ListResultObject<BookingPlatformFee>> GetAllBookingPlatformFeesAsync(BillingListFilter filter, long personId, bool isAdmin) => GetAllAsync(filter, personId, isAdmin);
    public Task<RowResultObject<BookingPlatformFee>> GetBookingPlatformFeeByIdAsync(long id, long personId, bool isAdmin) => GetByIdAsync(id, personId, isAdmin);
}

