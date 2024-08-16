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

        public void AddAdmin(Admin Admin)
        {
            _context.Admins.Add(Admin);
            _context.SaveChanges();
            _context.Entry(Admin).State = EntityState.Detached;
        }

        public void EditAdmin(Admin Admin)
        {
            _context.Admins.Update(Admin);
            _context.SaveChanges();
            _context.Entry(Admin).State = EntityState.Detached;
        }

        public bool ExistAdmin(long AdminId)
        {
            return _context.Admins.Any(x => x.ID == AdminId);
        }

        public List<Admin> GetAllAdmins(int pageIndex= 1, int pageSize = 20, string searchText= "")
        {
            return _context.Admins.Include(x => x.Person).Where(x =>
             (!string.IsNullOrEmpty(x.Person.FirstName.ToString()) && x.Person.FirstName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.LastName.ToString()) && x.Person.LastName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.NaCode.ToString()) && x.Person.NaCode.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.PhoneNumber.ToString()) && x.Person.PhoneNumber.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.Email.ToString()) && x.Person.Email.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.Description.ToString()) && x.Person.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             ).OrderByDescending(x=> x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
        }

        public List<Admin> GetAdminsOfDiscount(long DiscountId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            List<Admin> admins = new List<Admin>();

            admins.AddRange(
                  _context.DiscountAssignments
           .Where(bs => bs.DiscountId == DiscountId)
           .Select(bs => bs.Admin).Include(x => x.Person).Where(x =>
             (!string.IsNullOrEmpty(x.Person.FirstName.ToString()) && x.Person.FirstName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.LastName.ToString()) && x.Person.LastName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.NaCode.ToString()) && x.Person.NaCode.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.PhoneNumber.ToString()) && x.Person.PhoneNumber.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.Email.ToString()) && x.Person.Email.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.Description.ToString()) && x.Person.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList()
                );

            admins.AddRange(
                  _context.ServiceDiscounts
           .Where(bs => bs.DiscountId == DiscountId)
           .Select(bs => bs.Admin).Include(x => x.Person).Where(x =>
             (!string.IsNullOrEmpty(x.Person.FirstName.ToString()) && x.Person.FirstName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.LastName.ToString()) && x.Person.LastName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.NaCode.ToString()) && x.Person.NaCode.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.PhoneNumber.ToString()) && x.Person.PhoneNumber.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.Email.ToString()) && x.Person.Email.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.Description.ToString()) && x.Person.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList()
                );

            return admins;
        }

        public Admin GetAdminById(long AdminId)
        {
            return _context.Admins.Find(AdminId);
        }

        public void RemoveAdmin(Admin Admin)
        {
            _context.Admins.Remove(Admin);
            _context.SaveChanges();
            _context.Entry(Admin).State = EntityState.Detached;
        }

        public void RemoveAdmin(long AdminId)
        {
            var Admin = GetAdminById(AdminId);
            RemoveAdmin(Admin);
        }
    }
}
