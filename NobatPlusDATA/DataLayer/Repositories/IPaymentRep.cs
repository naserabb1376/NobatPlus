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
        public List<Payment> GetAllPayments(long bookingId = 0,int pageIndex = 1,int pageSize = 20, string searchText ="");
        public Payment GetPaymentById(long PaymentId);
        public void AddPayment(Payment Payment);
        public void EditPayment(Payment Payment);
        public void RemovePayment(Payment Payment);
        public void RemovePayment(long PaymentId);
        public bool ExistPayment(long PaymentId);
    }
}
