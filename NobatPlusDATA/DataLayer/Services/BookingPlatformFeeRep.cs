using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
namespace NobatPlusDATA.DataLayer.Services;
public class BookingPlatformFeeRep : BillingReadRep<BookingPlatformFee>, IBookingPlatformFeeRep
{
    public BookingPlatformFeeRep(NobatPlusContext context) : base(context) { }
}

