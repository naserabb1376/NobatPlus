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
    public class PaymentRep : IPaymentRep
    {

        private NobatPlusContext _context;
        public PaymentRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task<BitResultObject> AddPaymentAsync(Payment Payment)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Payments.Add(Payment);
                await _context.SaveChangesAsync();
                _context.Entry(Payment).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> EditPaymentAsync(Payment Payment)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Payments.Update(Payment);
                await _context.SaveChangesAsync();
                _context.Entry(Payment).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> ExistPaymentAsync(long PaymentId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.Payments
                .AsNoTracking()
                .AnyAsync(x => x.ID == PaymentId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<ListResultObject<Payment>> GetAllPaymentsAsync(long bookingId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<Payment> results = new ListResultObject<Payment>();
            try
            {
                IQueryable<Payment> query;

                if (bookingId == 0)
                {
                    query = _context.Payments
                        .AsNoTracking()
                        .Where(x =>
                            (!string.IsNullOrEmpty(x.PaymentStatus.ToString()) && x.PaymentStatus.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Amount.ToString()) && x.Amount.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.PaymentDate.ToString()) && x.PaymentDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (x.CreateDate.HasValue && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        );
                }
                else
                {
                    query = _context.Payments
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

        public async Task<RowResultObject<Payment>> GetPaymentByIdAsync(long PaymentId)
        {
            RowResultObject<Payment> result = new RowResultObject<Payment>();
            try
            {
                result.Result = await _context.Payments
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == PaymentId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> RemovePaymentAsync(Payment Payment)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Payments.Remove(Payment);
                await _context.SaveChangesAsync();
                _context.Entry(Payment).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemovePaymentAsync(long PaymentId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var Payment = await GetPaymentByIdAsync(PaymentId);
                result = await RemovePaymentAsync(Payment.Result);
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
