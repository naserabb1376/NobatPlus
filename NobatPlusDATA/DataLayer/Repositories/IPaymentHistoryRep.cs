using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IPaymentHistoryRep
    {
        public Task<List<PaymentHistory>> GetAllPaymentHistoriesAsync(long bookingId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<PaymentHistory> GetPaymentHistoryByIdAsync(long PaymentHistoryId);
        public Task AddPaymentHistoryAsync(PaymentHistory PaymentHistory);
        public Task EditPaymentHistoryAsync(PaymentHistory PaymentHistory);
        public Task RemovePaymentHistoryAsync(PaymentHistory PaymentHistory);
        public Task RemovePaymentHistoryAsync(long PaymentHistoryId);
        public Task<bool> ExistPaymentHistoryAsync(long PaymentHistoryId);
    }
}
