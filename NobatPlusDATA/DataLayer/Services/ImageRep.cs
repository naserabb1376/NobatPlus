using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.ResultObjects;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NobatPlusDATA.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace NobatPlusDATA.DataLayer.Services
{
    public class ImageRep : IImageRep
    {
        private NobatPlusContext _context;

        public ImageRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task<BitResultObject> AddImagesAsync(List<Image> images)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.Images.AddRangeAsync(images);
                await _context.SaveChangesAsync();
                result.ID = images.FirstOrDefault().ID;
                foreach (var image in images)
                {
                    _context.Entry(image).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> EditImagesAsync(List<Image> images)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Images.UpdateRange(images);
                await _context.SaveChangesAsync();
                result.ID = images.FirstOrDefault().ID;
                foreach (var image in images)
                {
                    _context.Entry(image).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> ExistImageAsync(long imageId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.Images
                    .AsNoTracking()
                    .AnyAsync(x => x.ID == imageId);
                result.ID = imageId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<Image>> GetAllImagesAsync(
      string entityType = "", long foreignKey = 0, long creatorId = 0,
      int pageIndex = 1, int pageSize = 20,
      string searchText = "", string sortQuery = "")
        {
            var results = new ListResultObject<Image>();

            try
            {
                var query = _context.Images.AsNoTracking().AsQueryable();

                // اعمال شرط‌ها فقط در صورت معتبر بودن
                if (foreignKey > 0)
                    query = query.Where(x => x.ForeignKeyId == foreignKey);

                if (creatorId > 0)
                    query = query.Where(x => x.CreatorId == creatorId);

                if (!string.IsNullOrEmpty(entityType))
                    query = query.Where(x => x.EntityType == entityType);

                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(x =>
                        (!string.IsNullOrEmpty(x.FileName) && x.FileName.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.EntityType) && x.EntityType.Contains(searchText))
                    );
                }

                results.TotalCount = await query.CountAsync();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);

                results.Results = await query
                    .OrderByDescending(x => x.CreateDate)
                    .SortBy(sortQuery) // اگر SortBy اکستنشن خاصی‌ست که پیاده‌سازی‌اش را دارید
                    .ToPaging(pageIndex, pageSize) // مشابه
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return results;
        }

        public async Task<RowResultObject<Image>> GetImageByIdAsync(long imageId)
        {
            RowResultObject<Image> result = new RowResultObject<Image>();
            try
            {
                result.Result = await _context.Images
                    .AsNoTracking()
                    .SingleOrDefaultAsync(x => x.ID == imageId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<RowResultObject<Image>> GetImageForShowAsync(long imageId = 0, long foreignKeyId = 0, string entityType = "", long userId = 0, long roleId = 0)
        {
            RowResultObject<Image> result = new RowResultObject<Image>();
            IQueryable<Image> query = _context.Images.AsNoTracking();
            try
            {
                if (imageId > 0)
                {
                    query = query.Where(x => x.ID == imageId);
                }
                else
                {
                    if (!string.IsNullOrEmpty(entityType))
                    {
                        query = query.Where(x => x.EntityType == entityType);
                    }
                    if (foreignKeyId > 0)
                    {
                        query = query.Where(x => x.ForeignKeyId == foreignKeyId);
                    }
                }
                var theRow = await query.OrderByDescending(x => x.ID).FirstOrDefaultAsync();

                if (theRow.Description.ToLower() != "public" && (roleId != 3 && theRow.CreatorId != userId))
                {
                    result.Status = false;
                    result.ErrorMessage = $"The User Has No Access To This Image";
                }
                else
                {
                    result.Status = true;
                    result.Result = theRow;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<long> GetNewRowNumber()
        {
            long rowNumber = await _context.Images.CountAsync() +1;
            return rowNumber;
        }

        public async Task<BitResultObject> RemoveImagesAsync(List<Image> images)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Images.RemoveRange(images);
                await _context.SaveChangesAsync();
                result.ID = images.FirstOrDefault().ID;
                foreach (var image in images)
                {
                    _context.Entry(image).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }
        public async Task<BitResultObject> RemoveOldImagesAsync(long foreignKeyId, string entityName)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var oldImages = await GetAllImagesAsync(entityName, foreignKeyId, pageSize: 0);
                foreach (var oldImage in oldImages.Results)
                {
                    if (File.Exists(oldImage.FilePath))
                    {
                        File.Delete(oldImage.FilePath);
                    }
                }
                var removeResult = await RemoveImagesAsync(oldImages.Results);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }
        public async Task<BitResultObject> RemoveImagesAsync(List<long> ImageIds)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var ImagesToRemove = new List<Image>();

                foreach (var ImageId in ImageIds)
                {
                    var Image = await GetImageByIdAsync(ImageId);
                    if (Image.Result != null)
                    {
                        ImagesToRemove.Add(Image.Result);
                    }
                }

                if (ImagesToRemove.Any())
                {
                    result = await RemoveImagesAsync(ImagesToRemove);
                }
                else
                {
                    result.Status = false;
                    result.ErrorMessage = "No matching Images found to remove.";
                }
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