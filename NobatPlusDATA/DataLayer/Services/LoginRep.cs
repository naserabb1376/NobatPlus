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
    public class LoginRep : ILoginRep
    {

        private NobatPlusContext _context;
        public LoginRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task AddLoginAsync(Login Login)
        {
            _context.Logins.Add(Login);
            await _context.SaveChangesAsync();
            _context.Entry(Login).State = EntityState.Detached;
        }

        public async Task EditLoginAsync(Login Login)
        {
            _context.Logins.Update(Login);
            await _context.SaveChangesAsync();
            _context.Entry(Login).State = EntityState.Detached;
        }

        public async Task<bool> ExistLoginAsync(long LoginId)
        {
            return await _context.Logins
                .AsNoTracking()
                .AnyAsync(x => x.ID == LoginId);
        }

        public async Task<List<Login>> GetAllLoginsAsync(long personId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            if (personId == 0)
            {
                return await _context.Logins
                    .AsNoTracking()
                    .Include(x => x.Person)
                    .Where(x =>
                        (!string.IsNullOrEmpty(x.Person.FirstName.ToString()) && x.Person.FirstName.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Person.LastName.ToString()) && x.Person.LastName.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Person.NaCode.ToString()) && x.Person.NaCode.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Person.PhoneNumber.ToString()) && x.Person.PhoneNumber.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Person.Email.ToString()) && x.Person.Email.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Person.Description.ToString()) && x.Person.Description.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Username.ToString()) && x.Username.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.LastLoginDate.ToString()) && x.LastLoginDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    )
                    .OrderByDescending(x => x.CreateDate)
                    .ToPaging(pageIndex, pageSize)
                    .ToListAsync();
            }
            else
            {
                return await _context.Logins
                    .AsNoTracking()
                    .Include(x => x.Person)
                    .Where(x =>
                        x.PersonID == personId &&
                        (
                            (!string.IsNullOrEmpty(x.Person.FirstName.ToString()) && x.Person.FirstName.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Person.LastName.ToString()) && x.Person.LastName.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Person.NaCode.ToString()) && x.Person.NaCode.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Person.PhoneNumber.ToString()) && x.Person.PhoneNumber.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Person.Email.ToString()) && x.Person.Email.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Person.Description.ToString()) && x.Person.Description.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Username.ToString()) && x.Username.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.LastLoginDate.ToString()) && x.LastLoginDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        )
                    )
                    .OrderByDescending(x => x.CreateDate)
                    .ToPaging(pageIndex, pageSize)
                    .ToListAsync();
            }
        }

        public async Task<Login> GetLoginByIdAsync(long LoginId)
        {
            return await _context.Logins
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == LoginId);
        }

        public async Task RemoveLoginAsync(Login Login)
        {
            _context.Logins.Remove(Login);
            await _context.SaveChangesAsync();
            _context.Entry(Login).State = EntityState.Detached;
        }

        public async Task RemoveLoginAsync(long LoginId)
        {
            var Login = await GetLoginByIdAsync(LoginId);
            await RemoveLoginAsync(Login);
        }
    }
}
