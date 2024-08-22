using Domain;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using System;
using System.Collections.Generic;
using System.Data;
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

        public async Task<BitResultObject> AddAdminAsync(Admin admin)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.Admins.AddAsync(admin);
                await _context.SaveChangesAsync();
                _context.Entry(admin).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }

        public async Task<BitResultObject> EditAdminAsync(Admin admin)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Admins.Update(admin);
                await _context.SaveChangesAsync();
                _context.Entry(admin).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> ExistAdminAsync(long adminId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.Admins.AsNoTracking().AnyAsync(x => x.ID == adminId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<Admin>> GetAllAdminsAsync(string role = "",long cityId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<Admin> results = new ListResultObject<Admin>();
            try
            {
                var query = _context.Admins
                .Include(x => x.Person).ThenInclude(x => x.Address).ThenInclude(x => x.City)
                .AsNoTracking()
                .Where(x =>
                    (!string.IsNullOrEmpty(role) && x.Role == role) &&
                    ((!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                    (x.Person.DateOfBirth.HasValue && x.Person.DateOfBirth.Value.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.City.CityName.ToString()) && x.Person.Address.City.CityName.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressLocationHorizentalPoint.ToString()) && x.Person.Address.AddressLocationHorizentalPoint.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressLocationVerticalPoint.ToString()) && x.Person.Address.AddressLocationVerticalPoint.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressPostalCode.ToString()) && x.Person.Address.AddressPostalCode.ToString().Contains(searchText)) ||
                    //(!string.IsNullOrEmpty(x.State.ToString()) && x.State.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.Description.ToString()) && x.Person.Address.Description.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressStreet.ToString()) && x.Person.Address.AddressStreet.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Role) && x.Role.Contains(searchText)) ||
                    (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                    (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                ));
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

        public async Task<ListResultObject<Admin>> GetAdminsOfDiscountAsync(long discountId, long cityId = 0, string role = "", int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<Admin> results = new ListResultObject<Admin>();
            try
            {
                var query1 = _context.DiscountAssignments
                .Where(bs => bs.DiscountId == discountId)
                .Select(bs => bs.Admin)
                .Include(x => x.Person).ThenInclude(x => x.Address).ThenInclude(x => x.City)
                .AsNoTracking()
                .Where(x =>
                    (!string.IsNullOrEmpty(role) && x.Role == role) &&
                    ((!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                    (x.Person.DateOfBirth.HasValue && x.Person.DateOfBirth.Value.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.City.CityName.ToString()) && x.Person.Address.City.CityName.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressLocationHorizentalPoint.ToString()) && x.Person.Address.AddressLocationHorizentalPoint.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressLocationVerticalPoint.ToString()) && x.Person.Address.AddressLocationVerticalPoint.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressPostalCode.ToString()) && x.Person.Address.AddressPostalCode.ToString().Contains(searchText)) ||
                    //(!string.IsNullOrEmpty(x.State.ToString()) && x.State.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.Description.ToString()) && x.Person.Address.Description.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressStreet.ToString()) && x.Person.Address.AddressStreet.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Role) && x.Role.Contains(searchText)) ||
                    (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                    (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))

                ));

                var query2 = _context.ServiceDiscounts
                .Where(bs => bs.DiscountId == discountId)
                .Select(bs => bs.Admin)
                .Include(x => x.Person).ThenInclude(x => x.Address).ThenInclude(x => x.City)
                .AsNoTracking()
                .Where(x =>
                    (!string.IsNullOrEmpty(role) && x.Role == role) &&
                    ((!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                    (x.Person.DateOfBirth.HasValue && x.Person.DateOfBirth.Value.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.City.CityName.ToString()) && x.Person.Address.City.CityName.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressLocationHorizentalPoint.ToString()) && x.Person.Address.AddressLocationHorizentalPoint.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressLocationVerticalPoint.ToString()) && x.Person.Address.AddressLocationVerticalPoint.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressPostalCode.ToString()) && x.Person.Address.AddressPostalCode.ToString().Contains(searchText)) ||
                    //(!string.IsNullOrEmpty(x.State.ToString()) && x.State.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.Description.ToString()) && x.Person.Address.Description.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressStreet.ToString()) && x.Person.Address.AddressStreet.ToString().Contains(searchText)) ||

                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Role) && x.Role.Contains(searchText)) ||
                    (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                    (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))

                ));

                var query = query1.Union(query2);
                if (cityId > 0)
                {
                    query = query.Where(x=> x.Person.Address.CityID == cityId);
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

        public async Task<RowResultObject<Admin>> GetAdminByIdAsync(long adminId)
        {
            RowResultObject<Admin> result = new RowResultObject<Admin>();
            try
            {
                result.Result = await _context.Admins
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == adminId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveAdminAsync(Admin admin)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Admins.Remove(admin);
                await _context.SaveChangesAsync();
                _context.Entry(admin).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveAdminAsync(long adminId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var admin = await GetAdminByIdAsync(adminId);
                result = await RemoveAdminAsync(admin.Result);
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
