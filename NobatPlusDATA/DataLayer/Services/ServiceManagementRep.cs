using Domain;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Services
{
    public class ServiceManagementRep : IServiceManagementRep
    {

        private NobatPlusContext _context;
        public ServiceManagementRep()
        {
            _context = DbTools.GetDbContext();
        }

        public void AddServiceManagement(ServiceManagement ServiceManagement)
        {
            _context.ServiceManagements.Add(ServiceManagement);
            _context.SaveChanges();
            _context.Entry(ServiceManagement).State = EntityState.Detached;
        }

        public void EditServiceManagement(ServiceManagement ServiceManagement)
        {
            _context.ServiceManagements.Update(ServiceManagement);
            _context.SaveChanges();
            _context.Entry(ServiceManagement).State = EntityState.Detached;
        }

        public bool ExistServiceManagement(long ServiceManagementId)
        {
            return _context.ServiceManagements.Any(x => x.ID == ServiceManagementId);
        }

        public List<ServiceManagement> GetAllServiceManagements(long parentId = 0, int pageIndex= 1, int pageSize = 20, string searchText= "")
        {
            if (parentId == 0)
            {
                return _context.ServiceManagements.Where(x =>
           (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.ServiceName.ToString()) && x.ServiceName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Duration.ToString()) && x.Duration.ToString("HH:mm").Contains(searchText))
            ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
            else
            {
                return _context.ServiceManagements.Where(x =>
                (x.ServiceParentID == parentId) &&
         ((!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
         || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
         || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
         || (!string.IsNullOrEmpty(x.ServiceName.ToString()) && x.ServiceName.ToString().Contains(searchText))
         )|| (!string.IsNullOrEmpty(x.Duration.ToString()) && x.Duration.ToString("HH:mm").Contains(searchText))
          ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
        }

        public List<ServiceManagement> GetServicesOfBooking(long bookingId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return _context.BookingServices
           .Where(bs => bs.BookingID == bookingId)
           .Select(bs => bs.ServiceManagement)
           .Where(x =>
            (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.ServiceName.ToString()) && x.ServiceName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Duration.ToString()) && x.Duration.ToString("HH:mm").Contains(searchText))
             ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
        }

        public List<ServiceManagement> GetServicesOfStylist(long stylistId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return _context.StylistServices
           .Where(bs => bs.StylistID == stylistId)
           .Select(bs => bs.ServiceManagement)
           .Where(x =>
            (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.ServiceName.ToString()) && x.ServiceName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Duration.ToString()) && x.Duration.ToString("HH:mm").Contains(searchText))
             ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
        }

        public List<ServiceManagement> GetServicesOfDiscount(long DiscountId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return _context.ServiceDiscounts
           .Where(bs => bs.DiscountId == DiscountId)
           .Select(bs => bs.ServiceManagement)
           .Where(x =>
            (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.ServiceName.ToString()) && x.ServiceName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Duration.ToString()) && x.Duration.ToString("HH:mm").Contains(searchText))
             ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
        }

        public ServiceManagement GetServiceManagementById(long ServiceManagementId)
        {
            return _context.ServiceManagements.Find(ServiceManagementId);
        }

        public void RemoveServiceManagement(ServiceManagement ServiceManagement)
        {
            _context.ServiceManagements.Remove(ServiceManagement);
            _context.SaveChanges();
            _context.Entry(ServiceManagement).State = EntityState.Detached;
        }

        public void RemoveServiceManagement(long ServiceManagementId)
        {
            var ServiceManagement = GetServiceManagementById(ServiceManagementId);
            RemoveServiceManagement(ServiceManagement);
        }
    }
}
