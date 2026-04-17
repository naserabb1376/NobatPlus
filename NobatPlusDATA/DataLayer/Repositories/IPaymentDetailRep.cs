using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IPaymentDetailRep
    {
        public Task<ListResultObject<PaymentDetail>> GetAllPaymentDetailsAsync(long stylistId = 0, long ServiceId = 0, long paymentId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<PaymentDetail>> GetPaymentDetailByIdAsync(long PaymentDetailId);
        public Task<BitResultObject> AddPaymentDetailAsync(PaymentDetail PaymentDetail);
        public Task<BitResultObject> EditPaymentDetailAsync(PaymentDetail PaymentDetail);
        public Task<BitResultObject> RemovePaymentDetailAsync(PaymentDetail PaymentDetail);
        public Task<BitResultObject> RemovePaymentDetailAsync(long PaymentDetailId);
        public Task<BitResultObject> ExistPaymentDetailAsync(long PaymentDetailId);
    }
}
