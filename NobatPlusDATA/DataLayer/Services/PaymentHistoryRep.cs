using Domain;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
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

        public async Task<BitResultObject> AddPaymentHistoryAsync(PaymentHistory PaymentHistory)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.PaymentHistories.AddAsync(PaymentHistory);
                await _context.SaveChangesAsync();
                result.ID = PaymentHistory.ID;
                _context.Entry(PaymentHistory).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> EditPaymentHistoryAsync(PaymentHistory PaymentHistory)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.PaymentHistories.Update(PaymentHistory);
                await _context.SaveChangesAsync();
                result.ID = PaymentHistory.ID;
                _context.Entry(PaymentHistory).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> ExistPaymentHistoryAsync(long PaymentHistoryId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.PaymentHistories
                .AsNoTracking()
                .AnyAsync(x => x.ID == PaymentHistoryId);
                result.ID = PaymentHistoryId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<ListResultObject<PaymentHistory>> GetAllPaymentHistoriesAsync(long bookingId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<PaymentHistory> results = new ListResultObject<PaymentHistory>();
            try
            {
                IQueryable<PaymentHistory> query;

                if (bookingId == 0)
                {
                    query = _context.PaymentHistories
                         .AsNoTracking()
                         .Where(x =>
                             (!string.IsNullOrEmpty(x.PaymentMethod) && x.PaymentMethod.Contains(searchText)) ||
                             (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                             (!string.IsNullOrEmpty(x.Amount.ToString()) && x.Amount.ToString().Contains(searchText)) ||
                             (!string.IsNullOrEmpty(x.PaymentDate.ToString()) && x.PaymentDate.ToString().Contains(searchText)) ||
                             (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                             (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                         );
                }
                else
                {
                    query = _context.PaymentHistories
                        .AsNoTracking()
                        .Where(x =>
                            x.BookingID == bookingId &&
                            (
                                (!string.IsNullOrEmpty(x.PaymentMethod) && x.PaymentMethod.Contains(searchText)) ||
                                (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                                (!string.IsNullOrEmpty(x.Amount.ToString()) && x.Amount.ToString().Contains(searchText)) ||
                                (!string.IsNullOrEmpty(x.PaymentDate.ToString()) && x.PaymentDate.ToString().Contains(searchText)) ||
                                (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                                (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                            )
                        );
                }
                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
            
        }

        public async Task<RowResultObject<PaymentHistory>> GetPaymentHistoryByIdAsync(long PaymentHistoryId)
        {
            RowResultObject<PaymentHistory> result = new RowResultObject<PaymentHistory>();
            try
            {
                result.Result = await _context.PaymentHistories
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == PaymentHistoryId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemovePaymentHistoryAsync(PaymentHistory PaymentHistory)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.PaymentHistories.Remove(PaymentHistory);
                await _context.SaveChangesAsync();
                result.ID = PaymentHistory.ID;
                _context.Entry(PaymentHistory).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemovePaymentHistoryAsync(long PaymentHistoryId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var PaymentHistory = await GetPaymentHistoryByIdAsync(PaymentHistoryId);
                result = await RemovePaymentHistoryAsync(PaymentHistory.Result);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }
    }
}
