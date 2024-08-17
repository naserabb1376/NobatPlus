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

        public async Task AddStylistServiceAsync(StylistService StylistService)
        {
            _context.StylistServices.Add(StylistService);
            await _context.SaveChangesAsync();
            _context.Entry(StylistService).State = EntityState.Detached;
        }

        public async Task EditStylistServiceAsync(StylistService StylistService)
        {
            _context.StylistServices.Update(StylistService);
            await _context.SaveChangesAsync();
            _context.Entry(StylistService).State = EntityState.Detached;
        }

        public async Task<bool> ExistStylistServiceAsync(long StylistId, long ServiceManagementId)
        {
            return await _context.StylistServices.AnyAsync(x => x.StylistID == StylistId && x.ServiceManagementID == ServiceManagementId);
        }

        public async Task<List<StylistService>> GetAllStylistServicesAsync(int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return await _context.StylistServices
                .Include(x => x.Stylist).ThenInclude(x => x.Person)
                .Include(x => x.ServiceManagement)
                .AsNoTracking()
                .Where(x =>
                    (!string.IsNullOrEmpty(x.ServiceManagement.ServiceName) && x.ServiceManagement.ServiceName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.ServiceManagement.Description) && x.ServiceManagement.Description.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.ServiceManagement.Duration.ToString("HH:mm")) && x.ServiceManagement.Duration.ToString("HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Stylist.Specialty) && x.Stylist.Specialty.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Stylist.Person.FirstName) && x.Stylist.Person.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Stylist.Person.LastName) && x.Stylist.Person.LastName.Contains(searchText))
                )
                .OrderByDescending(x => x.ServiceManagement.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
        }

        public async Task<StylistService> GetStylistServiceByIdAsync(long StylistId, long ServiceManagementId)
        {
            return await _context.StylistServices
                .Include(x => x.Stylist).ThenInclude(x => x.Person)
                .Include(x => x.ServiceManagement)
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.StylistID == StylistId && x.ServiceManagementID == ServiceManagementId);
        }

        public async Task RemoveStylistServiceAsync(StylistService StylistService)
        {
            _context.StylistServices.Remove(StylistService);
            await _context.SaveChangesAsync();
            _context.Entry(StylistService).State = EntityState.Detached;
        }

        public async Task RemoveStylistServiceAsync(long StylistId, long ServiceManagementId)
        {
            var stylistService = await GetStylistServiceByIdAsync(StylistId, ServiceManagementId);
            if (stylistService != null)
            {
                await RemoveStylistServiceAsync(stylistService);
            }
        }
    }
}
