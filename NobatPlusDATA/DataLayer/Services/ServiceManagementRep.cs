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
        public ServiceManagementRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddServiceManagementAsync(ServiceManagement ServiceManagement)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.ServiceManagements.AddAsync(ServiceManagement);
                await _context.SaveChangesAsync();
                result.ID = ServiceManagement.ID;
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
                result.ID = ServiceManagement.ID;
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
                result.ID = ServiceManagementId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<ListResultObject<ServiceManagement>> GetAllServiceManagementsAsync(long parentId = 0,long bookingId = 0,long stylistId = 0,long discountId = 0,char serviceGender = ' ',int pageIndex = 1,int pageSize = 20,string searchText = "",string sortQuery = "")
        {
            var results = new ListResultObject<ServiceManagement>();

            try
            {
                IQueryable<ServiceManagement> query;

                // 🟡 تعیین منبع داده
                if (bookingId > 0)
                {
                    query = _context.BookingServices
                        .Where(bs => bs.BookingID == bookingId)
                        .Select(bs => bs.ServiceManagement);
                }
                else if (stylistId > 0)
                {
                    query = _context.StylistServices
                        .Where(ss => ss.StylistID == stylistId)
                        .Select(ss => ss.ServiceManagement);
                }
                else if (discountId > 0)
                {
                    query = _context.ServiceDiscounts
                        .Where(sd => sd.DiscountId == discountId)
                        .Select(sd => sd.ServiceManagement);
                }
                else
                {
                    query = _context.ServiceManagements.AsQueryable();

                    if (parentId > 0)
                    {
                        query = query.Where(x => x.ServiceParentID == parentId);
                    }
                }

                // 🔍 فیلتر سرچ
                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(x =>
                        (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                        (x.UpdateDate .HasValue && x.UpdateDate.Value.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.ServiceName) && x.ServiceName.Contains(searchText))
                    );
                }

                // 🚻 فیلتر جنسیت
                if (serviceGender != ' ')
                {
                    query = query.Where(x => char.ToLower(x.ServiceGender) == char.ToLower(serviceGender));
                }

                // 📊 آمار و صفحه‌بندی
                results.TotalCount = await query.CountAsync();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);

                results.Results = await query
                    .AsNoTracking()
                    .OrderByDescending(x => x.CreateDate)
                    .SortBy(sortQuery)
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
                result.ID = ServiceManagement.ID;
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
