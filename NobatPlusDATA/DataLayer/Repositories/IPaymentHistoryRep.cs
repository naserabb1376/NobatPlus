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
        public List<PaymentHistory> GetAllPaymentHistories(long bookingId = 0,int pageIndex = 1,int pageSize = 20, string searchText ="");
        public PaymentHistory GetPaymentHistoryById(long PaymentHistoryId);
        public void AddPaymentHistory(PaymentHistory PaymentHistory);
        public void EditPaymentHistory(PaymentHistory PaymentHistory);
        public void RemovePaymentHistory(PaymentHistory PaymentHistory);
        public void RemovePaymentHistory(long PaymentHistoryId);
        public bool ExistPaymentHistory(long PaymentHistoryId);
    }
}
