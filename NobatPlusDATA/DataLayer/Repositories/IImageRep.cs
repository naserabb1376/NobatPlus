using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IImageRep
    {
        Task<ListResultObject<Image>> GetAllImagesAsync(string entityType="",long foreignKey=0,long creatorId=0,int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");

        Task<RowResultObject<Image>> GetImageByIdAsync(long imageId);

        Task<RowResultObject<Image>> GetImageForShowAsync(long imageId = 0, long foreignKeyId = 0, string entityType = "", long userId = 0,long roleId=0);

        Task<BitResultObject> AddImagesAsync(List<Image> images);

        Task<BitResultObject> EditImagesAsync(List<Image> images);

        Task<BitResultObject> RemoveImagesAsync(List<Image> images);

        Task<BitResultObject> RemoveImagesAsync(List<long> imageIds);
        Task<BitResultObject> RemoveOldImagesAsync(long foreignKeyId, string entityName);

        Task<BitResultObject> ExistImageAsync(long imageId);
        Task<long> GetNewRowNumber();
    }
}