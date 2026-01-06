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
        public PaymentRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddPaymentAsync(Payment Payment)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.Payments.AddAsync(Payment);
                await _context.SaveChangesAsync();
                result.ID = Payment.ID;
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
                result.ID = Payment.ID;
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
                result.ID = PaymentId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<ListResultObject<Payment>> GetAllPaymentsAsync(long bookingId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<Payment> results = new ListResultObject<Payment>();
            try
            {
                IQueryable<Payment> query = _context.Payments.Include(x => x.Booking).ThenInclude(x => x.Customer).Include(x => x.Booking).ThenInclude(x => x.Stylist)
                        .AsNoTracking();

                if (bookingId > 0)
                {
                    query = query.Where(x=> x.BookingID == bookingId);
                }

                query = query
                       .Where(x =>
                           x.BookingID == bookingId &&
                           (
                               (!string.IsNullOrEmpty(x.PaymentStatus.ToString()) && x.PaymentStatus.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.DepositAmount.ToString()) && x.DepositAmount.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.AllPaymentAmount.ToString()) && x.AllPaymentAmount.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.TotalServiceAmount.ToString()) && x.TotalServiceAmount.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.PlarformAmount.ToString()) && x.PlarformAmount.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.StylistAmount.ToString()) && x.StylistAmount.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.PaymentDate.ToString()) && x.PaymentDate.ToString().Contains(searchText)) ||
                               (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                               (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                           )
                       );

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
                .SortBy(sortQuery).ToPaging(pageIndex, pageSize)
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
                result.Result = await _context.Payments.Include(x => x.Booking).ThenInclude(x => x.Customer).Include(x => x.Booking).ThenInclude(x => x.Stylist)
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
                result.ID = Payment.ID;
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

        public async Task<RowResultObject<CalcPaymentDTO>> CalculatePaymentAsync(long bookingId)
        {
            RowResultObject<CalcPaymentDTO> result = new RowResultObject<CalcPaymentDTO>();
            try
            {
                var services = await (
                from bs in _context.BookingServices
                join ss in _context.StylistServices
                    on new { bs.ServiceManagementID }
                    equals new { ss.ServiceManagementID }
                join b in _context.Bookings
                    on bs.BookingID equals b.ID
                where bs.BookingID == bookingId
                      && ss.StylistID == b.StylistID
                select new
                {
                    ss.ServicePrice,
                    ss.DepositPercent
                }
            ).ToListAsync();

                decimal total = services.Sum(x => x.ServicePrice);
                decimal deposit = services.Sum(x => x.ServicePrice * x.DepositPercent / 100);
                decimal platform = decimal.Parse(_context.Settings.FirstOrDefault(x => x.Key.ToLower() == "platformamount").Value ?? "0");
                decimal stylist = total;
                decimal allPay = total + platform;


                result.Result.StylistAmount = stylist;
                result.Result.DepositAmount = deposit;
                result.Result.TotalServiceAmount = total;
                result.Result.AllPaymentAmount = allPay;
                result.Result.PlatformAmount =  platform ;
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
