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

        public async Task AddServiceManagementAsync(ServiceManagement ServiceManagement)
        {
            _context.ServiceManagements.Add(ServiceManagement);
            await _context.SaveChangesAsync();
            _context.Entry(ServiceManagement).State = EntityState.Detached;
        }

        public async Task EditServiceManagementAsync(ServiceManagement ServiceManagement)
        {
            _context.ServiceManagements.Update(ServiceManagement);
            await _context.SaveChangesAsync();
            _context.Entry(ServiceManagement).State = EntityState.Detached;
        }

        public async Task<bool> ExistServiceManagementAsync(long ServiceManagementId)
        {
            return await _context.ServiceManagements
                .AsNoTracking()
                .AnyAsync(x => x.ID == ServiceManagementId);
        }

        public async Task<List<ServiceManagement>> GetAllServiceManagementsAsync(long parentId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            IQueryable<ServiceManagement> query = _context.ServiceManagements
                .AsNoTracking();

            if (parentId != 0)
            {
                query = query.Where(x => x.ServiceParentID == parentId);
            }

            query = query.Where(x =>
                (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
                || (!string.IsNullOrEmpty(x.ServiceName.ToString()) && x.ServiceName.ToString().Contains(searchText))
                || (!string.IsNullOrEmpty(x.Duration.ToString()) && x.Duration.ToString("HH:mm").Contains(searchText))
            );

            return await query
                .OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
        }

        public async Task<List<ServiceManagement>> GetServicesOfBookingAsync(long bookingId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return await _context.BookingServices
                .Where(bs => bs.BookingID == bookingId)
                .Select(bs => bs.ServiceManagement)
                .AsNoTracking()
                .Where(x =>
                    (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
                    || (!string.IsNullOrEmpty(x.ServiceName.ToString()) && x.ServiceName.ToString().Contains(searchText))
                    || (!string.IsNullOrEmpty(x.Duration.ToString()) && x.Duration.ToString("HH:mm").Contains(searchText))
                )
                .OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
        }

        public async Task<List<ServiceManagement>> GetServicesOfStylistAsync(long stylistId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return await _context.StylistServices
                .Where(bs => bs.StylistID == stylistId)
                .Select(bs => bs.ServiceManagement)
                .AsNoTracking()
                .Where(x =>
                    (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
                    || (!string.IsNullOrEmpty(x.ServiceName.ToString()) && x.ServiceName.ToString().Contains(searchText))
                    || (!string.IsNullOrEmpty(x.Duration.ToString()) && x.Duration.ToString("HH:mm").Contains(searchText))
                )
                .OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
        }

        public async Task<List<ServiceManagement>> GetServicesOfDiscountAsync(long DiscountId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return await _context.ServiceDiscounts
                .Where(bs => bs.DiscountId == DiscountId)
                .Select(bs => bs.ServiceManagement)
                .AsNoTracking()
                .Where(x =>
                    (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
                    || (!string.IsNullOrEmpty(x.ServiceName.ToString()) && x.ServiceName.ToString().Contains(searchText))
                    || (!string.IsNullOrEmpty(x.Duration.ToString()) && x.Duration.ToString("HH:mm").Contains(searchText))
                )
                .OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
        }

        public async Task<ServiceManagement> GetServiceManagementByIdAsync(long ServiceManagementId)
        {
            return await _context.ServiceManagements
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == ServiceManagementId);
        }

        public async Task RemoveServiceManagementAsync(ServiceManagement ServiceManagement)
        {
            _context.ServiceManagements.Remove(ServiceManagement);
            await _context.SaveChangesAsync();
            _context.Entry(ServiceManagement).State = EntityState.Detached;
        }

        public async Task RemoveServiceManagementAsync(long ServiceManagementId)
        {
            var serviceManagement = await GetServiceManagementByIdAsync(ServiceManagementId);
            if (serviceManagement != null)
            {
                await RemoveServiceManagementAsync(serviceManagement);
            }
        }
    }
}
