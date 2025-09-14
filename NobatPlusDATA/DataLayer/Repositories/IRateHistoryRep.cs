using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IRateHistoryRep
    {
        public Task<ListResultObject<RateHistoryDTO>> GetAllRateHistoriesAsync(long customerId = 0,long stylistId =0,long RateQuestionId =0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<RateHistoryDTO>> GetRateHistoryByIdAsync(long RateHistoryId);
        public Task<BitResultObject> AddRateHistoryAsync(RateHistory RateHistory);
        public Task<BitResultObject> EditRateHistoryAsync(RateHistory RateHistory);
        public Task<BitResultObject> RemoveRateHistoryAsync(RateHistory RateHistory);
        public Task<BitResultObject> RemoveRateHistoryAsync(long RateHistoryId);
        public Task<BitResultObject> ExistRateHistoryAsync(long RateHistoryId);
    }
}
