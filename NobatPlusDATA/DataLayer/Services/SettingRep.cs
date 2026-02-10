using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace NobatPlusDATA.DataLayer.Services
{
    public class SettingRep : ISettingRep
    {
        private NobatPlusContext _context;

        public SettingRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddSettingAsync(Setting setting)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.Settings.AddAsync(setting);
                await _context.SaveChangesAsync();
                result.ID = setting.ID;
                _context.Entry(setting).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> EditSettingAsync(Setting setting)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Settings.Update(setting);
                await _context.SaveChangesAsync();
                result.ID = setting.ID;
                _context.Entry(setting).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> ExistSettingRowAsync(long settingId = 0, string settingKey = "")
        {
            BitResultObject result = new BitResultObject();
            try
            {
                if (!string.IsNullOrEmpty(settingKey))
                {
                    result.Status = await _context.Settings
    .AsNoTracking()
    .AnyAsync(x => x.Key.ToLower() == settingKey.ToLower());
                }
                else
                {
                    result.Status = await _context.Settings
    .AsNoTracking()
    .AnyAsync(x => x.ID == settingId);

                }
                result.ID = settingId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<Setting>> GetAllSettingsAsync(long ParentId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "")
        {
            ListResultObject<Setting> results = new ListResultObject<Setting>();
            try
            {
                var query = _context.Settings.AsNoTracking();

                if (ParentId > 0)
                {
                    query = query.Where(x => x.ParentId == ParentId);
                }

                query = query.Where(x =>
                        (!string.IsNullOrEmpty(x.Key) && x.Key.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Value) && x.Value.Contains(searchText))
                    );

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
                     .SortBy(sortQuery).ToPaging(pageIndex, pageSize)
                    .Include(x => x.Parent)
                    .Include(x => x.Children)
                    .ToListAsync();

            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
        }

        public async Task<RowResultObject<Setting>> GetSettingRowAsync(long settingId = 0, string settingKey = "")
        {
            RowResultObject<Setting> result = new RowResultObject<Setting>();
            try
            {
                if (!string.IsNullOrEmpty(settingKey))
                {
                    result.Result = await _context.Settings
    .AsNoTracking()
    .Include(x => x.Parent)
    .Include(x => x.Children)
    .SingleOrDefaultAsync(x => x.Key.ToLower() == settingKey.ToLower());
                }
                else
                {
                    result.Result = await _context.Settings
    .AsNoTracking()
    .Include(x => x.Parent)
    .Include(x => x.Children)
    .SingleOrDefaultAsync(x => x.ID == settingId);


                }
                
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveSettingAsync(Setting setting)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var Images = _context.Images.Where(img => img.ForeignKeyId == setting.ID && img.EntityType == "Setting");
                _context.Images.RemoveRange(Images);

                _context.Settings.Remove(setting);
                await _context.SaveChangesAsync();
                result.ID = setting.ID;
                _context.Entry(setting).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveSettingAsync(long settingId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var setting = await GetSettingRowAsync(settingId);
                result = await RemoveSettingAsync(setting.Result);
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