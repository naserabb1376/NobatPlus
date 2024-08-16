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
        public List<CheckAvailability> GetAllCheckAvailabilities(long stylistId = -1,int pageIndex = 1,int pageSize = 20, string searchText ="");
        public CheckAvailability GetCheckAvailabilityById(long CheckAvailabilityId);
        public void AddCheckAvailability(CheckAvailability CheckAvailability);
        public void EditCheckAvailability(CheckAvailability CheckAvailability);
        public void RemoveCheckAvailability(CheckAvailability CheckAvailability);
        public void RemoveCheckAvailability(long CheckAvailabilityId);
        public bool ExistCheckAvailability(long CheckAvailabilityId);
    }
}
