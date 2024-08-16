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
    public class CheckAvailabilityRep : ICheckAvailabilityRep
    {

        private NobatPlusContext _context;
        public CheckAvailabilityRep()
        {
            _context = DbTools.GetDbContext();
        }

        public void AddCheckAvailability(CheckAvailability CheckAvailability)
        {
            _context.CheckAvailabilities.Add(CheckAvailability);
            _context.SaveChanges();
            _context.Entry(CheckAvailability).State = EntityState.Detached;
        }

        public void EditCheckAvailability(CheckAvailability CheckAvailability)
        {
            _context.CheckAvailabilities.Update(CheckAvailability);
            _context.SaveChanges();
            _context.Entry(CheckAvailability).State = EntityState.Detached;
        }

        public bool ExistCheckAvailability(long CheckAvailabilityId)
        {
            return _context.CheckAvailabilities.Any(x => x.ID == CheckAvailabilityId);
        }

        public List<CheckAvailability> GetAllCheckAvailabilities(long stylistId = -1, int pageIndex= 1, int pageSize = 20, string searchText= "")
        {
            if (stylistId < 0)
            {
            return _context.CheckAvailabilities.Include(x=> x.Stylist).ThenInclude(x => x.Person).Where(x =>
             (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Stylist.Specialty.ToString()) && x.Stylist.Specialty.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Time.ToString()) && x.Time.ToString("HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Date.ToString()) && x.Date.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             ).OrderByDescending(x=> x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
            else
            {
                return _context.CheckAvailabilities.Include(x => x.Stylist).ThenInclude(x => x.Person).Where(x =>
                (x.StylistID == stylistId) &&
             ((!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Stylist.Specialty.ToString()) && x.Stylist.Specialty.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Time.ToString()) && x.Time.ToString("HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Date.ToString()) && x.Date.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             )).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
            
        }

        public CheckAvailability GetCheckAvailabilityById(long CheckAvailabilityId)
        {
            return _context.CheckAvailabilities.Find(CheckAvailabilityId);
        }

        public void RemoveCheckAvailability(CheckAvailability CheckAvailability)
        {
            _context.CheckAvailabilities.Remove(CheckAvailability);
            _context.SaveChanges();
            _context.Entry(CheckAvailability).State = EntityState.Detached;
        }

        public void RemoveCheckAvailability(long CheckAvailabilityId)
        {
            var CheckAvailability = GetCheckAvailabilityById(CheckAvailabilityId);
            RemoveCheckAvailability(CheckAvailability);
        }
    }
}
