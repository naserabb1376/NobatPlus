using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IServiceManagementRep
    {
        public List<ServiceManagement> GetAllServiceManagements(long parentId = 0,int pageIndex = 1,int pageSize = 20, string searchText ="");
        public List<ServiceManagement> GetServicesOfBooking(long bookingId,int pageIndex = 1,int pageSize = 20, string searchText ="");
        public List<ServiceManagement> GetServicesOfDiscount(long DiscountId,int pageIndex = 1,int pageSize = 20, string searchText ="");
        public List<ServiceManagement> GetServicesOfStylist(long stylistId,int pageIndex = 1,int pageSize = 20, string searchText ="");
        public ServiceManagement GetServiceManagementById(long ServiceManagementId);
        public void AddServiceManagement(ServiceManagement ServiceManagement);
        public void EditServiceManagement(ServiceManagement ServiceManagement);
        public void RemoveServiceManagement(ServiceManagement ServiceManagement);
        public void RemoveServiceManagement(long ServiceManagementId);
        public bool ExistServiceManagement(long ServiceManagementId);
    }
}
