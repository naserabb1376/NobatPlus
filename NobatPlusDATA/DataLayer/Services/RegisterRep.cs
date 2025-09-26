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
    public class RegisterRep : IRegisterRep
    {

        private NobatPlusContext _context;
        public RegisterRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddRegisterAsync(Register Register)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.Registers.AddAsync(Register);
                await _context.SaveChangesAsync();
                result.ID = Register.ID;
                _context.Entry(Register).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
         
        }

        public async Task<BitResultObject> EditRegisterAsync(Register Register)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Registers.Update(Register);
                await _context.SaveChangesAsync();
                result.ID = Register.ID;
                _context.Entry(Register).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> ExistRegisterAsync(long UniqueId, int SearchMode = 1)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                switch (SearchMode)
                {
                    default:
                    case 1:
               result.Status = await _context.Registers
               .AsNoTracking()
               .AnyAsync(x => x.ID == UniqueId);
                        break;
                    case 2:
                        result.Status = await _context.Registers
                        .AsNoTracking()
                        .AnyAsync(x => x.PersonID == UniqueId);
                        break;
                }
                result.ID = UniqueId;

            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<ListResultObject<Register>> GetAllRegistersAsync(int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<Register> results = new ListResultObject<Register>();
            try
            {
                var query = _context.Registers
                .AsNoTracking()
                .Include(x => x.Person)
                .Where(x =>
                    (!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.RegistrationDate.ToString()) && x.RegistrationDate.ToString().Contains(searchText)) ||
                    (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                    (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                );
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

        public async Task<RowResultObject<Register>> GetRegisterByIdAsync(long RegisterId)
        {
            RowResultObject<Register> result = new RowResultObject<Register>();
            try
            {
                result.Result = await _context.Registers.Include(x=> x.Person)
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == RegisterId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<RowResultObject<Register>> GetRegisterByPersonIdAsync(long PersonId)
        {
            RowResultObject<Register> result = new RowResultObject<Register>();
            try
            {
                result.Result = await _context.Registers
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.PersonID == PersonId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemoveRegisterAsync(Register Register)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Registers.Remove(Register);
                await _context.SaveChangesAsync();
                result.ID = Register.ID;
                _context.Entry(Register).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemoveRegisterAsync(long RegisterId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var Register = await GetRegisterByIdAsync(RegisterId);
                result = await RemoveRegisterAsync(Register.Result);
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
