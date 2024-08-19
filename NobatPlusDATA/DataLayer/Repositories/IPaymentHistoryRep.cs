using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IPaymentHistoryRep
    {
        public Task<ListResultObject<PaymentHistory>> GetAllPaymentHistoriesAsync(long bookingId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<RowResultObject<PaymentHistory>> GetPaymentHistoryByIdAsync(long PaymentHistoryId);
        public Task<BitResultObject> AddPaymentHistoryAsync(PaymentHistory PaymentHistory);
        public Task<BitResultObject> EditPaymentHistoryAsync(PaymentHistory PaymentHistory);
        public Task<BitResultObject> RemovePaymentHistoryAsync(PaymentHistory PaymentHistory);
        public Task<BitResultObject> RemovePaymentHistoryAsync(long PaymentHistoryId);
        public Task<BitResultObject> ExistPaymentHistoryAsync(long PaymentHistoryId);
    }
}
