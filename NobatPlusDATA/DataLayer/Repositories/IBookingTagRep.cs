using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IBookingTagRep
    {
        Task<ListResultObject<BookingTag>> GetAllBookingTagsAsync(long stylistId = 0, int isActive = -1, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "");
        Task<RowResultObject<BookingTag>> GetBookingTagByIdAsync(long id);
        Task<BitResultObject> AddBookingTagAsync(BookingTag tag);
        Task<BitResultObject> EditBookingTagAsync(BookingTag tag);
        Task<BitResultObject> RemoveBookingTagAsync(long id);
    }
}
