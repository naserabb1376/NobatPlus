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
    public class StylistRep : IStylistRep
    {

        private NobatPlusContext _context;
        public StylistRep()
        {
            _context = DbTools.GetDbContext();
        }

        public void AddStylist(Stylist Stylist)
        {
            _context.Stylists.Add(Stylist);
            _context.SaveChanges();
            _context.Entry(Stylist).State = EntityState.Detached;
        }

        public void EditStylist(Stylist Stylist)
        {
            _context.Stylists.Update(Stylist);
            _context.SaveChanges();
            _context.Entry(Stylist).State = EntityState.Detached;
        }

        public bool ExistStylist(long StylistId)
        {
            return _context.Stylists.Any(x => x.ID == StylistId);
        }

        public List<Stylist> GetAllStylists(long parentId = 0, int pageIndex= 1, int pageSize = 20, string searchText= "")
        {
            if (parentId<0)
            {
                return _context.Stylists.Include(x => x.Person).Include(x=> x.JobType).Where(x =>
            (!string.IsNullOrEmpty(x.Person.FirstName.ToString()) && x.Person.FirstName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.LastName.ToString()) && x.Person.LastName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.NaCode.ToString()) && x.Person.NaCode.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.PhoneNumber.ToString()) && x.Person.PhoneNumber.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.Email.ToString()) && x.Person.Email.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.Description.ToString()) && x.Person.Description.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.JobType.JobTitle.ToString()) && x.JobType.JobTitle.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.Specialty.ToString()) && x.Specialty.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
            ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
            else
            {
                return _context.Stylists.Include(x => x.Person).Include(x => x.JobType).Where(x =>
                (x.StylistParentID == parentId) &&
            ((!string.IsNullOrEmpty(x.Person.FirstName.ToString()) && x.Person.FirstName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.LastName.ToString()) && x.Person.LastName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.NaCode.ToString()) && x.Person.NaCode.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.PhoneNumber.ToString()) && x.Person.PhoneNumber.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.Email.ToString()) && x.Person.Email.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.Description.ToString()) && x.Person.Description.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.JobType.JobTitle.ToString()) && x.JobType.JobTitle.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.Specialty.ToString()) && x.Specialty.ToString().Contains(searchText))
            )).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
        }

        public List<Stylist> GetStylistsOfService(long serviceManagementId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return _context.StylistServices
           .Where(bs => bs.ServiceManagementID == serviceManagementId)
           .Select(bs => bs.Stylist).Include(x => x.Person).Include(x => x.JobType).Where(x =>
            (!string.IsNullOrEmpty(x.Person.FirstName.ToString()) && x.Person.FirstName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.LastName.ToString()) && x.Person.LastName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.NaCode.ToString()) && x.Person.NaCode.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.PhoneNumber.ToString()) && x.Person.PhoneNumber.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.Email.ToString()) && x.Person.Email.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.Description.ToString()) && x.Person.Description.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.JobType.JobTitle.ToString()) && x.JobType.JobTitle.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.Specialty.ToString()) && x.Specialty.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
            ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
        }

        public List<Stylist> GetStylistsOfJobType(long JobTypeId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return _context.Stylists.Include(x => x.Person).Include(x => x.JobType).Where(x =>
(x.JobTypeID == JobTypeId) &&
((!string.IsNullOrEmpty(x.Person.FirstName.ToString()) && x.Person.FirstName.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.Person.LastName.ToString()) && x.Person.LastName.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.Person.NaCode.ToString()) && x.Person.NaCode.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.Person.PhoneNumber.ToString()) && x.Person.PhoneNumber.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.Person.Email.ToString()) && x.Person.Email.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.Person.Description.ToString()) && x.Person.Description.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
|| (!string.IsNullOrEmpty(x.JobType.JobTitle.ToString()) && x.JobType.JobTitle.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
|| (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
|| (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
|| (!string.IsNullOrEmpty(x.Specialty.ToString()) && x.Specialty.ToString().Contains(searchText))
)).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
        }

        public List<Stylist> GetStylistsOfDiscount(long DiscountId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            List<Stylist> stylists = new List<Stylist>();

            stylists.AddRange(
                  _context.DiscountAssignments
           .Where(bs => bs.DiscountId == DiscountId)
           .Select(bs => bs.Stylist).Include(x => x.Person).Include(x => x.JobType).Where(x =>
            (!string.IsNullOrEmpty(x.Person.FirstName.ToString()) && x.Person.FirstName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.LastName.ToString()) && x.Person.LastName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.NaCode.ToString()) && x.Person.NaCode.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.PhoneNumber.ToString()) && x.Person.PhoneNumber.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.Email.ToString()) && x.Person.Email.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.Description.ToString()) && x.Person.Description.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.JobType.JobTitle.ToString()) && x.JobType.JobTitle.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.Specialty.ToString()) && x.Specialty.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
            ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList()
                );

            stylists.AddRange(
      _context.ServiceDiscounts
.Where(bs => bs.DiscountId == DiscountId)
.Select(bs => bs.Stylist).Include(x => x.Person).Include(x => x.JobType).Where(x =>
(!string.IsNullOrEmpty(x.Person.FirstName.ToString()) && x.Person.FirstName.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.Person.LastName.ToString()) && x.Person.LastName.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.Person.NaCode.ToString()) && x.Person.NaCode.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.Person.PhoneNumber.ToString()) && x.Person.PhoneNumber.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.Person.Email.ToString()) && x.Person.Email.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.Person.Description.ToString()) && x.Person.Description.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
|| (!string.IsNullOrEmpty(x.JobType.JobTitle.ToString()) && x.JobType.JobTitle.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
|| (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
|| (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
|| (!string.IsNullOrEmpty(x.Specialty.ToString()) && x.Specialty.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList()
    );

            stylists.AddRange(
      _context.CustomerDiscounts
.Where(bs => bs.DiscountId == DiscountId)
.Select(bs => bs.Stylist).Include(x => x.Person).Include(x => x.JobType).Where(x =>
(!string.IsNullOrEmpty(x.Person.FirstName.ToString()) && x.Person.FirstName.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.Person.LastName.ToString()) && x.Person.LastName.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.Person.NaCode.ToString()) && x.Person.NaCode.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.Person.PhoneNumber.ToString()) && x.Person.PhoneNumber.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.Person.Email.ToString()) && x.Person.Email.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.Person.Description.ToString()) && x.Person.Description.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
|| (!string.IsNullOrEmpty(x.JobType.JobTitle.ToString()) && x.JobType.JobTitle.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
|| (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
|| (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
|| (!string.IsNullOrEmpty(x.Specialty.ToString()) && x.Specialty.ToString().Contains(searchText))
|| (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList()
    );


            return stylists;
        }

        public Stylist GetStylistById(long StylistId)
        {
            return _context.Stylists.Find(StylistId);
        }

        public void RemoveStylist(Stylist Stylist)
        {
            _context.Stylists.Remove(Stylist);
            _context.SaveChanges();
            _context.Entry(Stylist).State = EntityState.Detached;
        }

        public void RemoveStylist(long StylistId)
        {
            var Stylist = GetStylistById(StylistId);
            RemoveStylist(Stylist);
        }
    }
}
