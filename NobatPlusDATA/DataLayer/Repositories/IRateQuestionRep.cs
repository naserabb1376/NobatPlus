using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IRateQuestionRep
    {
        public Task<ListResultObject<RateQuestion>> GetAllRateQuestionsAsync(int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<RateQuestion>> GetRateQuestionByIdAsync(long RateQuestionId);
        public Task<BitResultObject> AddRateQuestionAsync(RateQuestion RateQuestion);
        public Task<BitResultObject> EditRateQuestionAsync(RateQuestion RateQuestion);
        public Task<BitResultObject> RemoveRateQuestionAsync(RateQuestion RateQuestion);
        public Task<BitResultObject> RemoveRateQuestionAsync(long RateQuestionId);
        public Task<BitResultObject> ExistRateQuestionAsync(long RateQuestionId);
    }
}
