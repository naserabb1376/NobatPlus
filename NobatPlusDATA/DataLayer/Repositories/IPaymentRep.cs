using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IPaymentRep
    {
        public Task<ListResultObject<Payment>> GetAllPaymentsAsync(long bookingId = 0, long customerId = 0, int paymentIncludes = 0, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "", string paymentStatus = "", int paymentLevel = 0, DateTime? fromDate = null, DateTime? toDate = null);
        public Task<RowResultObject<Payment>> GetPaymentByIdAsync(long PaymentId);
        public Task<BitResultObject> AddPaymentAsync(Payment Payment);
        public Task<BitResultObject> EditPaymentAsync(Payment Payment);
        public Task<BitResultObject> RemovePaymentAsync(Payment Payment);
        public Task<BitResultObject> RemovePaymentAsync(long PaymentId);
        public Task<BitResultObject> ExistPaymentAsync(long PaymentId);
        public Task<RowResultObject<CalcPaymentDTO>> CalculatePaymentAsync(long customerId, long bookingId, long discountId);
        public Task<RowResultObject<CalcPaymentDTO>> CalculatePaymentAsync(long customerId, List<long> bookingIds, long discountId);
    }
}
