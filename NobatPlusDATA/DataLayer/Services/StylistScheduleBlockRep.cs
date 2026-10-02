using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using System.Data;

namespace NobatPlusDATA.DataLayer.Services
{
    public class StylistScheduleBlockRep : IStylistScheduleBlockRep
    {
        private readonly NobatPlusContext _context;

        public StylistScheduleBlockRep(NobatPlusContext context) => _context = context;

        public async Task<ListResultObject<StylistScheduleBlockDTO>> GetAllStylistScheduleBlocksAsync(
            long stylistId, DateTime? fromDate = null, DateTime? toDate = null,
            long serviceManagementId = 0, long bookingTagId = 0, string status = "",
            long customerId = 0, long discountId = 0, int pageIndex = 1, int pageSize = 100,
            string searchText = "", string sortQuery = "")
        {
            var result = new ListResultObject<StylistScheduleBlockDTO>();
            try
            {
                var query = BuildQuery(stylistId, fromDate, toDate, serviceManagementId, bookingTagId, status, searchText);
                result.TotalCount = await query.CountAsync();
                result.PageCount = DbTools.GetPageCount(result.TotalCount, pageSize);

                var rows = await query.OrderBy(x => x.StartDateTime)
                    .SortBy(sortQuery).ToPaging(pageIndex, pageSize).ToListAsync();
                result.Results = new List<StylistScheduleBlockDTO>();
                foreach (var row in rows)
                    result.Results.Add(await MapAsync(row, customerId, discountId));
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<RowResultObject<StylistScheduleBlockDTO>> GetStylistScheduleBlockByIdAsync(long id, long customerId = 0, long discountId = 0)
        {
            var result = new RowResultObject<StylistScheduleBlockDTO>();
            try
            {
                var row = await FullQuery().SingleOrDefaultAsync(x => x.ID == id);
                if (row == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "بازه زمانی یافت نشد.";
                    return result;
                }
                result.Result = await MapAsync(row, customerId, discountId);
            }
            catch (Exception ex) { result.Status = false; result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}"; }
            return result;
        }

        public async Task<BitResultObject> AddStylistScheduleBlocksAsync(List<StylistScheduleBlock> blocks)
        {
            var result = new BitResultObject();
            try
            {
                if (blocks == null || blocks.Count == 0) return Fail(result, "حداقل یک بازه زمانی باید ارسال شود.");
                await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
                var accepted = new List<StylistScheduleBlock>();
                foreach (var block in blocks.OrderBy(x => x.StartDateTime))
                {
                    var error = await ValidateAsync(block, 0, accepted);
                    if (error != null) return Fail(result, error);
                    accepted.Add(block);
                }
                await _context.StylistScheduleBlocks.AddRangeAsync(accepted);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                result.ID = accepted.First().ID;
            }
            catch (Exception ex) { return Fail(result, $"{ex.Message} - {ex.InnerException?.Message}"); }
            return result;
        }

        public async Task<BitResultObject> EditStylistScheduleBlockAsync(StylistScheduleBlock block)
        {
            var result = new BitResultObject();
            try
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
                var old = await _context.StylistScheduleBlocks.SingleOrDefaultAsync(x => x.ID == block.ID);
                if (old == null) return Fail(result, "بازه زمانی یافت نشد.");
                if (await HasActiveBookingAsync(block.ID)) return Fail(result, "بازه رزروشده قابل ویرایش نیست؛ ابتدا نوبت را لغو یا جابه‌جا کنید.");
                var error = await ValidateAsync(block, block.ID);
                if (error != null) return Fail(result, error);

                _context.Entry(old).Property(x => x.RowVersion).OriginalValue = block.RowVersion;
                old.StylistID = block.StylistID;
                old.StartDateTime = block.StartDateTime;
                old.EndDateTime = block.EndDateTime;
                old.ServiceManagementID = block.ServiceManagementID;
                old.StylistServicePriceVariantID = block.StylistServicePriceVariantID;
                old.BookingTagID = block.BookingTagID;
                old.Title = block.Title;
                old.PriceOverride = block.PriceOverride;
                old.DepositPercentOverride = block.DepositPercentOverride;
                old.IsBookable = block.IsBookable;
                old.IsActive = block.IsActive;
                old.Description = block.Description;
                old.UpdateDate = block.UpdateDate;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                result.ID = block.ID;
            }
            catch (DbUpdateConcurrencyException) { return Fail(result, "این بازه توسط کاربر دیگری تغییر کرده است؛ اطلاعات را دوباره دریافت کنید."); }
            catch (Exception ex) { return Fail(result, $"{ex.Message} - {ex.InnerException?.Message}"); }
            return result;
        }

        public async Task<BitResultObject> RemoveStylistScheduleBlockAsync(long id)
        {
            var result = new BitResultObject();
            try
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
                var row = await _context.StylistScheduleBlocks.SingleOrDefaultAsync(x => x.ID == id);
                if (row == null) return Fail(result, "بازه زمانی یافت نشد.");
                if (await HasActiveBookingAsync(id)) return Fail(result, "بازه رزروشده قابل حذف نیست؛ ابتدا نوبت را لغو یا جابه‌جا کنید.");
                _context.StylistScheduleBlocks.Remove(row);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                result.ID = id;
            }
            catch (Exception ex) { return Fail(result, $"{ex.Message} - {ex.InnerException?.Message}"); }
            return result;
        }

