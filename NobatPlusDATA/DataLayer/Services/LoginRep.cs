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
        public LoginRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddLoginAsync(Login Login)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Logins.Add(Login);
                await _context.SaveChangesAsync();
                result.ID = Login.ID;
                _context.Entry(Login).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }

        public async Task<RowResultObject<Login>> AuthenticateAsync(string userName, string password, int authenticateType = 1)
        {
            RowResultObject<Login> result = new RowResultObject<Login>();
            try
            {
                switch (authenticateType)
                {
                    default:
                    case 1:
                        {
                            result.Status = await _context.Logins
               .AsNoTracking()
               .AnyAsync(x => x.Username == userName && x.PasswordHash == password.ToHash());
                            if (result.Status)
                            {
                                var loginRow = await _context.Logins
                            .AsNoTracking().Include(x => x.Person)
                            .SingleOrDefaultAsync(x => x.Username == userName && x.PasswordHash == password.ToHash());
                                loginRow.LastLoginDate = DateTime.Now.ToShamsi();
                                loginRow.UpdateDate = DateTime.Now.ToShamsi();
                                var updateRow = await EditLoginAsync(loginRow);
                                if (updateRow.Status)
                                {
                                    result.Result = loginRow;
                                    result.ErrorMessage = $"احراز هویت موفق بود";
                                }
                                else
                                {
                                    result.Status = updateRow.Status;
                                    result.ErrorMessage = updateRow.ErrorMessage;

                                }

                            }
                            else
                            {
                                result.ErrorMessage = $"احراز هویت ناموفق بود";
                            }
                        }
                        break;
                    case 2:
                        {
                            result.Status = await _context.Logins.Include(x => x.Person)
               .AsNoTracking()
               .AnyAsync(x => x.Person.PhoneNumber == userName);
                            if (result.Status)
                            {
                                var loginRow = await _context.Logins.Include(x => x.Person)
               .AsNoTracking()
               .SingleOrDefaultAsync(x => x.Person.PhoneNumber == userName);
                                loginRow.LastLoginDate = DateTime.Now.ToShamsi();
                                loginRow.UpdateDate = DateTime.Now.ToShamsi();
                                var updateRow = await EditLoginAsync(loginRow);
                                if (updateRow.Status)
                                {
                                    result.Result = loginRow;
                                    result.ErrorMessage = $"احراز هویت موفق بود";
                                }
                                else
                                {
                                    result.Status = updateRow.Status;
                                    result.ErrorMessage = updateRow.ErrorMessage;

                                }

                            }
                            else
                            {
                                result.ErrorMessage = $"احراز هویت ناموفق بود";
                            }
                        }
                        break;
                    case 3:
                        {
                            result.Status = await _context.Logins.Include(x => x.Person)
               .AsNoTracking()
               .AnyAsync(x => x.Person.PhoneNumber == userName && x.PasswordHash == password.ToHash());
                            if (result.Status)
                            {
                                var loginRow = await _context.Logins.Include(x => x.Person)
               .AsNoTracking()
               .SingleOrDefaultAsync(x => x.Person.PhoneNumber == userName && x.PasswordHash == password.ToHash());
                                loginRow.LastLoginDate = DateTime.Now.ToShamsi();
                                loginRow.UpdateDate = DateTime.Now.ToShamsi();
                                var updateRow = await EditLoginAsync(loginRow);
                                if (updateRow.Status)
                                {
                                    result.Result = loginRow;
                                    result.ErrorMessage = $"احراز هویت موفق بود";
                                }
                                else
                                {
                                    result.Status = updateRow.Status;
                                    result.ErrorMessage = updateRow.ErrorMessage;

                                }

                            }
                            else
                            {
                                result.ErrorMessage = $"احراز هویت ناموفق بود";
                            }
                        }
                        break;
                }

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
                result.ID = Login.ID;
                _context.Entry(Login).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }

        public async Task<BitResultObject> ExistLoginAsync(string fieldValue, string fieldName)
        {
            BitResultObject result = new BitResultObject();
            long userId = 0;
            try
            {
                switch (fieldName.ToLower().Trim())
                {
                    case "personid":
                        {
                            var theUser = await _context.Persons.AsNoTracking().FirstOrDefaultAsync(x => x.ID == long.Parse(fieldValue)) ?? new Person();
                            userId = theUser.ID;
                            break;
                        }
                    case "loginid":
                        {
                            var theUser = await _context.Logins.AsNoTracking().FirstOrDefaultAsync(x => x.ID == long.Parse(fieldValue)) ?? new Login();
                            userId = theUser.PersonID;
                            break;
                        }
                    case "username":
                    default:
                        {
                            var theUser = await _context.Logins.AsNoTracking().FirstOrDefaultAsync(x => x.Username == fieldValue) ?? new Login();
                            userId = theUser.PersonID;
                            break;
                        }
                    case "email":
                        {
                            var theUser = await _context.Persons.AsNoTracking().FirstOrDefaultAsync(x => x.Email == fieldValue) ?? new Person();
                            userId = theUser.ID;
                            break;
                        }
                    case "nationalcode":
                        {
                            var theUser = await _context.Persons.AsNoTracking().FirstOrDefaultAsync(x => x.NaCode == fieldValue) ?? new Person();
                            userId = theUser.ID;
                            break;
                        }
                    case "phonenumber":
                        {
                            var theUser = await _context.Persons.AsNoTracking().FirstOrDefaultAsync(x => x.PhoneNumber == fieldValue) ?? new Person();
                            userId = theUser.ID;
                            break;
                        }
                }
                result.ID = userId;
                result.Status = userId > 0;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }

        public async Task<ListResultObject<Login>> GetAllLoginsAsync(long personId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
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
                            (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Username.ToString()) && x.Username.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.LastLoginDate.ToString()) && x.LastLoginDate.ToString().Contains(searchText)) ||
                            (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                            (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
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
                                 (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.ToString().Contains(searchText)) ||
                                 (!string.IsNullOrEmpty(x.Username.ToString()) && x.Username.ToString().Contains(searchText)) ||
                                 (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText)) ||
                                 (!string.IsNullOrEmpty(x.LastLoginDate.ToString()) && x.LastLoginDate.ToString().Contains(searchText)) ||
                                 (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                                 (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                             )
                         );
                }

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
                .SortBy(sortQuery).ToPaging(pageIndex, pageSize)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;

        }

        public async Task<RowResultObject<Login>> GetLoginByIdAsync(long LoginId, int searchMode)
        {
            RowResultObject<Login> result = new RowResultObject<Login>();
            try
            {
                switch (searchMode)
                {
                    default:
                    case 1:
                        {
                            result.Result = await _context.Logins.Include(x=> x.Person)
                                            .AsNoTracking()
                                            .SingleOrDefaultAsync(x => x.ID == LoginId);
                        }
                        break;
                    case 2:
                        {
                            result.Result = await _context.Logins.Include(x => x.Person)
                                            .AsNoTracking()
                                            .SingleOrDefaultAsync(x => x.PersonID == LoginId);
                        }
                        break;
                }
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
                result.ID = Login.ID;
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
                var Login = await GetLoginByIdAsync(LoginId,1);
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
