using Domain;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Services
{
    public class ApiGuideRep : IApiGuideRep
    {

        private NobatPlusContext _context;
        public ApiGuideRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task<BitResultObject> AddApiGuideAsync(ApiGuide ApiGuide)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.ApiGuides.AddAsync(ApiGuide);
                await _context.SaveChangesAsync();
                result.ID = ApiGuide.ID;
                _context.Entry(ApiGuide).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> EditApiGuideAsync(ApiGuide ApiGuide)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.ApiGuides.Update(ApiGuide);
                await _context.SaveChangesAsync();
                result.ID = ApiGuide.ID;
                _context.Entry(ApiGuide).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> ExistApiGuideAsync(long ApiGuideId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.ApiGuides
                .AsNoTracking()
                .AnyAsync(x => x.ID == ApiGuideId);
                result.ID = ApiGuideId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<ListResultObject<ApiGuide>> GetAllApiGuidesAsync(string guideType="", int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<ApiGuide> results = new ListResultObject<ApiGuide>();
            try
            {
                var query = _context.ApiGuides
                .AsNoTracking()
                .Where(x =>
                        (!string.IsNullOrEmpty(guideType) && x.GuideType == guideType) &&
                        ((!string.IsNullOrEmpty(x.ApiName.ToString()) && x.ApiName.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.ModelName.ToString()) && x.ModelName.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.FieldEnglishName.ToString()) && x.FieldEnglishName.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.FieldFarsiName.ToString()) && x.FieldFarsiName.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.FieldDataType.ToString()) && x.FieldDataType.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.FieldRecomendedInputType.ToString()) && x.FieldRecomendedInputType.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText)) ||
                        (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                        (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText)))
                );
                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
           
        }

        public async Task<RowResultObject<ApiGuide>> GetApiGuideByIdAsync(long ApiGuideId)
        {
            RowResultObject<ApiGuide> result = new RowResultObject<ApiGuide>();
            try
            {
                result.Result = await _context.ApiGuides
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == ApiGuideId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<RowResultObject<ApiGuide>> GetGuideForApiAsync(string apiName, string guideType)
        {

            RowResultObject<ApiGuide> result = new RowResultObject<ApiGuide>();
            try
            {
                result.Result = await _context.ApiGuides
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ApiName.ToLower() == apiName.ToLower() && x.GuideType.ToLower() == guideType.ToLower());
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveApiGuideAsync(ApiGuide ApiGuide)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.ApiGuides.Remove(ApiGuide);
                await _context.SaveChangesAsync();
                result.ID = ApiGuide.ID;
                _context.Entry(ApiGuide).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> RemoveApiGuideAsync(long ApiGuideId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var ApiGuide = await GetApiGuideByIdAsync(ApiGuideId);
                result = await RemoveApiGuideAsync(ApiGuide.Result);
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
