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
    public class LoginRep : ILoginRep
    {

        private NobatPlusContext _context;
        public LoginRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task<BitResultObject> AddLoginAsync(Login Login)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Logins.Add(Login);
                await _context.SaveChangesAsync();
                _context.Entry(Login).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> EditLoginAsync(Login Login)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Logins.Update(Login);
                await _context.SaveChangesAsync();
                _context.Entry(Login).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> ExistLoginAsync(long LoginId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.Logins
                .AsNoTracking()
                .AnyAsync(x => x.ID == LoginId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<ListResultObject<Login>> GetAllLoginsAsync(long personId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<Login> results = new ListResultObject<Login>();
            try
            {
                IQueryable<Login> query;

                if (personId == 0)
                {
                    query = _context.Logins
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
                        );
                }
                else
                {
                    query = _context.Logins
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

        public async Task<RowResultObject<Login>> GetLoginByIdAsync(long LoginId)
        {
            RowResultObject<Login> result = new RowResultObject<Login>();
            try
            {
                result.Result = await _context.Logins
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == LoginId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemoveLoginAsync(Login Login)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Logins.Remove(Login);
                await _context.SaveChangesAsync();
                _context.Entry(Login).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemoveLoginAsync(long LoginId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var Login = await GetLoginByIdAsync(LoginId);
                result = await RemoveLoginAsync(Login.Result);
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
