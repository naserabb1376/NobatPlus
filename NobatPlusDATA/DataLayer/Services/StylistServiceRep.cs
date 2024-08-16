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
    public class StylistServiceRep : IStylistServiceRep
    {

        private NobatPlusContext _context;
        public StylistServiceRep()
        {
            _context = DbTools.GetDbContext();
        }

        public void AddStylistService(StylistService StylistService)
        {
            _context.StylistServices.Add(StylistService);
            _context.SaveChanges();
            _context.Entry(StylistService).State = EntityState.Detached;
        }

        public void EditStylistService(StylistService StylistService)
        {
            _context.StylistServices.Update(StylistService);
            _context.SaveChanges();
            _context.Entry(StylistService).State = EntityState.Detached;
        }

        public bool ExistStylistService(long StylistId, long ServiceManagementId)
        {
            return _context.StylistServices.Any(x => x.StylistID == StylistId && x.ServiceManagementID == ServiceManagementId);
        }

        public List<StylistService> GetAllStylistServices(int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return _context.StylistServices.Include(x=> x.Stylist).ThenInclude(x=> x.Person).Include(x=> x.ServiceManagement).Where(x =>
            (!string.IsNullOrEmpty(x.ServiceManagement.ServiceName.ToString()) && x.ServiceManagement.ServiceName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.ServiceManagement.Description.ToString()) && x.ServiceManagement.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.ServiceManagement.Duration.ToString()) && x.ServiceManagement.Duration.ToString("HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.ServiceManagement.ServiceName.ToString()) && x.ServiceManagement.ServiceName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Stylist.Specialty.ToString()) && x.Stylist.Specialty.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))
            ).OrderByDescending(x => x.ServiceManagement.CreateDate).ToPaging(pageIndex,pageSize).ToList();
        }

        public StylistService GetStylistServiceById(long StylistId, long ServiceManagementId)
        {
            return _context.StylistServices.Include(x => x.Stylist).ThenInclude(x => x.Person).Include(x => x.ServiceManagement).SingleOrDefault(x => x.StylistID == StylistId && x.ServiceManagementID == ServiceManagementId);
        }

        public void RemoveStylistService(StylistService StylistService)
        {
            _context.StylistServices.Remove(StylistService);
            _context.SaveChanges();
            _context.Entry(StylistService).State = EntityState.Detached;
        }

        public void RemoveStylistService(long StylistId, long ServiceManagementId)
        {
            var StylistService = GetStylistServiceById(StylistId,ServiceManagementId);
            RemoveStylistService(StylistService);
        }
    }
}