        private IQueryable<StylistScheduleBlock> BuildQuery(long stylistId, DateTime? fromDate, DateTime? toDate, long serviceId, long tagId, string status, string searchText)
        {
            var query = FullQuery();
            if (stylistId > 0) query = query.Where(x => x.StylistID == stylistId);
            if (fromDate.HasValue) query = query.Where(x => x.EndDateTime > fromDate.Value);
            if (toDate.HasValue) query = query.Where(x => x.StartDateTime < toDate.Value);
            if (serviceId > 0) query = query.Where(x => x.ServiceManagementID == serviceId);
            if (tagId > 0) query = query.Where(x => x.BookingTagID == tagId);
            if (!string.IsNullOrWhiteSpace(searchText)) query = query.Where(x => x.Title.Contains(searchText) || (x.Description != null && x.Description.Contains(searchText)));

            status = status?.Trim().ToLowerInvariant() ?? "";
            if (status == "available") query = query.Where(x => x.IsActive && x.IsBookable && !x.Bookings.Any(b => !b.IsCancelled));
            else if (status == "reserved") query = query.Where(x => x.Bookings.Any(b => !b.IsCancelled));
            else if (status == "blocked") query = query.Where(x => !x.IsActive || !x.IsBookable);
            return query;
        }

        private IQueryable<StylistScheduleBlock> FullQuery() => _context.StylistScheduleBlocks.AsNoTracking()
            .Include(x => x.Stylist).ThenInclude(x => x.Person)
            .Include(x => x.ServiceManagement)
            .Include(x => x.BookingTag)
            .Include(x => x.StylistServicePriceVariant).ThenInclude(x => x.OptionValues).ThenInclude(x => x.ServiceOptionValue).ThenInclude(x => x.ServiceOption)
            .Include(x => x.Bookings);

        private async Task<StylistScheduleBlockDTO> MapAsync(StylistScheduleBlock row, long customerId, long discountId)
        {
            var activeBooking = row.Bookings?.Where(x => !x.IsCancelled).OrderByDescending(x => x.ID).FirstOrDefault();
            var pricing = await ResolvePricingAsync(row);
            var discount = row.ServiceManagementID.HasValue
                ? await GetDiscountPercentAsync(row.StylistID, row.ServiceManagementID.Value, customerId, discountId)
                : 0;
            var finalPrice = pricing.Price * (1m - discount / 100m);

            return new StylistScheduleBlockDTO
            {
                ID = row.ID, CreateDate = row.CreateDate, UpdateDate = row.UpdateDate, Description = row.Description, IsActive = row.IsActive,
                StylistID = row.StylistID,
                StylistName = row.Stylist?.Person == null ? row.Stylist?.StylistName ?? "" : $"{row.Stylist.Person.FirstName} {row.Stylist.Person.LastName}".Trim(),
                StartDateTime = row.StartDateTime, EndDateTime = row.EndDateTime,
                DurationMinutes = Math.Max(0, Convert.ToInt32((row.EndDateTime - row.StartDateTime).TotalMinutes)),
                ServiceManagementID = row.ServiceManagementID, ServiceName = row.ServiceManagement?.ServiceName ?? "",
                StylistServicePriceVariantID = row.StylistServicePriceVariantID,
                BookingTagID = row.BookingTagID, BookingTagTitle = row.BookingTag?.Title ?? "", BookingTagColor = row.BookingTag?.Color ?? "",
                Title = row.Title, ServicePrice = pricing.Price, DiscountPercent = discount, PriceAfterDiscount = finalPrice,
                DepositPercent = pricing.DepositPercent, IsBookable = row.IsBookable,
                Status = activeBooking != null ? "reserved" : row.IsActive && row.IsBookable ? "available" : "blocked",
                BookingID = activeBooking?.ID,
                RowVersion = Convert.ToBase64String(row.RowVersion ?? Array.Empty<byte>()),
                OptionValueIDs = pricing.OptionValueIDs,
                OptionSummary = pricing.OptionSummary
            };
        }

