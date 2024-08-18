using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface ICheckAvailabilityRep
    {
        public Task<ListResultObject<CheckAvailability>> GetAllCheckAvailabilitiesAsync(long stylistId = -1, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<RowResultObject<CheckAvailability>> GetCheckAvailabilityByIdAsync(long CheckAvailabilityId);
        public Task<BitResultObject> AddCheckAvailabilityAsync(CheckAvailability CheckAvailability);
        public Task<BitResultObject> EditCheckAvailabilityAsync(CheckAvailability CheckAvailability);
        public Task<BitResultObject> RemoveCheckAvailabilityAsync(CheckAvailability CheckAvailability);
        public Task<BitResultObject> RemoveCheckAvailabilityAsync(long CheckAvailabilityId);
        public Task<BitResultObject> ExistCheckAvailabilityAsync(long CheckAvailabilityId);
    }
}
