using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IStylistServiceRep
    {
        public List<StylistService> GetAllStylistServices(int pageIndex = 1,int pageSize = 20, string searchText ="");
        public StylistService GetStylistServiceById(long StylistId, long ServiceManagementId);
        public void AddStylistService(StylistService StylistService);
        public void EditStylistService(StylistService StylistService);
        public void RemoveStylistService(StylistService StylistService);
        public void RemoveStylistService(long StylistId, long ServiceManagementId);
        public bool ExistStylistService(long StylistId, long ServiceManagementId);
    }
}
