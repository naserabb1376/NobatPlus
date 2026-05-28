using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class BookingServiceOptionValueRep : IBookingServiceOptionValueRep
    {
        private readonly NobatPlusContext _context;

        public BookingServiceOptionValueRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddBookingServiceOptionValuesAsync(List<BookingServiceOptionValue> optionValues)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.BookingServiceOptionValues.AddRangeAsync(optionValues);
                await _context.SaveChangesAsync();
                result.ID = optionValues.FirstOrDefault()?.BookingID ?? 0;
                foreach (var item in optionValues) _context.Entry(item).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> EditBookingServiceOptionValuesAsync(List<BookingServiceOptionValue> optionValues)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.BookingServiceOptionValues.UpdateRange(optionValues);
                await _context.SaveChangesAsync();
                result.ID = optionValues.FirstOrDefault()?.BookingID ?? 0;
                foreach (var item in optionValues) _context.Entry(item).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveBookingServiceOptionValuesAsync(List<BookingServiceOptionValue> optionValues)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.BookingServiceOptionValues.RemoveRange(optionValues);
                await _context.SaveChangesAsync();
                result.ID = optionValues.FirstOrDefault()?.BookingID ?? 0;
                foreach (var item in optionValues) _context.Entry(item).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveBookingServiceOptionValuesAsync(List<(long BookingId, long ServiceManagementId, long ServiceOptionValueId)> ids)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var rows = new List<BookingServiceOptionValue>();
                foreach (var id in ids)
                {
                    var row = await GetBookingServiceOptionValueByIdAsync(id.BookingId, id.ServiceManagementId, id.ServiceOptionValueId);
                    if (row.Result != null) rows.Add(row.Result);
                }

                if (!rows.Any())
                {
                    result.Status = false;
                    result.ErrorMessage = "No matching booking service option values found to remove.";
                    return result;
                }

                result = await RemoveBookingServiceOptionValuesAsync(rows);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> ExistBookingServiceOptionValueAsync(long bookingId, long serviceManagementId, long serviceOptionValueId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.BookingServiceOptionValues.AsNoTracking()
                    .AnyAsync(x => x.BookingID == bookingId && x.ServiceManagementID == serviceManagementId && x.ServiceOptionValueID == serviceOptionValueId);
                result.ID = bookingId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<BookingServiceOptionValue>> GetAllBookingServiceOptionValuesAsync(long bookingId = 0, long serviceManagementId = 0, long serviceOptionValueId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "")
        {
            ListResultObject<BookingServiceOptionValue> results = new ListResultObject<BookingServiceOptionValue>();
            try
            {
                var query = _context.BookingServiceOptionValues
                    .Include(x => x.BookingService).ThenInclude(x => x.Booking)
                    .Include(x => x.BookingService).ThenInclude(x => x.ServiceManagement)
                    .Include(x => x.ServiceOptionValue).ThenInclude(x => x.ServiceOption)
                    .AsNoTracking();

                if (bookingId > 0) query = query.Where(x => x.BookingID == bookingId);
                if (serviceManagementId > 0) query = query.Where(x => x.ServiceManagementID == serviceManagementId);
                if (serviceOptionValueId > 0) query = query.Where(x => x.ServiceOptionValueID == serviceOptionValueId);

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        x.BookingService.ServiceManagement.ServiceName.Contains(searchText) ||
                        x.ServiceOptionValue.ValueName.Contains(searchText) ||
                        x.ServiceOptionValue.ServiceOption.OptionName.Contains(searchText));
                }

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.BookingID).SortBy(sortQuery).ToPaging(pageIndex, pageSize).ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
        }

        public async Task<RowResultObject<BookingServiceOptionValue>> GetBookingServiceOptionValueByIdAsync(long bookingId, long serviceManagementId, long serviceOptionValueId)
        {
            RowResultObject<BookingServiceOptionValue> result = new RowResultObject<BookingServiceOptionValue>();
            try
            {
                result.Result = await _context.BookingServiceOptionValues
                    .Include(x => x.BookingService).ThenInclude(x => x.Booking)
                    .Include(x => x.BookingService).ThenInclude(x => x.ServiceManagement)
                    .Include(x => x.ServiceOptionValue).ThenInclude(x => x.ServiceOption)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(x => x.BookingID == bookingId && x.ServiceManagementID == serviceManagementId && x.ServiceOptionValueID == serviceOptionValueId);
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
