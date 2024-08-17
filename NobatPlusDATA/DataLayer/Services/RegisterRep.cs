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
    public class RegisterRep : IRegisterRep
    {

        private NobatPlusContext _context;
        public RegisterRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task AddRegisterAsync(Register Register)
        {
            _context.Registers.Add(Register);
            await _context.SaveChangesAsync();
            _context.Entry(Register).State = EntityState.Detached;
        }

        public async Task EditRegisterAsync(Register Register)
        {
            _context.Registers.Update(Register);
            await _context.SaveChangesAsync();
            _context.Entry(Register).State = EntityState.Detached;
        }

        public async Task<bool> ExistRegisterAsync(long RegisterId)
        {
            return await _context.Registers
                .AsNoTracking()
                .AnyAsync(x => x.ID == RegisterId);
        }

        public async Task<List<Register>> GetAllRegistersAsync(int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return await _context.Registers
                .AsNoTracking()
                .Include(x => x.Person)
                .Where(x =>
                    (!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                    (x.Person.DateOfBirth.HasValue && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                    (x.RegistrationDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (x.CreateDate.HasValue && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                )
                .OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
        }

        public async Task<Register> GetRegisterByIdAsync(long RegisterId)
        {
            return await _context.Registers
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == RegisterId);
        }

        public async Task<Register> GetRegisterByPersonIdAsync(long PersonId)
        {
            return await _context.Registers
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.PersonID == PersonId);
        }

        public async Task RemoveRegisterAsync(Register Register)
        {
            _context.Registers.Remove(Register);
            await _context.SaveChangesAsync();
            _context.Entry(Register).State = EntityState.Detached;
        }

        public async Task RemoveRegisterAsync(long RegisterId)
        {
            var Register = await GetRegisterByIdAsync(RegisterId);
            await RemoveRegisterAsync(Register);
        }
    }
}
