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
    public class CityRep : ICityRep
    {

        private NobatPlusContext _context;
        public CityRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddCityAsync(City City)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.Cities.AddAsync(City);
                await _context.SaveChangesAsync();
                result.ID = City.ID;
                _context.Entry(City).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> EditCityAsync(City City)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Cities.Update(City);
                await _context.SaveChangesAsync();
                result.ID = City.ID;
                _context.Entry(City).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> ExistCityAsync(long CityId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.Cities
                .AsNoTracking()
                .AnyAsync(x => x.ID == CityId);
                result.ID = CityId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<ListResultObject<City>> GetAllCitiesAsync(long parentId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<City> results = new ListResultObject<City>();
            try
            {
                IQueryable<City> query = _context.Cities.AsNoTracking();

                if (parentId < 0)
                {
                    query = query.Where(x =>
                        (!string.IsNullOrEmpty(x.CityName) && x.CityName.Contains(searchText)) ||
                        (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                        (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText))
                    );
                }
                else
                {
                    query = query.Where(x =>
                        x.CityParentID == parentId &&
                        ((!string.IsNullOrEmpty(x.CityName) && x.CityName.Contains(searchText)) ||
                        (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                        (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText))
                        )
                    );
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

        public async Task<RowResultObject<City>> GetCityByIdAsync(long CityId)
        {
            RowResultObject<City> result = new RowResultObject<City>();
            try
            {
                result.Result = await _context.Cities
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == CityId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemoveCityAsync(City City)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Cities.Remove(City);
                await _context.SaveChangesAsync();
                result.ID = City.ID;
                _context.Entry(City).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> RemoveCityAsync(long CityId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var City = await GetCityByIdAsync(CityId);
                result = await RemoveCityAsync(City.Result);
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
