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
                if (Payment.PaymentDetails != null && Payment.PaymentDetails.Any())
                {
                    var oldDetails = await _context.PaymentDetails
                        .Where(x => x.PaymentID == Payment.ID)
                        .ToListAsync();

                    if (oldDetails.Any())
                    {
                        _context.PaymentDetails.RemoveRange(oldDetails);
                    }

                    foreach (var detail in Payment.PaymentDetails)
                    {
                        detail.PaymentID = Payment.ID;
                    }
                }

                if (Payment.PaymentBookings != null && Payment.PaymentBookings.Any())
                {
                    var oldBookings = await _context.PaymentBookings
                        .Where(x => x.PaymentID == Payment.ID)
                        .ToListAsync();

                    if (oldBookings.Any())
                    {
                        _context.PaymentBookings.RemoveRange(oldBookings);
                    }

                    foreach (var paymentBooking in Payment.PaymentBookings)
                    {
                        paymentBooking.PaymentID = Payment.ID;
                    }
                }

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

        public async Task<ListResultObject<Payment>> GetAllPaymentsAsync(long bookingId = 0, long customerId = 0, int paymentIncludes = 0, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "", string paymentStatus = "", int paymentLevel = 0, DateTime? fromDate = null, DateTime? toDate = null)
        {
            ListResultObject<Payment> results = new ListResultObject<Payment>();
            try
            {
                IQueryable<Payment> query = _context.Payments
                    .Include(x => x.PaymentDetails).ThenInclude(x=> x.Stylist).ThenInclude(x=> x.Person)
                    .Include(x => x.PaymentDetails).ThenInclude(x => x.ServiceManagement)
                    .Include(x => x.PaymentDetails).ThenInclude(x => x.StylistService)
                    .Include(x => x.PaymentDetails).ThenInclude(x => x.StylistServicePriceVariant)
                    .Include(x => x.PaymentDetails).ThenInclude(x => x.OptionValues).ThenInclude(x => x.ServiceOptionValue).ThenInclude(x => x.ServiceOption)
                    .Include(x => x.PaymentBookings).ThenInclude(x => x.Booking).ThenInclude(x => x.Customer).ThenInclude(x => x.Person)
                    .Include(x => x.PaymentBookings).ThenInclude(x => x.Booking).ThenInclude(x => x.Stylist).ThenInclude(x => x.Person)
                        .AsNoTracking();

                if (customerId > 0)
                {
                    query = query.Where(x => x.PaymentBookings.Any(pb => pb.Booking.CustomerID == customerId));
                }

                if (bookingId > 0)
                {
                    query = query.Where(x=> x.PaymentBookings.Any(pb => pb.BookingID == bookingId));
                }
                if(paymentIncludes == 0)
                {
                    query = query.Where(x => !x.PaymentFinished);

                }

                if (paymentIncludes == 1)
                {
                    query = query.Where(x => x.PaymentFinished);

                }

                if (!string.IsNullOrWhiteSpace(paymentStatus))
                {
                    query = query.Where(x => x.PaymentStatus == paymentStatus);
                }

                if (paymentLevel > 0)
                {
                    query = query.Where(x => x.PaymentLevel == paymentLevel);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(x => x.PaymentDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    var endDate = toDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(x => x.PaymentDate <= endDate);
                }

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query
                           .Where(x =>
                               (
                                   (!string.IsNullOrEmpty(x.PaymentStatus.ToString()) && x.PaymentStatus.ToString().Contains(searchText)) ||
                                   (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                                   x.PaymentBookings.Any(pb =>
                                       pb.BookingID.ToString().Contains(searchText) ||
                                       pb.Booking.Customer.Person.FirstName.Contains(searchText) ||
                                       pb.Booking.Customer.Person.LastName.Contains(searchText) ||
                                       pb.Booking.Customer.Person.PhoneNumber.Contains(searchText) ||
                                       pb.Booking.Stylist.StylistName.Contains(searchText) ||
                                       pb.Booking.Stylist.Person.FirstName.Contains(searchText) ||
                                       pb.Booking.Stylist.Person.LastName.Contains(searchText) ||
                                       pb.Booking.Stylist.Person.PhoneNumber.Contains(searchText)) ||
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
                }

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
                result.Result = await _context.Payments
                    .Include(x=> x.PaymentDetails).ThenInclude(x => x.Stylist).ThenInclude(x=> x.Person)
                    .Include(x => x.PaymentDetails).ThenInclude(x => x.ServiceManagement)
                    .Include(x => x.PaymentDetails).ThenInclude(x => x.StylistService)
                    .Include(x => x.PaymentDetails).ThenInclude(x => x.StylistServicePriceVariant)
                    .Include(x => x.PaymentDetails).ThenInclude(x => x.OptionValues).ThenInclude(x => x.ServiceOptionValue).ThenInclude(x => x.ServiceOption)
                    .Include(x => x.PaymentBookings).ThenInclude(x => x.Booking).ThenInclude(x => x.Customer).ThenInclude(x => x.Person)
                    .Include(x => x.PaymentBookings).ThenInclude(x => x.Booking).ThenInclude(x => x.Stylist).ThenInclude(x => x.Person)
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
            return await CalculatePaymentAsync(customerId, new List<long> { bookingId }, discountId);
        }

        public async Task<RowResultObject<CalcPaymentDTO>> CalculatePaymentAsync(long customerId, List<long> bookingIds, long discountId)
        {
            RowResultObject<CalcPaymentDTO> result = new RowResultObject<CalcPaymentDTO>();
            result.Result = new CalcPaymentDTO();
            try
            {
                bookingIds = bookingIds
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

                if (!bookingIds.Any())
                {
                    result.Status = false;
                    result.ErrorMessage = "حداقل یک رزرو باید انتخاب شود";
                    return result;
                }

                var invalidBookingExists = await _context.Bookings
                    .AsNoTracking()
                    .AnyAsync(x => bookingIds.Contains(x.ID) && x.CustomerID != customerId);

                if (invalidBookingExists)
                {
                    result.Status = false;
                    result.ErrorMessage = "یک یا چند رزرو متعلق به این مشتری نیست";
                    return result;
                }

                var ssService = await GetAllStylistServicesAsync(customerId,bookingIds,discountId);
                if (!ssService.Any())
                {
                    result.Status = false;
                    result.ErrorMessage = "خدمتی برای این رزرو یافت نشد";
                    return result;
                }

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
            return await GetAllStylistServicesAsync(customerId, new List<long> { bookingId }, discountId);
        }

        public async Task<List<StylistServiceWithDiscountDto>> GetAllStylistServicesAsync(
long customerId,
List<long> bookingIds,
long discountId = 0
)
        {
            var results = new List<StylistServiceWithDiscountDto>();
            try
            {
                var now = DateTime.Now.ToShamsi();
                bookingIds = bookingIds.Where(x => x > 0).Distinct().ToList();

                var serviceItems = await (
                    from b in _context.Bookings.AsNoTracking()
                    join bs in _context.BookingServices.AsNoTracking()
                        on b.ID equals bs.BookingID
                    join ss in _context.StylistServices.AsNoTracking()
                        on new { b.StylistID, bs.ServiceManagementID }
                        equals new { ss.StylistID, ss.ServiceManagementID }
                    where bookingIds.Contains(b.ID)
                    select new
                {
                    BookingID = b.ID,
                    StylistID = ss.StylistID,
                    ServiceManagementID = ss.ServiceManagementID,

                    ServiceTitle = ss.ServiceManagement.ServiceName,
                    ServiceDescription = ss.ServiceManagement.Description ?? "",

                    SalonName = ss.Stylist.StylistName,
                    StylistName = $"{ss.Stylist.Person.FirstName} {ss.Stylist.Person.LastName}",

                    ServicePrice = ss.ServicePrice,
                    ServiceDuration = ss.ServiceDuration,
                    DepositPercent = ss.DepositPercent,
                    HasDynamicPricing = ss.HasDynamicPricing
                })
                    .OrderByDescending(x => x.ServiceManagementID)
                    .ToListAsync();

                foreach (var item in serviceItems)
                {
                    var resolvedPricing = await ResolvePricingAsync(
                        item.BookingID,
                        item.StylistID,
                        item.ServiceManagementID,
                        item.ServicePrice,
                        item.ServiceDuration,
                        item.DepositPercent,
                        item.HasDynamicPricing);

                    var discountPercent = await GetApplicableDiscountPercentsQuery(
                            item.StylistID,
                            item.ServiceManagementID,
                            customerId,
                            discountId,
                            now
                        )
                        .Select(x => (decimal?)x)
                        .MaxAsync() ?? 0m;

                    discountPercent = Math.Clamp(discountPercent, 0m, 100m);

                    results.Add(new StylistServiceWithDiscountDto
                    {
                        StylistID = item.StylistID,
                        ServiceManagementID = item.ServiceManagementID,
                        BookingID = item.BookingID,
                        ServiceTitle = item.ServiceTitle,
                        ServiceDescription = item.ServiceDescription,
                        SalonName = item.SalonName,
                        StylistName = item.StylistName,
                        ServicePrice = resolvedPricing.Price,
                        ServiceDuration = resolvedPricing.Duration,
                        DepositPercent = resolvedPricing.DepositPercent,
                        HasDynamicPricing = item.HasDynamicPricing,
                        StylistServicePriceVariantID = resolvedPricing.VariantId,
                        AppliedOptionValueIDs = resolvedPricing.OptionValueIds,
                        AppliedOptionSummary = resolvedPricing.OptionSummary,
                        DiscountPercent = Convert.ToInt32(discountPercent),
                        PriceAfterDiscount = resolvedPricing.Price * (1m - (discountPercent / 100m))
                    });
                }
            }
            catch (Exception ex)
            {
                
            }

            return results;
        }


        private async Task<ResolvedServicePricing> ResolvePricingAsync(
            long bookingId,
            long stylistId,
            long serviceManagementId,
            decimal basePrice,
            TimeSpan baseDuration,
            int baseDepositPercent,
            bool hasDynamicPricing)
        {
            var optionValueIds = await _context.BookingServiceOptionValues
                .AsNoTracking()
                .Where(x => x.BookingID == bookingId && x.ServiceManagementID == serviceManagementId)
                .Select(x => x.ServiceOptionValueID)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            if (!hasDynamicPricing || !optionValueIds.Any())
            {
                return new ResolvedServicePricing(basePrice, baseDuration, baseDepositPercent, null, optionValueIds, await BuildOptionSummaryAsync(optionValueIds));
            }

            var variants = await _context.StylistServicePriceVariants
                .AsNoTracking()
                .Include(x => x.OptionValues)
                .Where(x => x.StylistID == stylistId &&
                            x.ServiceManagementID == serviceManagementId &&
                            x.IsActive)
                .ToListAsync();

            var matchedVariant = variants.FirstOrDefault(x =>
                x.OptionValues.Select(ov => ov.ServiceOptionValueID).OrderBy(id => id).SequenceEqual(optionValueIds));

            if (matchedVariant == null)
            {
                return new ResolvedServicePricing(basePrice, baseDuration, baseDepositPercent, null, optionValueIds, await BuildOptionSummaryAsync(optionValueIds));
            }

            return new ResolvedServicePricing(
                matchedVariant.Price,
                matchedVariant.Duration,
                matchedVariant.DepositPercent,
                matchedVariant.ID,
                optionValueIds,
                await BuildOptionSummaryAsync(optionValueIds));
        }

        private async Task<string> BuildOptionSummaryAsync(List<long> optionValueIds)
        {
            if (optionValueIds == null || !optionValueIds.Any())
                return "";

            var optionValues = await _context.ServiceOptionValues
                .AsNoTracking()
                .Where(x => optionValueIds.Contains(x.ID))
                .Select(x => new
                {
                    x.ValueName,
                    x.ServiceOption.OptionName,
                    OptionSortOrder = x.ServiceOption.SortOrder,
                    ValueSortOrder = x.SortOrder
                })
                .ToListAsync();

            return string.Join("، ", optionValues
                .OrderBy(x => x.OptionSortOrder)
                .ThenBy(x => x.ValueSortOrder)
                .Select(x => $"{x.OptionName}: {x.ValueName}"));
        }

        private record ResolvedServicePricing(
            decimal Price,
            TimeSpan Duration,
            int DepositPercent,
            long? VariantId,
            List<long> OptionValueIds,
            string OptionSummary);

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
                      && (sd.StylistId == null || sd.StylistId <= 0 || sd.StylistId == stylistId)
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
                      && (cd.StylistId <= 0 || cd.StylistId == stylistId)
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
                       || ((da.StylistId == null || da.StylistId <= 0) && da.AdminId != null && da.AdminId > 0))
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
