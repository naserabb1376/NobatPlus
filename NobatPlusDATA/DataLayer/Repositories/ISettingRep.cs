using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface ISettingRep
    {
        Task<ListResultObject<Setting>> GetAllSettingsAsync(long ParentId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "");

        Task<RowResultObject<Setting>> GetSettingRowAsync(long settingId=0,string settingKey="");

        Task<BitResultObject> AddSettingAsync(Setting setting);

        Task<BitResultObject> EditSettingAsync(Setting setting);

        Task<BitResultObject> RemoveSettingAsync(Setting setting);

        Task<BitResultObject> RemoveSettingAsync(long settingId);

        Task<BitResultObject> ExistSettingRowAsync(long settingId = 0, string settingKey = "");
    }
}