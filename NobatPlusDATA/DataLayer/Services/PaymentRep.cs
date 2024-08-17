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

        public async Task AddPaymentAsync(Payment Payment)
        {
            _context.Payments.Add(Payment);
            await _context.SaveChangesAsync();
            _context.Entry(Payment).State = EntityState.Detached;
        }

        public async Task EditPaymentAsync(Payment Payment)
        {
            _context.Payments.Update(Payment);
            await _context.SaveChangesAsync();
            _context.Entry(Payment).State = EntityState.Detached;
        }

        public async Task<bool> ExistPaymentAsync(long PaymentId)
        {
            return await _context.Payments
                .AsNoTracking()
                .AnyAsync(x => x.ID == PaymentId);
        }

        public async Task<List<Payment>> GetAllPaymentsAsync(long bookingId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            if (bookingId == 0)
            {
                return await _context.Payments
                    .AsNoTracking()
                    .Where(x =>
                        (!string.IsNullOrEmpty(x.PaymentStatus.ToString()) && x.PaymentStatus.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Amount.ToString()) && x.Amount.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.PaymentDate.ToString()) && x.PaymentDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                        (x.CreateDate.HasValue && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                        (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    )
                    .OrderByDescending(x => x.CreateDate)
                    .ToPaging(pageIndex, pageSize)
                    .ToListAsync();
            }
            else
            {
                return await _context.Payments
                    .AsNoTracking()
                    .Where(x =>
                        x.BookingID == bookingId &&
                        (
                            (!string.IsNullOrEmpty(x.PaymentStatus.ToString()) && x.PaymentStatus.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Amount.ToString()) && x.Amount.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.PaymentDate.ToString()) && x.PaymentDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (x.CreateDate.HasValue && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        )
                    )
                    .OrderByDescending(x => x.CreateDate)
                    .ToPaging(pageIndex, pageSize)
                    .ToListAsync();
            }
        }

        public async Task<Payment> GetPaymentByIdAsync(long PaymentId)
        {
            return await _context.Payments
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == PaymentId);
        }

        public async Task RemovePaymentAsync(Payment Payment)
        {
            _context.Payments.Remove(Payment);
            await _context.SaveChangesAsync();
            _context.Entry(Payment).State = EntityState.Detached;
        }

        public async Task RemovePaymentAsync(long PaymentId)
        {
            var Payment = await GetPaymentByIdAsync(PaymentId);
            await RemovePaymentAsync(Payment);
        }
    }
}
