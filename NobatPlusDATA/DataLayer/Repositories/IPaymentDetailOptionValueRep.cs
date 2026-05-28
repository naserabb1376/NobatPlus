using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IPaymentDetailOptionValueRep
    {
        Task<ListResultObject<PaymentDetailOptionValue>> GetAllPaymentDetailOptionValuesAsync(long paymentDetailId = 0, long serviceOptionValueId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "");
        Task<RowResultObject<PaymentDetailOptionValue>> GetPaymentDetailOptionValueByIdAsync(long paymentDetailId, long serviceOptionValueId);
        Task<BitResultObject> AddPaymentDetailOptionValuesAsync(List<PaymentDetailOptionValue> optionValues);
        Task<BitResultObject> EditPaymentDetailOptionValuesAsync(List<PaymentDetailOptionValue> optionValues);
        Task<BitResultObject> RemovePaymentDetailOptionValuesAsync(List<PaymentDetailOptionValue> optionValues);
        Task<BitResultObject> RemovePaymentDetailOptionValuesAsync(List<(long PaymentDetailId, long ServiceOptionValueId)> ids);
        Task<BitResultObject> ExistPaymentDetailOptionValueAsync(long paymentDetailId, long serviceOptionValueId);
    }
}
