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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static System.Net.Mime.MediaTypeNames;

namespace NobatPlusDATA.DataLayer.Services
{
    public class FileUploadRep : IFileUploadRep
    {
        private NobatPlusContext _context;

        public FileUploadRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task<BitResultObject> AddFileUploadAsync(FileUpload fileUpload)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.FileUploads.AddAsync(fileUpload);
                await _context.SaveChangesAsync();
                result.ID = fileUpload.ID;
                _context.Entry(fileUpload).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> EditFileUploadAsync(FileUpload fileUpload)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.FileUploads.Update(fileUpload);
                await _context.SaveChangesAsync();
                result.ID = fileUpload.ID;
                _context.Entry(fileUpload).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> ExistFileUploadAsync(long fileUploadId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.FileUploads
                    .AsNoTracking()
                    .AnyAsync(x => x.ID == fileUploadId);
                result.ID = fileUploadId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<FileUpload>> GetAllFileUploadsAsync(
          string entityType = "", long ForeignKeyId = 0, long creatorId = 0,
          int pageIndex = 1, int pageSize = 20,
          string searchText = "", string sortQuery = "")
        {
            ListResultObject<FileUpload> results = new ListResultObject<FileUpload>();

            try
            {
                var query = _context.FileUploads.AsNoTracking().AsQueryable();

                // شرط‌های دینامیک فقط در صورت معتبر بودن
                if (ForeignKeyId > 0)
                    query = query.Where(x => x.ForeignKeyId == ForeignKeyId);

                if (creatorId > 0)
                    query = query.Where(x => x.CreatorId == creatorId);

                if (!string.IsNullOrEmpty(entityType))
                    query = query.Where(x => x.EntityType == entityType);

                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(x =>
                        (!string.IsNullOrEmpty(x.FileName) && x.FileName.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.FilePath) && x.FilePath.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.ContentType) && x.ContentType.Contains(searchText))
                    );
                }

                results.TotalCount = await query.CountAsync();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);

                results.Results = await query
                    .OrderByDescending(x => x.CreateDate)
                    .SortBy(sortQuery)
                    .ToPaging(pageIndex, pageSize)
                    //.Include(x => x.Assignment) // در صورت نیاز بازکنید
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return results;
        }

        public async Task<RowResultObject<FileUpload>> GetFileForDownloadAsync(long fileUploadId = 0, long foreignKeyId = 0, string entityType = "", long userId = 0, long roleId = 0)
        {
            RowResultObject<FileUpload> result = new RowResultObject<FileUpload>();
            IQueryable<FileUpload> query = _context.FileUploads.AsNoTracking();
            try
            {
                if (fileUploadId > 0)
                {
                    query = query.Where(x => x.ID == fileUploadId);
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

                var theRow = await query.FirstOrDefaultAsync();

                if (roleId < 4 && theRow.CreatorId != userId)
                {
                    result.Status = false;
                    result.ErrorMessage = $"The User Has No Access To This File";
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

        public async Task<RowResultObject<FileUpload>> GetFileUploadByIdAsync(long fileUploadId)
        {
            RowResultObject<FileUpload> result = new RowResultObject<FileUpload>();
            try
            {
                result.Result = await _context.FileUploads
                    .AsNoTracking()
                    //.Include(x => x.Assignment)
                    .SingleOrDefaultAsync(x => x.ID == fileUploadId);
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
            long rowNumber = await _context.FileUploads.CountAsync() + 1;

            bool existRow = await _context.FileUploads.AnyAsync(x => x.FileName.Contains($"_{rowNumber}_"));

            while (existRow)
            {
                rowNumber++;
                existRow = await _context.FileUploads.AnyAsync(x => x.FileName.Contains($"_{rowNumber}_"));
            }

            return rowNumber;
        }

        public async Task<BitResultObject> RemoveFileUploadAsync(FileUpload fileUpload)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.FileUploads.Remove(fileUpload);
                await _context.SaveChangesAsync();
                result.ID = fileUpload.ID;
                _context.Entry(fileUpload).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveFileUploadAsync(long fileUploadId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var fileUpload = await GetFileUploadByIdAsync(fileUploadId);
                result = await RemoveFileUploadAsync(fileUpload.Result);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveOldFilesAsync(long foreignKeyId, string entityName)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var oldFiles = await GetAllFileUploadsAsync(entityName, foreignKeyId, pageSize: 0);
                foreach (var oldFile in oldFiles.Results)
                {
                    if (File.Exists(oldFile.FilePath))
                    {
                        File.Delete(oldFile.FilePath);
                    }
                    var removeResult = await RemoveFileUploadAsync(oldFile);
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