using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IPaymentRep
    {
        public Task<List<Payment>> GetAllPaymentsAsync(long bookingId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<Payment> GetPaymentByIdAsync(long PaymentId);
        public Task AddPaymentAsync(Payment Payment);
        public Task EditPaymentAsync(Payment Payment);
        public Task RemovePaymentAsync(Payment Payment);
        public Task RemovePaymentAsync(long PaymentId);
        public Task<bool> ExistPaymentAsync(long PaymentId);
    }
}