        private async Task<string?> ValidateAsync(StylistScheduleBlock block, long excludedId, IReadOnlyCollection<StylistScheduleBlock>? pending = null)
        {
            block.Title = block.Title?.Trim() ?? "";
            if (block.StylistID <= 0 || !await _context.Stylists.AnyAsync(x => x.ID == block.StylistID)) return "آرایشگر معتبر نیست.";
            if (block.EndDateTime <= block.StartDateTime) return "زمان پایان باید بعد از زمان شروع باشد.";
            if (block.StartDateTime < DateTime.Now.ToShamsi()) return "امکان ثبت بازه در زمان گذشته وجود ندارد.";
            if (block.PriceOverride < 0) return "قیمت اختصاصی نمی‌تواند منفی باشد.";
            if (block.DepositPercentOverride is < 0 or > 100) return "درصد بیعانه باید بین صفر تا صد باشد.";

            var stylist = await _context.Stylists.AsNoTracking().SingleAsync(x => x.ID == block.StylistID);
            var restMinutes = Math.Max(0, Convert.ToInt32(stylist.RestTime.TotalMinutes));
            var workTimes = await _context.WorkTimes.AsNoTracking().Where(x => x.StylistID == block.StylistID).ToListAsync();
            if (!workTimes.Any(x => MatchDayOfWeek(x.DayOfWeek, block.StartDateTime.DayOfWeek) && x.WorkStartTime <= block.StartDateTime.TimeOfDay && x.WorkEndTime >= block.EndDateTime.TimeOfDay))
                return "بازه زمانی خارج از ساعت کاری آرایشگر است.";

            if (await _context.StylistPacifics.AsNoTracking().AnyAsync(x => x.StylistID == block.StylistID && x.PacificStartDate < block.EndDateTime.AddMinutes(restMinutes) && x.PacificEndDate > block.StartDateTime))
                return "آرایشگر در این بازه مرخصی دارد.";

            if (block.ServiceManagementID.HasValue)
            {
                if (!await _context.StylistServices.AnyAsync(x => x.StylistID == block.StylistID && x.ServiceManagementID == block.ServiceManagementID.Value)) return "خدمت برای این آرایشگر تعریف نشده است.";
            }
            if (block.StylistServicePriceVariantID.HasValue)
            {
                var variant = await _context.StylistServicePriceVariants.AsNoTracking().SingleOrDefaultAsync(x => x.ID == block.StylistServicePriceVariantID.Value);
                if (variant == null || !variant.IsActive || variant.StylistID != block.StylistID) return "قیمت متغیر انتخاب‌شده معتبر نیست.";
                if (block.ServiceManagementID.HasValue && variant.ServiceManagementID != block.ServiceManagementID.Value) return "قیمت متغیر متعلق به خدمت انتخاب‌شده نیست.";
                if (block.BookingTagID.HasValue && variant.BookingTagID != block.BookingTagID) return "برچسب بازه با برچسب قیمت متغیر مطابقت ندارد.";
                block.ServiceManagementID ??= variant.ServiceManagementID;
                block.BookingTagID ??= variant.BookingTagID;
            }
            if (block.BookingTagID.HasValue && !await _context.BookingTags.AnyAsync(x => x.ID == block.BookingTagID && x.StylistID == block.StylistID && x.IsActive)) return "برچسب انتخاب‌شده معتبر نیست.";

            var blockEndWithRest = block.EndDateTime.AddMinutes(restMinutes);
            if (await _context.StylistScheduleBlocks.AsNoTracking().AnyAsync(x => x.ID != excludedId && x.StylistID == block.StylistID && x.IsActive && x.StartDateTime < blockEndWithRest && x.EndDateTime.AddMinutes(restMinutes) > block.StartDateTime))
                return "این بازه با برنامه دستی دیگری یا زمان استراحت آن تداخل دارد.";
            if (pending?.Any(x => x.StylistID == block.StylistID && x.StartDateTime < blockEndWithRest && x.EndDateTime.AddMinutes(restMinutes) > block.StartDateTime) == true)
                return "بازه‌های ارسالی با یکدیگر یا زمان استراحت تداخل دارند.";
            return null;
        }

