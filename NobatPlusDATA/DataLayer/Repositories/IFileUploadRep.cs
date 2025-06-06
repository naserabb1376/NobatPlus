using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IFileUploadRep
    {
        Task<ListResultObject<FileUpload>> GetAllFileUploadsAsync(string entityType = "", long ForeignKeyId = 0, long creatorId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "");

        Task<RowResultObject<FileUpload>> GetFileUploadByIdAsync(long fileUploadId);

        Task<RowResultObject<FileUpload>> GetFileForDownloadAsync(long fileUploadId = 0, long foreignKeyId = 0, string entityType = "", long userId = 0, long roleId = 0);

        Task<BitResultObject> AddFileUploadAsync(FileUpload fileUpload);

        Task<BitResultObject> EditFileUploadAsync(FileUpload fileUpload);

        Task<BitResultObject> RemoveFileUploadAsync(FileUpload fileUpload);

        Task<BitResultObject> RemoveFileUploadAsync(long fileUploadId);
        Task<BitResultObject> RemoveOldFilesAsync(long foreignKeyId, string entityName);
        Task<BitResultObject> ExistFileUploadAsync(long fileUploadId);
        Task<long> GetNewRowNumber();
    }
}