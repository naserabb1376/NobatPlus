using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class PaymentBookingRep : IPaymentBookingRep
    {
        private readonly NobatPlusContext _context;

        public PaymentBookingRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddPaymentBookingsAsync(List<PaymentBooking> paymentBookings)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.PaymentBookings.AddRangeAsync(paymentBookings);
                await _context.SaveChangesAsync();
                result.ID = paymentBookings.FirstOrDefault()?.PaymentID ?? 0;
                foreach (var item in paymentBookings) _context.Entry(item).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> EditPaymentBookingsAsync(List<PaymentBooking> paymentBookings)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.PaymentBookings.UpdateRange(paymentBookings);
                await _context.SaveChangesAsync();
                result.ID = paymentBookings.FirstOrDefault()?.PaymentID ?? 0;
                foreach (var item in paymentBookings) _context.Entry(item).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemovePaymentBookingsAsync(List<PaymentBooking> paymentBookings)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.PaymentBookings.RemoveRange(paymentBookings);
                await _context.SaveChangesAsync();
                result.ID = paymentBookings.FirstOrDefault()?.PaymentID ?? 0;
                foreach (var item in paymentBookings) _context.Entry(item).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemovePaymentBookingsAsync(List<(long PaymentId, long BookingId)> paymentBookingIds)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var rows = new List<PaymentBooking>();
                foreach (var id in paymentBookingIds)
                {
                    var row = await GetPaymentBookingByIdAsync(id.PaymentId, id.BookingId);
                    if (row.Result != null) rows.Add(row.Result);
                }

                if (!rows.Any())
                {
                    result.Status = false;
                    result.ErrorMessage = "No matching payment bookings found to remove.";
                    return result;
                }

                result = await RemovePaymentBookingsAsync(rows);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> ExistPaymentBookingAsync(long paymentId, long bookingId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.PaymentBookings.AsNoTracking().AnyAsync(x => x.PaymentID == paymentId && x.BookingID == bookingId);
                result.ID = paymentId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<PaymentBooking>> GetAllPaymentBookingsAsync(long paymentId = 0, long bookingId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "")
        {
            ListResultObject<PaymentBooking> results = new ListResultObject<PaymentBooking>();
            try
            {
                var query = _context.PaymentBookings
                    .Include(x => x.Payment)
                    .Include(x => x.Booking).ThenInclude(x => x.Customer).ThenInclude(x => x.Person)
                    .Include(x => x.Booking).ThenInclude(x => x.Stylist).ThenInclude(x => x.Person)
                    .AsNoTracking();

                if (paymentId > 0) query = query.Where(x => x.PaymentID == paymentId);
                if (bookingId > 0) query = query.Where(x => x.BookingID == bookingId);

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        x.Payment.PaymentStatus.Contains(searchText) ||
                        x.Payment.AllPaymentAmount.ToString().Contains(searchText) ||
                        x.Booking.Status.Contains(searchText) ||
                        x.Booking.Customer.Person.FirstName.Contains(searchText) ||
                        x.Booking.Customer.Person.LastName.Contains(searchText) ||
                        x.Booking.Stylist.Person.FirstName.Contains(searchText) ||
                        x.Booking.Stylist.Person.LastName.Contains(searchText));
                }

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.PaymentID).SortBy(sortQuery).ToPaging(pageIndex, pageSize).ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
        }

        public async Task<RowResultObject<PaymentBooking>> GetPaymentBookingByIdAsync(long paymentId, long bookingId)
        {
            RowResultObject<PaymentBooking> result = new RowResultObject<PaymentBooking>();
            try
            {
                result.Result = await _context.PaymentBookings
                    .Include(x => x.Payment)
                    .Include(x => x.Booking).ThenInclude(x => x.Customer).ThenInclude(x => x.Person)
                    .Include(x => x.Booking).ThenInclude(x => x.Stylist).ThenInclude(x => x.Person)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(x => x.PaymentID == paymentId && x.BookingID == bookingId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }
    }
}
