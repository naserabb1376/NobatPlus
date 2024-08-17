using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
    public class StylistRep : IStylistRep
    {

        private NobatPlusContext _context;
        public StylistRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task AddStylistAsync(Stylist Stylist)
        {
            _context.Stylists.Add(Stylist);
            await _context.SaveChangesAsync();
            _context.Entry(Stylist).State = EntityState.Detached;
        }

        public async Task EditStylistAsync(Stylist Stylist)
        {
            _context.Stylists.Update(Stylist);
            await _context.SaveChangesAsync();
            _context.Entry(Stylist).State = EntityState.Detached;
        }

        public async Task<bool> ExistStylistAsync(long StylistId)
        {
            return await _context.Stylists.AnyAsync(x => x.ID == StylistId);
        }

        public async Task<List<Stylist>> GetAllStylistsAsync(long parentId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            IQueryable<Stylist> query = _context.Stylists.Include(x => x.Person).Include(x => x.JobType).AsNoTracking();

            if (parentId < 0)
            {
                query = query.Where(x =>
                    (!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                    (x.Person.DateOfBirth.HasValue && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.JobType.JobTitle) && x.JobType.JobTitle.Contains(searchText)) ||
                    (x.CreateDate.HasValue && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
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
                     (x.Person.DateOfBirth.HasValue && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.JobType.JobTitle) && x.JobType.JobTitle.Contains(searchText)) ||
                     (x.CreateDate.HasValue && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                     (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString().Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.Specialty) && x.Specialty.Contains(searchText))
                    )
                );
            }

            return await query.OrderByDescending(x => x.CreateDate)
                              .ToPaging(pageIndex, pageSize)
                              .ToListAsync();
        }

        public async Task<List<Stylist>> GetStylistsOfServiceAsync(long serviceManagementId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return await _context.StylistServices
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
                    (x.Person.DateOfBirth.HasValue && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.JobType.JobTitle) && x.JobType.JobTitle.Contains(searchText)) ||
                    (x.CreateDate.HasValue && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Specialty) && x.Specialty.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText))
                )
                .OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
        }

        public async Task<List<Stylist>> GetStylistsOfJobTypeAsync(long JobTypeId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return await _context.Stylists
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
                     (x.Person.DateOfBirth.HasValue && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.JobType.JobTitle) && x.JobType.JobTitle.Contains(searchText)) ||
                     (x.CreateDate.HasValue && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                     (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString().Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.Specialty) && x.Specialty.Contains(searchText))
                    )
                )
                .OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
        }

        public async Task<List<Stylist>> GetStylistsOfDiscountAsync(long DiscountId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            List<Stylist> stylists = new List<Stylist>();

            var discountAssignments = _context.DiscountAssignments
                .Where(bs => bs.DiscountId == DiscountId)
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
                    (x.Person.DateOfBirth.HasValue && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.JobType.JobTitle) && x.JobType.JobTitle.Contains(searchText)) ||
                    (x.CreateDate.HasValue && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Specialty) && x.Specialty.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText))
                );

            stylists.AddRange(await discountAssignments.OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToListAsync());

            var serviceDiscounts = _context.ServiceDiscounts
                .Where(bs => bs.DiscountId == DiscountId)
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
                    (x.Person.DateOfBirth.HasValue && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.JobType.JobTitle) && x.JobType.JobTitle.Contains(searchText)) ||
                    (x.CreateDate.HasValue && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Specialty) && x.Specialty.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText))
                );

            stylists.AddRange(await serviceDiscounts.OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToListAsync());

            var customerDiscounts = _context.CustomerDiscounts
                .Where(bs => bs.DiscountId == DiscountId)
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
                    (x.Person.DateOfBirth.HasValue && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.JobType.JobTitle) && x.JobType.JobTitle.Contains(searchText)) ||
                    (x.CreateDate.HasValue && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Specialty) && x.Specialty.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText))
                );

            stylists.AddRange(await customerDiscounts.OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToListAsync());

            return stylists;
        }

        public async Task<Stylist> GetStylistByIdAsync(long StylistId)
        {
            return await _context.Stylists.AsNoTracking().SingleOrDefaultAsync(x => x.ID == StylistId);
        }

        public async Task RemoveStylistAsync(Stylist Stylist)
        {
            _context.Stylists.Remove(Stylist);
            await _context.SaveChangesAsync();
            _context.Entry(Stylist).State = EntityState.Detached;
        }

        public async Task RemoveStylistAsync(long StylistId)
        {
            var stylist = await GetStylistByIdAsync(StylistId);
            if (stylist != null)
            {
                await RemoveStylistAsync(stylist);
            }
        }
    }
}
