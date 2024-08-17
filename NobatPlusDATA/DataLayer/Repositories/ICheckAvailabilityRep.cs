using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface ICheckAvailabilityRep
    {
        public Task<List<CheckAvailability>> GetAllCheckAvailabilitiesAsync(long stylistId = -1, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<CheckAvailability> GetCheckAvailabilityByIdAsync(long CheckAvailabilityId);
        public Task AddCheckAvailabilityAsync(CheckAvailability CheckAvailability);
        public Task EditCheckAvailabilityAsync(CheckAvailability CheckAvailability);
        public Task RemoveCheckAvailabilityAsync(CheckAvailability CheckAvailability);
        public Task RemoveCheckAvailabilityAsync(long CheckAvailabilityId);
        public Task<bool> ExistCheckAvailabilityAsync(long CheckAvailabilityId);
    }
}
