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

        public void AddRegister(Register Register)
        {
            _context.Registers.Add(Register);
            _context.SaveChanges();
            _context.Entry(Register).State = EntityState.Detached;
        }

        public void EditRegister(Register Register)
        {
            _context.Registers.Update(Register);
            _context.SaveChanges();
            _context.Entry(Register).State = EntityState.Detached;
        }

        public bool ExistRegister(long RegisterId)
        {
            return _context.Registers.Any(x => x.ID == RegisterId);
        }

        public List<Register> GetAllRegisters(int pageIndex= 1, int pageSize = 20, string searchText= "")
        {
            return _context.Registers.Include(x => x.Person).Where(x =>
   (!string.IsNullOrEmpty(x.Person.FirstName.ToString()) && x.Person.FirstName.ToString().Contains(searchText))
  || (!string.IsNullOrEmpty(x.Person.LastName.ToString()) && x.Person.LastName.ToString().Contains(searchText))
  || (!string.IsNullOrEmpty(x.Person.NaCode.ToString()) && x.Person.NaCode.ToString().Contains(searchText))
  || (!string.IsNullOrEmpty(x.Person.PhoneNumber.ToString()) && x.Person.PhoneNumber.ToString().Contains(searchText))
  || (!string.IsNullOrEmpty(x.Person.Email.ToString()) && x.Person.Email.ToString().Contains(searchText))
  || (!string.IsNullOrEmpty(x.Person.Description.ToString()) && x.Person.Description.ToString().Contains(searchText))
  || (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
  || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
  || (!string.IsNullOrEmpty(x.RegistrationDate.ToString()) && x.RegistrationDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
  || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
  || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
   ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();

        }

        public Register GetRegisterById(long RegisterId)
        {
            return _context.Registers.Find(RegisterId);
        }

        public Register GetRegisterByPersonId(long PersonId)
        {
            return _context.Registers.SingleOrDefault(x=> x.PersonID == PersonId);
        }

        public void RemoveRegister(Register Register)
        {
            _context.Registers.Remove(Register);
            _context.SaveChanges();
            _context.Entry(Register).State = EntityState.Detached;
        }

        public void RemoveRegister(long RegisterId)
        {
            var Register = GetRegisterById(RegisterId);
            RemoveRegister(Register);
        }
    }
}
