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

        public async Task AddCheckAvailabilityAsync(CheckAvailability CheckAvailability)
        {
            _context.CheckAvailabilities.Add(CheckAvailability);
            await _context.SaveChangesAsync();
            _context.Entry(CheckAvailability).State = EntityState.Detached;
        }

        public async Task EditCheckAvailabilityAsync(CheckAvailability CheckAvailability)
        {
            _context.CheckAvailabilities.Update(CheckAvailability);
            await _context.SaveChangesAsync();
            _context.Entry(CheckAvailability).State = EntityState.Detached;
        }

        public async Task<bool> ExistCheckAvailabilityAsync(long CheckAvailabilityId)
        {
            return await _context.CheckAvailabilities
                .AsNoTracking()
                .AnyAsync(x => x.ID == CheckAvailabilityId);
        }

        public async Task<List<CheckAvailability>> GetAllCheckAvailabilitiesAsync(long stylistId = -1, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            if (stylistId < 0)
            {
                return await _context.CheckAvailabilities
                    .AsNoTracking()
                    .Include(x => x.Stylist).ThenInclude(x => x.Person)
                    .Where(x =>
                        (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Stylist.Specialty.ToString()) && x.Stylist.Specialty.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Time.ToString()) && x.Time.ToString("HH:mm").Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Date.ToString()) && x.Date.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    )
                    .OrderByDescending(x => x.CreateDate)
                    .ToPaging(pageIndex, pageSize)
                    .ToListAsync();
            }
            else
            {
                return await _context.CheckAvailabilities
                    .AsNoTracking()
                    .Include(x => x.Stylist).ThenInclude(x => x.Person)
                    .Where(x =>
                        x.StylistID == stylistId &&
                        (
                            (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Stylist.Specialty.ToString()) && x.Stylist.Specialty.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Time.ToString()) && x.Time.ToString("HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Date.ToString()) && x.Date.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        )
                    )
                    .OrderByDescending(x => x.CreateDate)
                    .ToPaging(pageIndex, pageSize)
                    .ToListAsync();
            }
        }

        public async Task<CheckAvailability> GetCheckAvailabilityByIdAsync(long CheckAvailabilityId)
        {
            return await _context.CheckAvailabilities
                .AsNoTracking()
 .SingleOrDefaultAsync(x => x.ID == CheckAvailabilityId);
        }

        public async Task RemoveCheckAvailabilityAsync(CheckAvailability CheckAvailability)
        {
            _context.CheckAvailabilities.Remove(CheckAvailability);
            await _context.SaveChangesAsync();
            _context.Entry(CheckAvailability).State = EntityState.Detached;
        }

        public async Task RemoveCheckAvailabilityAsync(long CheckAvailabilityId)
        {
            var CheckAvailability = await GetCheckAvailabilityByIdAsync(CheckAvailabilityId);
            await RemoveCheckAvailabilityAsync(CheckAvailability);
        }
    }
}
