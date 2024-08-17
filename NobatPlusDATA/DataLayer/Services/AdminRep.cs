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
    public class AdminRep : IAdminRep
    {

        private NobatPlusContext _context;
        public AdminRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task AddAdminAsync(Admin admin)
        {
            await _context.Admins.AddAsync(admin);
            await _context.SaveChangesAsync();
            _context.Entry(admin).State = EntityState.Detached;
        }

        public async Task EditAdminAsync(Admin admin)
        {
            _context.Admins.Update(admin);
            await _context.SaveChangesAsync();
            _context.Entry(admin).State = EntityState.Detached;
        }

        public async Task<bool> ExistAdminAsync(long adminId)
        {
            return await _context.Admins.AsNoTracking().AnyAsync(x => x.ID == adminId);
        }

        public async Task<List<Admin>> GetAllAdminsAsync(int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return await _context.Admins
                .Include(x => x.Person)
                .AsNoTracking()
                .Where(x =>
                    (!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                )
                .OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
        }

        public async Task<List<Admin>> GetAdminsOfDiscountAsync(long discountId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            var admins = new List<Admin>();

            admins.AddRange(await _context.DiscountAssignments
                .Where(bs => bs.DiscountId == discountId)
                .Select(bs => bs.Admin)
                .Include(x => x.Person)
                .AsNoTracking()
                .Where(x =>
                    (!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                )
                .OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync());

            admins.AddRange(await _context.ServiceDiscounts
                .Where(bs => bs.DiscountId == discountId)
                .Select(bs => bs.Admin)
                .Include(x => x.Person)
                .AsNoTracking()
                .Where(x =>
                    (!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                )
                .OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync());

            return admins;
        }

        public async Task<Admin> GetAdminByIdAsync(long adminId)
        {
            return await _context.Admins
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == adminId);
        }

        public async Task RemoveAdminAsync(Admin admin)
        {
            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();
            _context.Entry(admin).State = EntityState.Detached;
        }

        public async Task RemoveAdminAsync(long adminId)
        {
            var admin = await GetAdminByIdAsync(adminId);
            await RemoveAdminAsync(admin);
        }
    }
}
