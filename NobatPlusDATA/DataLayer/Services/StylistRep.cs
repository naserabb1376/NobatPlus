using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
    public class StylistRep : IStylistRep
    {

        private NobatPlusContext _context;
        public StylistRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task<BitResultObject> AddStylistAsync(Stylist Stylist)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.Stylists.AddAsync(Stylist);
                await _context.SaveChangesAsync();
                _context.Entry(Stylist).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> EditStylistAsync(Stylist Stylist)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Stylists.Update(Stylist);
                await _context.SaveChangesAsync();
                _context.Entry(Stylist).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> ExistStylistAsync(long StylistId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.Stylists.AnyAsync(x => x.ID == StylistId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<ListResultObject<Stylist>> GetAllStylistsAsync(long parentId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<Stylist> results = new ListResultObject<Stylist>();
            try
            {
                IQueryable<Stylist> query = _context.Stylists.Include(x => x.Person).Include(x => x.JobType).AsNoTracking();

                if (parentId == 0)
                {
                    query = query.Where(x =>
                        (!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                        (x.Person.DateOfBirth.HasValue && x.Person.DateOfBirth.Value.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.JobType.JobTitle) && x.JobType.JobTitle.Contains(searchText)) ||
                        (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                        (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Specialty) && x.Specialty.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText))
                    );
                }
                else
                {
                    query = query.Where(x =>
                        x.StylistParentID == parentId &&
                        ((!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                         (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                         (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                         (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                         (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                         (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                         (x.Person.DateOfBirth.HasValue && x.Person.DateOfBirth.Value.ToString().Contains(searchText)) ||
                         (!string.IsNullOrEmpty(x.JobType.JobTitle) && x.JobType.JobTitle.Contains(searchText)) ||
                         (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                         (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText)) ||
                         (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString().Contains(searchText)) ||
                         (!string.IsNullOrEmpty(x.Specialty) && x.Specialty.Contains(searchText))
                        )
                    );
                }

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

        public async Task<ListResultObject<Stylist>> GetStylistsOfServiceAsync(long serviceManagementId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<Stylist> results = new ListResultObject<Stylist>();
            try
            {
                var query = _context.StylistServices
                .Where(bs => bs.ServiceManagementID == serviceManagementId)
                .Select(bs => bs.Stylist)
                .Include(x => x.Person)
                .Include(x => x.JobType)
                .AsNoTracking()
                .Where(x =>
                    (!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                    (x.Person.DateOfBirth.HasValue && x.Person.DateOfBirth.Value.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.JobType.JobTitle) && x.JobType.JobTitle.Contains(searchText)) ||
                    (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                    (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Specialty) && x.Specialty.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText))
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

        public async Task<ListResultObject<Stylist>> GetStylistsOfJobTypeAsync(long JobTypeId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<Stylist> results = new ListResultObject<Stylist>();
            try
            {
                var query = _context.Stylists
                .Include(x => x.Person)
                .Include(x => x.JobType)
                .AsNoTracking()
                .Where(x =>
                    x.JobTypeID == JobTypeId &&
                    ((!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                     (x.Person.DateOfBirth.HasValue && x.Person.DateOfBirth.Value.ToString().Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.JobType.JobTitle) && x.JobType.JobTitle.Contains(searchText)) ||
                     (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                     (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString().Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.Specialty) && x.Specialty.Contains(searchText))
                    )
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

        public async Task<ListResultObject<Stylist>> GetStylistsOfDiscountAsync(long DiscountId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<Stylist> results = new ListResultObject<Stylist>();
            try
            {
                var discountAssignments = _context.DiscountAssignments
               .Where(bs => bs.DiscountId == DiscountId)
               .Select(bs => bs.Stylist)
               .Include(x => x.Person)
               .Include(x => x.JobType)
               .AsNoTracking();

                var serviceDiscounts = _context.ServiceDiscounts
             .Where(bs => bs.DiscountId == DiscountId)
             .Select(bs => bs.Stylist)
             .Include(x => x.Person)
             .Include(x => x.JobType)
             .AsNoTracking();

                var customerDiscounts = _context.CustomerDiscounts
               .Where(bs => bs.DiscountId == DiscountId)
               .Select(bs => bs.Stylist)
               .Include(x => x.Person)
               .Include(x => x.JobType)
               .AsNoTracking();

                var query = discountAssignments.Union(serviceDiscounts).Union(customerDiscounts).Where(x =>
                    (!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                    (x.Person.DateOfBirth.HasValue && x.Person.DateOfBirth.Value.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.JobType.JobTitle) && x.JobType.JobTitle.Contains(searchText)) ||
                    (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                    (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Specialty) && x.Specialty.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText))
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

        public async Task<RowResultObject<Stylist>> GetStylistByIdAsync(long StylistId)
        {
            RowResultObject<Stylist> result = new RowResultObject<Stylist>();
            try
            {
                result.Result = await _context.Stylists.AsNoTracking().SingleOrDefaultAsync(x => x.ID == StylistId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }

        public async Task<BitResultObject> RemoveStylistAsync(Stylist Stylist)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Stylists.Remove(Stylist);
                await _context.SaveChangesAsync();
                _context.Entry(Stylist).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> RemoveStylistAsync(long StylistId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var stylist = await GetStylistByIdAsync(StylistId);
                result = await RemoveStylistAsync(stylist.Result);
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
