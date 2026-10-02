using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IStylistScheduleBlockRep
    {
        Task<ListResultObject<StylistScheduleBlockDTO>> GetAllStylistScheduleBlocksAsync(long stylistId, DateTime? fromDate = null, DateTime? toDate = null, long serviceManagementId = 0, long bookingTagId = 0, string status = "", long customerId = 0, long discountId = 0, int pageIndex = 1, int pageSize = 100, string searchText = "", string sortQuery = "");
        Task<RowResultObject<StylistScheduleBlockDTO>> GetStylistScheduleBlockByIdAsync(long id, long customerId = 0, long discountId = 0);
        Task<BitResultObject> AddStylistScheduleBlocksAsync(List<StylistScheduleBlock> blocks);
        Task<BitResultObject> EditStylistScheduleBlockAsync(StylistScheduleBlock block);
        Task<BitResultObject> RemoveStylistScheduleBlockAsync(long id);
    }
}
