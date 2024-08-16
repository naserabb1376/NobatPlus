using Domain;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Services
{
    public class PaymentRep : IPaymentRep
    {

        private NobatPlusContext _context;
        public PaymentRep()
        {
            _context = DbTools.GetDbContext();
        }

        public void AddPayment(Payment Payment)
        {
            _context.Payments.Add(Payment);
            _context.SaveChanges();
            _context.Entry(Payment).State = EntityState.Detached;
        }

        public void EditPayment(Payment Payment)
        {
            _context.Payments.Update(Payment);
            _context.SaveChanges();
            _context.Entry(Payment).State = EntityState.Detached;
        }

        public bool ExistPayment(long PaymentId)
        {
            return _context.Payments.Any(x => x.ID == PaymentId);
        }

        public List<Payment> GetAllPayments(long bookingId = 0, int pageIndex= 1, int pageSize = 20, string searchText= "")
        {
            if (bookingId == 0)
            {
                return _context.Payments.Where(x =>
            (!string.IsNullOrEmpty(x.PaymentStatus.ToString()) && x.PaymentStatus.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Amount.ToString()) && x.Amount.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.PaymentDate.ToString()) && x.PaymentDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
            else
            {
                return _context.Payments.Where(x =>
                (x.BookingID == bookingId) &&
           ((!string.IsNullOrEmpty(x.PaymentStatus.ToString()) && x.PaymentStatus.ToString().Contains(searchText))
          || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
          || (!string.IsNullOrEmpty(x.Amount.ToString()) && x.Amount.ToString().Contains(searchText))
          || (!string.IsNullOrEmpty(x.PaymentDate.ToString()) && x.PaymentDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
          || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
          || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           )).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
        }

        public Payment GetPaymentById(long PaymentId)
        {
            return _context.Payments.Find(PaymentId);
        }

        public void RemovePayment(Payment Payment)
        {
            _context.Payments.Remove(Payment);
            _context.SaveChanges();
            _context.Entry(Payment).State = EntityState.Detached;
        }

        public void RemovePayment(long PaymentId)
        {
            var Payment = GetPaymentById(PaymentId);
            RemovePayment(Payment);
        }
    }
}
