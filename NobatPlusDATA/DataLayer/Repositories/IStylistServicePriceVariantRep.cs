using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IStylistServicePriceVariantRep
    {
        Task<ListResultObject<StylistServicePriceVariant>> GetAllStylistServicePriceVariantsAsync(long stylistId = 0, long serviceManagementId = 0, int isActive = -1, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "");
        Task<RowResultObject<StylistServicePriceVariant>> GetStylistServicePriceVariantByIdAsync(long stylistServicePriceVariantId);
        Task<BitResultObject> AddStylistServicePriceVariantAsync(StylistServicePriceVariant stylistServicePriceVariant);
        Task<BitResultObject> EditStylistServicePriceVariantAsync(StylistServicePriceVariant stylistServicePriceVariant);
        Task<BitResultObject> RemoveStylistServicePriceVariantAsync(long stylistServicePriceVariantId);
        Task<BitResultObject> ExistStylistServicePriceVariantAsync(long stylistServicePriceVariantId);
    }
}
