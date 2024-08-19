using Domain;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
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

        public async Task<BitResultObject> AddServiceManagementAsync(ServiceManagement ServiceManagement)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.ServiceManagements.Add(ServiceManagement);
                await _context.SaveChangesAsync();
                _context.Entry(ServiceManagement).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> EditServiceManagementAsync(ServiceManagement ServiceManagement)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.ServiceManagements.Update(ServiceManagement);
                await _context.SaveChangesAsync();
                _context.Entry(ServiceManagement).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> ExistServiceManagementAsync(long ServiceManagementId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.ServiceManagements
                .AsNoTracking()
                .AnyAsync(x => x.ID == ServiceManagementId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<ListResultObject<ServiceManagement>> GetAllServiceManagementsAsync(long parentId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<ServiceManagement> results = new ListResultObject<ServiceManagement>();
            try
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

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
           
        }

        public async Task<ListResultObject<ServiceManagement>> GetServicesOfBookingAsync(long bookingId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<ServiceManagement> results = new ListResultObject<ServiceManagement>();
            try
            {
                var query = _context.BookingServices
                .Where(bs => bs.BookingID == bookingId)
                .Select(bs => bs.ServiceManagement)
                .AsNoTracking()
                .Where(x =>
                    (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
                    || (!string.IsNullOrEmpty(x.ServiceName.ToString()) && x.ServiceName.ToString().Contains(searchText))
                    || (!string.IsNullOrEmpty(x.Duration.ToString()) && x.Duration.ToString("HH:mm").Contains(searchText))
                );

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
          
        }

        public async Task<ListResultObject<ServiceManagement>> GetServicesOfStylistAsync(long stylistId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<ServiceManagement> results = new ListResultObject<ServiceManagement>();
            try
            {
                var query = _context.StylistServices
                .Where(bs => bs.StylistID == stylistId)
                .Select(bs => bs.ServiceManagement)
                .AsNoTracking()
                .Where(x =>
                    (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
                    || (!string.IsNullOrEmpty(x.ServiceName.ToString()) && x.ServiceName.ToString().Contains(searchText))
                    || (!string.IsNullOrEmpty(x.Duration.ToString()) && x.Duration.ToString("HH:mm").Contains(searchText))
                );

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
          
        }

        public async Task<ListResultObject<ServiceManagement>> GetServicesOfDiscountAsync(long DiscountId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<ServiceManagement> results = new ListResultObject<ServiceManagement>();
            try
            {
                var query =  _context.ServiceDiscounts
                .Where(bs => bs.DiscountId == DiscountId)
                .Select(bs => bs.ServiceManagement)
                .AsNoTracking()
                .Where(x =>
                    (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
                    || (!string.IsNullOrEmpty(x.ServiceName.ToString()) && x.ServiceName.ToString().Contains(searchText))
                    || (!string.IsNullOrEmpty(x.Duration.ToString()) && x.Duration.ToString("HH:mm").Contains(searchText))
                );

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
           
        }

        public async Task<RowResultObject<ServiceManagement>> GetServiceManagementByIdAsync(long ServiceManagementId)
        {
            RowResultObject<ServiceManagement> result = new RowResultObject<ServiceManagement>();
            try
            {
                result.Result = await _context.ServiceManagements
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == ServiceManagementId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemoveServiceManagementAsync(ServiceManagement ServiceManagement)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.ServiceManagements.Remove(ServiceManagement);
                await _context.SaveChangesAsync();
                _context.Entry(ServiceManagement).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemoveServiceManagementAsync(long ServiceManagementId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var serviceManagement = await GetServiceManagementByIdAsync(ServiceManagementId);
                result = await RemoveServiceManagementAsync(serviceManagement.Result);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
         
        }
    }
}
