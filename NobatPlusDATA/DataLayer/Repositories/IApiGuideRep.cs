using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IApiGuideRep
    {
        public Task<ListResultObject<ApiGuide>> GetAllApiGuidesAsync(string guideType ="", int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<RowResultObject<ApiGuide>> GetApiGuideByIdAsync(long ApiGuideId);
        public Task<RowResultObject<ApiGuide>> GetGuideForApiAsync(string apiName,string guideType);
        public Task<BitResultObject> AddApiGuideAsync(ApiGuide ApiGuide);
        public Task<BitResultObject> EditApiGuideAsync(ApiGuide ApiGuide);
        public Task<BitResultObject> RemoveApiGuideAsync(ApiGuide ApiGuide);
        public Task<BitResultObject> RemoveApiGuideAsync(long ApiGuideId);
        public Task<BitResultObject> ExistApiGuideAsync(long ApiGuideId);
    }
}
