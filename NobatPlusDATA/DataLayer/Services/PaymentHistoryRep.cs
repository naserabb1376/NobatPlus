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

        public async Task AddPaymentHistoryAsync(PaymentHistory PaymentHistory)
        {
            _context.PaymentHistories.Add(PaymentHistory);
            await _context.SaveChangesAsync();
            _context.Entry(PaymentHistory).State = EntityState.Detached;
        }

        public async Task EditPaymentHistoryAsync(PaymentHistory PaymentHistory)
        {
            _context.PaymentHistories.Update(PaymentHistory);
            await _context.SaveChangesAsync();
            _context.Entry(PaymentHistory).State = EntityState.Detached;
        }

        public async Task<bool> ExistPaymentHistoryAsync(long PaymentHistoryId)
        {
            return await _context.PaymentHistories
                .AsNoTracking()
                .AnyAsync(x => x.ID == PaymentHistoryId);
        }

        public async Task<List<PaymentHistory>> GetAllPaymentHistoriesAsync(long bookingId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            if (bookingId == 0)
            {
                return await _context.PaymentHistories
                    .AsNoTracking()
                    .Where(x =>
                        (!string.IsNullOrEmpty(x.PaymentMethod) && x.PaymentMethod.Contains(searchText)) ||
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
                return await _context.PaymentHistories
                    .AsNoTracking()
                    .Where(x =>
                        x.BookingID == bookingId &&
                        (
                            (!string.IsNullOrEmpty(x.PaymentMethod) && x.PaymentMethod.Contains(searchText)) ||
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

        public async Task<PaymentHistory> GetPaymentHistoryByIdAsync(long PaymentHistoryId)
        {
            return await _context.PaymentHistories
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == PaymentHistoryId);
        }

        public async Task RemovePaymentHistoryAsync(PaymentHistory PaymentHistory)
        {
            _context.PaymentHistories.Remove(PaymentHistory);
            await _context.SaveChangesAsync();
            _context.Entry(PaymentHistory).State = EntityState.Detached;
        }

        public async Task RemovePaymentHistoryAsync(long PaymentHistoryId)
        {
            var PaymentHistory = await GetPaymentHistoryByIdAsync(PaymentHistoryId);
            await RemovePaymentHistoryAsync(PaymentHistory);
        }
    }
}
