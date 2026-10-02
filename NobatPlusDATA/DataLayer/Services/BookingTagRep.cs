using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class BookingTagRep : IBookingTagRep
    {
        private readonly NobatPlusContext _context;

        public BookingTagRep(NobatPlusContext context) => _context = context;

        public async Task<ListResultObject<BookingTag>> GetAllBookingTagsAsync(long stylistId = 0, int isActive = -1, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "")
        {
            var result = new ListResultObject<BookingTag>();
            try
            {
                var query = _context.BookingTags.AsNoTracking().AsQueryable();
                if (stylistId > 0) query = query.Where(x => x.StylistID == stylistId);
                if (isActive >= 0) query = query.Where(x => x.IsActive == (isActive == 1));
                if (!string.IsNullOrWhiteSpace(searchText)) query = query.Where(x => x.Title.Contains(searchText));

                result.TotalCount = await query.CountAsync();
                result.PageCount = DbTools.GetPageCount(result.TotalCount, pageSize);
                result.Results = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.ID)
                    .SortBy(sortQuery).ToPaging(pageIndex, pageSize).ToListAsync();
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<RowResultObject<BookingTag>> GetBookingTagByIdAsync(long id)
        {
            var result = new RowResultObject<BookingTag>();
            try { result.Result = await _context.BookingTags.AsNoTracking().SingleOrDefaultAsync(x => x.ID == id); }
            catch (Exception ex) { result.Status = false; result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}"; }
            return result;
        }

        public async Task<BitResultObject> AddBookingTagAsync(BookingTag tag)
        {
            var result = new BitResultObject();
            try
            {
                var error = await ValidateAsync(tag);
                if (error != null) return Fail(result, error);
                await _context.BookingTags.AddAsync(tag);
                await _context.SaveChangesAsync();
                result.ID = tag.ID;
            }
            catch (Exception ex) { return Fail(result, $"{ex.Message} - {ex.InnerException?.Message}"); }
            return result;
        }

        public async Task<BitResultObject> EditBookingTagAsync(BookingTag tag)
        {
            var result = new BitResultObject();
            try
            {
                if (!await _context.BookingTags.AnyAsync(x => x.ID == tag.ID)) return Fail(result, "برچسب یافت نشد.");
                var error = await ValidateAsync(tag, tag.ID);
                if (error != null) return Fail(result, error);
                _context.BookingTags.Update(tag);
                await _context.SaveChangesAsync();
                result.ID = tag.ID;
            }
            catch (Exception ex) { return Fail(result, $"{ex.Message} - {ex.InnerException?.Message}"); }
            return result;
        }

        public async Task<BitResultObject> RemoveBookingTagAsync(long id)
        {
            var result = new BitResultObject();
            try
            {
                var row = await _context.BookingTags.SingleOrDefaultAsync(x => x.ID == id);
                if (row == null) return Fail(result, "برچسب یافت نشد.");
                if (await _context.StylistServicePriceVariants.AnyAsync(x => x.BookingTagID == id) ||
                    await _context.StylistScheduleBlocks.AnyAsync(x => x.BookingTagID == id))
                    return Fail(result, "این برچسب در قیمت‌های متغیر یا برنامه زمانی استفاده شده و قابل حذف نیست؛ آن را غیرفعال کنید.");
                _context.BookingTags.Remove(row);
                await _context.SaveChangesAsync();
                result.ID = id;
            }
            catch (Exception ex) { return Fail(result, $"{ex.Message} - {ex.InnerException?.Message}"); }
            return result;
        }

        private async Task<string?> ValidateAsync(BookingTag tag, long excludedId = 0)
        {
            tag.Title = tag.Title?.Trim() ?? "";
            tag.Color = tag.Color?.Trim() ?? "";
            if (tag.StylistID <= 0 || !await _context.Stylists.AnyAsync(x => x.ID == tag.StylistID)) return "آرایشگر معتبر نیست.";
            if (string.IsNullOrWhiteSpace(tag.Title)) return "عنوان برچسب الزامی است.";
            if (await _context.BookingTags.AnyAsync(x => x.ID != excludedId && x.StylistID == tag.StylistID && x.Title == tag.Title)) return "این عنوان برای آرایشگر قبلاً ثبت شده است.";
            return null;
        }

        private static BitResultObject Fail(BitResultObject result, string message) { result.Status = false; result.ErrorMessage = message; return result; }
    }
}