        private async Task<(decimal Price, int DepositPercent, List<long> OptionValueIDs, string OptionSummary)> ResolvePricingAsync(StylistScheduleBlock row)
        {
            if (row.StylistServicePriceVariant != null)
            {
                var options = row.StylistServicePriceVariant.OptionValues?.OrderBy(x => x.ServiceOptionValue.ServiceOption.SortOrder).ThenBy(x => x.ServiceOptionValue.SortOrder).ToList() ?? new();
                return (row.PriceOverride ?? row.StylistServicePriceVariant.Price, row.DepositPercentOverride ?? row.StylistServicePriceVariant.DepositPercent,
                    options.Select(x => x.ServiceOptionValueID).ToList(), string.Join("، ", options.Select(x => $"{x.ServiceOptionValue.ServiceOption.OptionName}: {x.ServiceOptionValue.ValueName}")));
            }
            if (row.ServiceManagementID.HasValue)
            {
                var service = await _context.StylistServices.AsNoTracking().SingleAsync(x => x.StylistID == row.StylistID && x.ServiceManagementID == row.ServiceManagementID.Value);
                return (row.PriceOverride ?? service.ServicePrice, row.DepositPercentOverride ?? service.DepositPercent, new List<long>(), "");
            }
            return (row.PriceOverride ?? 0, row.DepositPercentOverride ?? 0, new List<long>(), "");
        }

        private async Task<int> GetDiscountPercentAsync(long stylistId, long serviceId, long customerId, long discountId)
        {
            var now = DateTime.Now.ToShamsi();
            var service = from sd in _context.ServiceDiscounts join d in _context.Discounts on sd.DiscountId equals d.ID where sd.ServiceManagementId == serviceId && (sd.StylistId == null || sd.StylistId <= 0 || sd.StylistId == stylistId) && d.StartDate <= now && d.EndDate >= now && ((discountId <= 0 && !d.CodeRequired) || (discountId > 0 && d.ID == discountId)) select d.DiscountAmount;
            var customer = from cd in _context.CustomerDiscounts join d in _context.Discounts on cd.DiscountId equals d.ID where customerId > 0 && cd.CustomerId == customerId && (cd.StylistId <= 0 || cd.StylistId == stylistId) && d.StartDate <= now && d.EndDate >= now && ((discountId <= 0 && !d.CodeRequired) || (discountId > 0 && d.ID == discountId)) select d.DiscountAmount;
            var assignment = from da in _context.DiscountAssignments join d in _context.Discounts on da.DiscountId equals d.ID where (da.StylistId == stylistId || ((da.StylistId == null || da.StylistId <= 0) && da.AdminId != null && da.AdminId > 0)) && d.StartDate <= now && d.EndDate >= now && ((discountId <= 0 && !d.CodeRequired) || (discountId > 0 && d.ID == discountId)) select d.DiscountAmount;
            return Math.Clamp(await service.Concat(customer).Concat(assignment).Select(x => (int?)x).MaxAsync() ?? 0, 0, 100);
        }

        private Task<bool> HasActiveBookingAsync(long blockId) => _context.Bookings.AnyAsync(x => x.ScheduleBlockID == blockId && !x.IsCancelled);
        private static BitResultObject Fail(BitResultObject result, string message) { result.Status = false; result.ErrorMessage = message; return result; }
        private static bool MatchDayOfWeek(string dayName, DayOfWeek dayOfWeek)
        {
            var value = (dayName ?? "").Replace("ي", "ی").Replace("ك", "ک").Replace("‌", "").Trim();
            return dayOfWeek switch
            {
                DayOfWeek.Saturday => value.Contains("شنبه") && !value.Contains("یک") && !value.Contains("دو") && !value.Contains("سه") && !value.Contains("چهار") && !value.Contains("پنج"),
                DayOfWeek.Sunday => value.Contains("یکشنبه"), DayOfWeek.Monday => value.Contains("دوشنبه"),
                DayOfWeek.Tuesday => value.Contains("سهشنبه") || value.Contains("سه"), DayOfWeek.Wednesday => value.Contains("چهارشنبه") || value.Contains("چهار"),
                DayOfWeek.Thursday => value.Contains("پنجشنبه") || value.Contains("پنج"), DayOfWeek.Friday => value.Contains("جمعه"), _ => false
            };
        }
    }
}
