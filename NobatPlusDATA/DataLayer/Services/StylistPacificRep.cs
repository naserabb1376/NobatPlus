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
    public class StylistPacificRep : IStylistPacificRep
    {

        private NobatPlusContext _context;
        public StylistPacificRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddStylistPacificAsync(StylistPacific StylistPacific)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.StylistPacifics.AddAsync(StylistPacific);
                await _context.SaveChangesAsync();
                result.ID = StylistPacific.ID;
                _context.Entry(StylistPacific).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> EditStylistPacificAsync(StylistPacific StylistPacific)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.StylistPacifics.Update(StylistPacific);
                await _context.SaveChangesAsync();
                result.ID = StylistPacific.ID;
                _context.Entry(StylistPacific).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> ExistStylistPacificAsync(long StylistPacificId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.StylistPacifics.AsNoTracking().AnyAsync(x => x.ID == StylistPacificId);
                result.ID = StylistPacificId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
             
        }

        public async Task<ListResultObject<StylistPacific>> GetAllStylistPacificsAsync(long stylistId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<StylistPacific> results = new ListResultObject<StylistPacific>();
            try
            {
                IQueryable<StylistPacific> query;

                query = _context.StylistPacifics
                        .AsNoTracking()
                        .Include(x => x.Stylist).ThenInclude(x => x.Person)
                        .Where(x =>
                            (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))                           
                            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
                            || (x.PacificStartDate != null && x.PacificStartDate.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.PacificEndDate.ToString()) && x.PacificEndDate.ToString().Contains(searchText))
                            || (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText))
                            || (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                        );
                if (stylistId > 0)
                {
                    query = query.Where(x=> x.StylistID == stylistId);
                }
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

        public async Task<RowResultObject<StylistPacific>> GetStylistPacificByIdAsync(long StylistPacificId)
        {
            RowResultObject<StylistPacific> result = new RowResultObject<StylistPacific>();
            try
            {
                result.Result = await _context.StylistPacifics
                .AsNoTracking()
                .Include(x => x.Stylist).ThenInclude(x => x.Person)
                .SingleOrDefaultAsync(x => x.ID == StylistPacificId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveStylistPacificAsync(StylistPacific StylistPacific)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.StylistPacifics.Remove(StylistPacific);
                await _context.SaveChangesAsync();
                result.ID = StylistPacific.ID;
                _context.Entry(StylistPacific).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> RemoveStylistPacificAsync(long StylistPacificId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var StylistPacific = await GetStylistPacificByIdAsync(StylistPacificId);
                result = await RemoveStylistPacificAsync(StylistPacific.Result);
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
