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
    public class DiscountAssignmentRep : IDiscountAssignmentRep
    {

        private NobatPlusContext _context;
        public DiscountAssignmentRep()
        {
            _context = DbTools.GetDbContext();
        }

        public void AddDiscountAssignment(DiscountAssignment DiscountAssignment)
        {
            _context.DiscountAssignments.Add(DiscountAssignment);
            _context.SaveChanges();
            _context.Entry(DiscountAssignment).State = EntityState.Detached;
        }

        public void EditDiscountAssignment(DiscountAssignment DiscountAssignment)
        {
            _context.DiscountAssignments.Update(DiscountAssignment);
            _context.SaveChanges();
            _context.Entry(DiscountAssignment).State = EntityState.Detached;
        }

        public bool ExistDiscountAssignment(long DiscountAssignmentId)
        {
            return _context.DiscountAssignments.Any(x => x.ID == DiscountAssignmentId);
        }

        public List<DiscountAssignment> GetAllDiscountAssignments(long DiscountId, long AdminId = 0, long StylistId = 0, int pageIndex= 1, int pageSize = 20, string searchText= "")
        {
            if (AdminId > 0)
            {
                return _context.DiscountAssignments.Include(x => x.Discount).Include(x => x.Admin).ThenInclude(x => x.Person).Include(x => x.Stylist).ThenInclude(x => x.Person).Where(x =>
                 ( x.DiscountId == DiscountId && x.AdminId == AdminId) &&
             ((!string.IsNullOrEmpty(x.Discount.DiscountCode.ToString()) && x.Discount.DiscountCode.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Discount.DiscountAmount.ToString()) && x.Discount.DiscountCode.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Discount.Description.ToString()) && x.Discount.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Discount.StartDate.ToString()) && x.Discount.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Discount.EndDate.ToString()) && x.Discount.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Admin.Person.FirstName.ToString()) && x.Admin.Person.FirstName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Admin.Person.LastName.ToString()) && x.Admin.Person.LastName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Stylist.Specialty.ToString()) && x.Stylist.Specialty.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             )).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }

            if (StylistId > 0)
            {
                return _context.DiscountAssignments.Include(x => x.Discount).Include(x => x.Admin).ThenInclude(x => x.Person).Include(x => x.Stylist).ThenInclude(x => x.Person).Where(x =>
                (x.DiscountId == DiscountId && x.StylistId == StylistId) &&
            ((!string.IsNullOrEmpty(x.Discount.DiscountCode.ToString()) && x.Discount.DiscountCode.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Discount.DiscountAmount.ToString()) && x.Discount.DiscountCode.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Discount.Description.ToString()) && x.Discount.Description.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Discount.StartDate.ToString()) && x.Discount.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.Discount.EndDate.ToString()) && x.Discount.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.Admin.Person.FirstName.ToString()) && x.Admin.Person.FirstName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Admin.Person.LastName.ToString()) && x.Admin.Person.LastName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Stylist.Specialty.ToString()) && x.Stylist.Specialty.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            )).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
            else
            {
                return _context.DiscountAssignments.Include(x => x.Discount).Include(x => x.Admin).ThenInclude(x => x.Person).Include(x => x.Stylist).ThenInclude(x => x.Person).Where(x =>
                (x.DiscountId == DiscountId ) &&
          ((!string.IsNullOrEmpty(x.Discount.DiscountCode.ToString()) && x.Discount.DiscountCode.ToString().Contains(searchText))
         || (!string.IsNullOrEmpty(x.Discount.DiscountAmount.ToString()) && x.Discount.DiscountCode.ToString().Contains(searchText))
         || (!string.IsNullOrEmpty(x.Discount.Description.ToString()) && x.Discount.Description.ToString().Contains(searchText))
         || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
         || (!string.IsNullOrEmpty(x.Discount.StartDate.ToString()) && x.Discount.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
         || (!string.IsNullOrEmpty(x.Discount.EndDate.ToString()) && x.Discount.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
         || (!string.IsNullOrEmpty(x.Admin.Person.FirstName.ToString()) && x.Admin.Person.FirstName.ToString().Contains(searchText))
         || (!string.IsNullOrEmpty(x.Admin.Person.LastName.ToString()) && x.Admin.Person.LastName.ToString().Contains(searchText))
         || (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
         || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))
         || (!string.IsNullOrEmpty(x.Stylist.Specialty.ToString()) && x.Stylist.Specialty.ToString().Contains(searchText))
         || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
         || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
          )).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
        }

        public DiscountAssignment GetDiscountAssignmentById(long DiscountAssignmentId)
        {
            return _context.DiscountAssignments.Find(DiscountAssignmentId);
        }

        public void RemoveDiscountAssignment(DiscountAssignment DiscountAssignment)
        {
            _context.DiscountAssignments.Remove(DiscountAssignment);
            _context.SaveChanges();
            _context.Entry(DiscountAssignment).State = EntityState.Detached;
        }

        public void RemoveDiscountAssignment(long DiscountAssignmentId)
        {
            var DiscountAssignment = GetDiscountAssignmentById(DiscountAssignmentId);
            RemoveDiscountAssignment(DiscountAssignment);
        }
    }
}
