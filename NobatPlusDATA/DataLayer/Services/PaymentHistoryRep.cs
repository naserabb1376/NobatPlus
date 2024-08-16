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
    public class PaymentHistoryRep : IPaymentHistoryRep
    {

        private NobatPlusContext _context;
        public PaymentHistoryRep()
        {
            _context = DbTools.GetDbContext();
        }

        public void AddPaymentHistory(PaymentHistory PaymentHistory)
        {
            _context.PaymentHistories.Add(PaymentHistory);
            _context.SaveChanges();
            _context.Entry(PaymentHistory).State = EntityState.Detached;
        }

        public void EditPaymentHistory(PaymentHistory PaymentHistory)
        {
            _context.PaymentHistories.Update(PaymentHistory);
            _context.SaveChanges();
            _context.Entry(PaymentHistory).State = EntityState.Detached;
        }

        public bool ExistPaymentHistory(long PaymentHistoryId)
        {
            return _context.PaymentHistories.Any(x => x.ID == PaymentHistoryId);
        }

        public List<PaymentHistory> GetAllPaymentHistories(long bookingId = 0, int pageIndex= 1, int pageSize = 20, string searchText= "")
        {
            if (bookingId == 0)
            {
                return _context.PaymentHistories.Where(x =>
            (!string.IsNullOrEmpty(x.PaymentMethod.ToString()) && x.PaymentMethod.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Amount.ToString()) && x.Amount.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.PaymentDate.ToString()) && x.PaymentDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
            else
            {
                return _context.PaymentHistories.Where(x =>
                (x.BookingID == bookingId) &&
           ((!string.IsNullOrEmpty(x.PaymentMethod.ToString()) && x.PaymentMethod.ToString().Contains(searchText))
          || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
          || (!string.IsNullOrEmpty(x.Amount.ToString()) && x.Amount.ToString().Contains(searchText))
          || (!string.IsNullOrEmpty(x.PaymentDate.ToString()) && x.PaymentDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
          || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
          || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           )).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
        }

        public PaymentHistory GetPaymentHistoryById(long PaymentHistoryId)
        {
            return _context.PaymentHistories.Find(PaymentHistoryId);
        }

        public void RemovePaymentHistory(PaymentHistory PaymentHistory)
        {
            _context.PaymentHistories.Remove(PaymentHistory);
            _context.SaveChanges();
            _context.Entry(PaymentHistory).State = EntityState.Detached;
        }

        public void RemovePaymentHistory(long PaymentHistoryId)
        {
            var PaymentHistory = GetPaymentHistoryById(PaymentHistoryId);
            RemovePaymentHistory(PaymentHistory);
        }
    }
}
