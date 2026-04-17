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

        public async Task<ListResultObject<Payment>> GetAllPaymentsAsync(long bookingId = 0,long customerId =0,int paymentIncludes = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<Payment> results = new ListResultObject<Payment>();
            try
            {
                IQueryable<Payment> query = _context.Payments.Include(x => x.PaymentDetails).ThenInclude(x=> x.Stylist).ThenInclude(x=> x.Person).Include(x => x.PaymentDetails).ThenInclude(x => x.ServiceManagement)
                    .Include(x => x.Booking).ThenInclude(x => x.Customer)
                        .AsNoTracking();

                if (customerId > 0)
                {
                    query = query.Where(x => x.Booking.CustomerID == customerId);
                }

                if (bookingId > 0)
                {
                    query = query.Where(x=> x.BookingID == bookingId);
                }
                if(paymentIncludes == 0)
                {
                    query = query.Where(x => !x.PaymentFinished);

                }

                if (paymentIncludes == 1)
                {
                    query = query.Where(x => x.PaymentFinished);

                }

                query = query
                       .Where(x =>
                           (
                               (!string.IsNullOrEmpty(x.PaymentStatus.ToString()) && x.PaymentStatus.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.DepositAmount.ToString()) && x.DepositAmount.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.AllPaymentAmount.ToString()) && x.AllPaymentAmount.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.TotalServiceAmount.ToString()) && x.TotalServiceAmount.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.PlarformAmount.ToString()) && x.PlarformAmount.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.StylistAmount.ToString()) && x.StylistAmount.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.DiscountedServiceAmount.ToString()) && x.DiscountedServiceAmount.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.PayedAmount.ToString()) && x.PayedAmount.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.RemainAmount.ToString()) && x.RemainAmount.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.VatAmount.ToString()) && x.VatAmount.ToString().Contains(searchText)) ||
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
                result.Result = await _context.Payments.Include(x=> x.PaymentDetails).ThenInclude(x => x.Stylist).ThenInclude(x=> x.Person).Include(x => x.PaymentDetails).ThenInclude(x => x.ServiceManagement)
                    .Include(x => x.Booking).ThenInclude(x => x.Customer)
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

        public async Task<RowResultObject<CalcPaymentDTO>> CalculatePaymentAsync(long customerId,long bookingId,long discountId)
        {
            RowResultObject<CalcPaymentDTO> result = new RowResultObject<CalcPaymentDTO>();
            try
            {

            //    var services = await (
            //    from bs in _context.BookingServices
            //    join ss in _context.StylistServices
            //        on new { bs.ServiceManagementID }
            //        equals new { ss.ServiceManagementID }
            //    join b in _context.Bookings
            //        on bs.BookingID equals b.ID
            //    where bs.BookingID == bookingId
            //          && ss.StylistID == b.StylistID
            //    select new
            //    {
            //        ss.ServicePrice,
            //        ss.DepositPercent
            //    }
            //).ToListAsync();

                var ssService = await GetAllStylistServicesAsync(customerId,bookingId,discountId);

                decimal total = ssService.Sum(x => x.ServicePrice);
                decimal discounted = ssService.Sum(x => x.PriceAfterDiscount);
                decimal deposit = ssService.Sum(x => x.PriceAfterDiscount * x.DepositPercent / 100);
                decimal platform = decimal.Parse(_context.Settings.FirstOrDefault(x => x.Key.ToLower() == "platformamount").Value ?? "0");
                decimal stylist = discounted;
                decimal allPay = discounted + platform;
                decimal vatAmount = allPay * int.Parse(_context.Settings.FirstOrDefault(x => x.Key.ToLower() == "vatpercent").Value ?? "0") / 100;
                allPay += vatAmount;

                result.Result.StylistAmount = stylist;
                result.Result.DepositAmount = deposit;
                result.Result.TotalServiceAmount = total;
                result.Result.DiscountedServiceAmount = discounted;
                result.Result.AllPaymentAmount = allPay;
                result.Result.PlatformAmount =  platform ;
                result.Result.VatAmount =  vatAmount ;
                result.Result.PayedAmount =  vatAmount + deposit + platform ;
                result.Result.RemainAmount = allPay - result.Result.PayedAmount;
                result.Result.stylistServiceWithDiscountDtos = ssService;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }


        public async Task<List<StylistServiceWithDiscountDto>> GetAllStylistServicesAsync(
long customerId = 0,
long bookingId = 0,
long discountId = 0
)
        {
            var results = new List<StylistServiceWithDiscountDto>();
            try
            {
                var now = DateTime.Now; // یا UtcNow طبق سیاست پروژه‌ات

                var query = _context.StylistServices
                    .Include(x => x.Stylist).ThenInclude(x => x.Person)
                    .Include(x => x.ServiceManagement).ThenInclude(x => x.BookingServices)
                    .AsNoTracking()
                    .AsQueryable();

                if (bookingId > 0)
                {
                    query = query.Where(ss =>
                        ss.ServiceManagement.BookingServices.Any(bs => bs.BookingID == bookingId)
                    );
                }

               

                // ✅ Projection to DTO + Discount calc
                var dtoQuery = query.Select(ss => new StylistServiceWithDiscountDto
                {
                    StylistID = ss.StylistID,
                    ServiceManagementID = ss.ServiceManagementID,

                    ServiceTitle = ss.ServiceManagement.ServiceName,
                    ServiceDescription = ss.ServiceManagement.Description ?? "",

                    SalonName = ss.Stylist.StylistName,
                    StylistName = $"{ss.Stylist.Person.FirstName} {ss.Stylist.Person.LastName}",

                    ServicePrice = ss.ServicePrice,
                    ServiceDuration = ss.ServiceDuration,
                    DepositPercent = ss.DepositPercent,



                    DiscountPercent =
                        GetApplicableDiscountPercentsQuery(
                            ss.StylistID,
                            ss.ServiceManagementID,
                            customerId,
                            discountId,
                            now
                        )
                        .DefaultIfEmpty(0)
                        .Max(),

                    PriceAfterDiscount =
                        ss.ServicePrice *
                        (1m - (
                            GetApplicableDiscountPercentsQuery(
                                ss.StylistID,
                                ss.ServiceManagementID,
                                customerId,
                                discountId,
                                now
                            )
                            .DefaultIfEmpty(0)
                            .Max() / 100m
                        ))
                });

                // شمارش و صفحه‌بندی

                results = await dtoQuery
                    // اگر SortBy فقط روی Entity کار می‌کنه باید SortBy را بعداً برای DTO سازگار کنی
                    .OrderByDescending(x => x.ServiceManagementID) // یا CreateDate اگر داخل DTO آوردی
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                
            }

            return results;
        }


        private IQueryable<int> GetApplicableDiscountPercentsQuery(
  long stylistId,
  long serviceManagementId,
  long customerId,
  long discountId,
  DateTime now
)
        {
            // حالت 1: تخفیف‌های سرویس
            var serviceDiscounts =
                from sd in _context.ServiceDiscounts
                join d in _context.Discounts on sd.DiscountId equals d.ID
                where sd.ServiceManagementId == serviceManagementId
                      && (sd.StylistId == null || sd.StylistId == stylistId)
                      && d.StartDate <= now && d.EndDate >= now
                      && (
                            (discountId <= 0 && d.CodeRequired == false) ||
                            (discountId > 0 && d.ID == discountId)
                         )
                select d.DiscountAmount;

            // حالت 2: تخفیف‌های مشتری
            var customerDiscounts =
                from cd in _context.CustomerDiscounts
                join d in _context.Discounts on cd.DiscountId equals d.ID
                where (customerId > 0 && cd.CustomerId == customerId)
                      && cd.StylistId == stylistId
                      && d.StartDate <= now && d.EndDate >= now
                      && (
                            (discountId <= 0 && d.CodeRequired == false) ||
                            (discountId > 0 && d.ID == discountId)
                         )
                select d.DiscountAmount;

            // حالت 3: تخفیف‌های عمومی (assignment)
            var assignmentDiscounts =
                from da in _context.DiscountAssignments
                join d in _context.Discounts on da.DiscountId equals d.ID
                where (da.StylistId == stylistId
                       // اگر می‌خوای AdminId هم "عمومی" حساب شود:
                       || (da.StylistId == null && da.AdminId != null))
                      && d.StartDate <= now && d.EndDate >= now
                      && (
                            (discountId <= 0 && d.CodeRequired == false) ||
                            (discountId > 0 && d.ID == discountId)
                         )
                select d.DiscountAmount;

            return serviceDiscounts
                .Concat(customerDiscounts)
                .Concat(assignmentDiscounts);
        }
    }
}
