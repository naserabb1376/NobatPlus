using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IStylistServicePriceVariantOptionValueRep
    {
        Task<ListResultObject<StylistServicePriceVariantOptionValue>> GetAllStylistServicePriceVariantOptionValuesAsync(long stylistServicePriceVariantId = 0, long serviceOptionValueId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "");
        Task<RowResultObject<StylistServicePriceVariantOptionValue>> GetStylistServicePriceVariantOptionValueByIdAsync(long stylistServicePriceVariantId, long serviceOptionValueId);
        Task<BitResultObject> AddStylistServicePriceVariantOptionValuesAsync(List<StylistServicePriceVariantOptionValue> optionValues);
        Task<BitResultObject> EditStylistServicePriceVariantOptionValuesAsync(List<StylistServicePriceVariantOptionValue> optionValues);
        Task<BitResultObject> RemoveStylistServicePriceVariantOptionValuesAsync(List<StylistServicePriceVariantOptionValue> optionValues);
        Task<BitResultObject> RemoveStylistServicePriceVariantOptionValuesAsync(List<(long StylistServicePriceVariantId, long ServiceOptionValueId)> ids);
        Task<BitResultObject> ExistStylistServicePriceVariantOptionValueAsync(long stylistServicePriceVariantId, long serviceOptionValueId);
    }
}
