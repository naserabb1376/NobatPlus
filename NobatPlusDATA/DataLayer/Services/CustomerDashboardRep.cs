using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class CustomerDashboardRep : ICustomerDashboardRep
    {
        private readonly NobatPlusContext _context;

        public CustomerDashboardRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<RowResultObject<CustomerDashboardReport>> GetCustomerDashboardReportAsync(long personId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var result = new RowResultObject<CustomerDashboardReport>();

            try
            {
                var customer = await _context.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.PersonID == personId);

                if (customer == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "مشتری یافت نشد";
                    return result;
                }

                var startDate = (fromDate ?? DateTime.Now.AddDays(-30)).Date.ToShamsi();
                var endDate = (toDate ?? DateTime.Now).Date.AddDays(1).AddTicks(-1).ToShamsi();
                var today = DateTime.Now.ToShamsi().Date;
                var monthStart = DateTime.Now.ToShamsi().Date.AddDays(-30);

                var bookings = await _context.Bookings
                    .AsNoTracking()
                    .Include(x => x.Stylist)
                    .Include(x => x.PaymentBookings).ThenInclude(x => x.Payment)
                    .Include(x => x.BookingServices).ThenInclude(x => x.ServiceManagement)
                    .Where(x => x.CustomerID == customer.ID)
                    .ToListAsync();

                var rangeBookings = bookings
                    .Where(x => x.BookingDate >= startDate && x.BookingDate <= endDate)
                    .ToList();

                var bookingIds = bookings.Select(x => x.ID).ToList();
                var bookingStylistIds = bookings.Select(x => x.StylistID).Distinct().ToList();

                var stylistServices = await _context.StylistServices
                    .AsNoTracking()
                    .Where(x => bookingStylistIds.Contains(x.StylistID))
                    .ToListAsync();

                var reviews = await _context.Reviews
                    .AsNoTracking()
                    .Where(x => x.CustomerID == customer.ID && bookingIds.Contains(x.BookingID))
                    .ToListAsync();

                var activeDiscountsCount = await _context.CustomerDiscounts
                    .AsNoTracking()
                    .Include(x => x.Discount)
                    .CountAsync(x => x.CustomerId == customer.ID && x.Discount.StartDate <= today && x.Discount.EndDate >= today);

                var notificationsCount = await _context.Notifications
                    .AsNoTracking()
                    .CountAsync(x => x.PersonID == personId);

                var payments = rangeBookings
                    .SelectMany(GetPayments)
                    .ToList();

                var report = new CustomerDashboardReport();
                var completedBookings = bookings.Where(x => !x.IsCancelled && x.Status == "4").ToList();
                var pendingReviewBookings = completedBookings
                    .Where(x => !reviews.Any(r => r.BookingID == x.ID))
                    .OrderByDescending(x => x.BookingDate)
                    .Take(5)
                    .ToList();

                report.Summary = new CustomerDashboardSummary
                {
                    TodayAppointmentsCount = bookings.Count(x => x.BookingDate.Date == today && !x.IsCancelled),
                    UpcomingAppointmentsCount = bookings.Count(x => x.BookingDate.Date >= today && !x.IsCancelled),
                    CompletedAppointmentsCount = completedBookings.Count,
                    CancelledAppointmentsCount = bookings.Count(x => x.IsCancelled),
                    ActiveDiscountsCount = activeDiscountsCount,
                    UnreadNotificationsCount = notificationsCount,
                    PendingReviewsCount = completedBookings.Count(x => !reviews.Any(r => r.BookingID == x.ID)),
                    PaidAmount = payments.Sum(x => x.PayedAmount),
                    RemainAmount = payments.Sum(x => x.RemainAmount),
                    MonthPaidAmount = bookings
                    .SelectMany(GetPayments)
                        .Where(x => x.PaymentDate >= monthStart && x.PaymentDate <= today.AddDays(1).AddTicks(-1))
                        .Sum(x => x.PayedAmount)
                };

                report.TodayAppointments = bookings
                    .Where(x => x.BookingDate.Date == today && !x.IsCancelled)
                    .OrderBy(x => x.BookingDate)
                    .Select(ToAppointmentDto)
                    .ToList();

                report.UpcomingAppointments = bookings
                    .Where(x => x.BookingDate.Date >= today && !x.IsCancelled)
                    .OrderBy(x => x.BookingDate)
                    .Take(6)
                    .Select(ToAppointmentDto)
                    .ToList();

                report.RecentPayments = bookings
                    .SelectMany(booking => GetPayments(booking)
                        .Select(payment => ToPaymentDto(booking, payment)))
                    .OrderByDescending(x => x.PaymentDate)
                    .Take(6)
                    .ToList();

                report.RecentServices = bookings
                    .SelectMany(b => b.BookingServices.Select(bs => new { Booking = b, BookingService = bs }))
                    .GroupBy(x => x.BookingService.ServiceManagementID)
                    .Select(g => new CustomerServiceHistoryDto
                    {
                        ServiceId = g.Key,
                        ServiceName = g.First().BookingService.ServiceManagement?.ServiceName ?? "نامشخص",
                        BookingCount = g.Count(),
                        LastBookingDate = g.Max(x => x.Booking.BookingDate),
                        LastBookingDateText = FormatDate(g.Max(x => x.Booking.BookingDate)),
                        PaidAmount = g.Sum(x => GetAllocatedServiceRevenue(x.Booking, x.BookingService, stylistServices))
                    })
                    .OrderByDescending(x => x.LastBookingDate)
                    .Take(6)
                    .ToList();

                report.RecentStylists = bookings
                    .GroupBy(x => x.StylistID)
                    .Select(g => new CustomerStylistHistoryDto
                    {
                        StylistId = g.Key,
                        StylistName = g.First().Stylist?.StylistName ?? "نامشخص",
                        BookingCount = g.Count(),
                        LastBookingDate = g.Max(x => x.BookingDate),
                        LastBookingDateText = FormatDate(g.Max(x => x.BookingDate)),
                        PaidAmount = g.SelectMany(GetPayments).Sum(x => x.PayedAmount)
                    })
                    .OrderByDescending(x => x.LastBookingDate)
                    .Take(6)
                    .ToList();

                report.PendingReviews = pendingReviewBookings
                    .Select(x => new CustomerPendingReviewDto
                    {
                        BookingId = x.ID,
                        Date = FormatDate(x.BookingDate),
                        Services = GetServicesTitle(x),
                        StylistName = x.Stylist?.StylistName ?? "نامشخص"
                    })
                    .ToList();

                report.BookingTrend = BuildDateRange(startDate.Date, endDate.Date)
                    .Select(date => new ChartPointDto
                    {
                        Date = date,
                        Label = date.ToString("yyyy/MM/dd"),
                        Count = rangeBookings.Count(x => x.BookingDate.Date == date)
                    })
                    .ToList();

                result.Result = report;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        private static CustomerAppointmentDto ToAppointmentDto(Booking booking)
        {
            var payment = GetPayments(booking).OrderByDescending(x => x.PaymentDate).FirstOrDefault();
            return new CustomerAppointmentDto
            {
                BookingId = booking.ID,
                BookingDate = booking.BookingDate,
                Date = FormatDate(booking.BookingDate),
                Time = booking.BookingDate.ToString("HH:mm"),
                Services = GetServicesTitle(booking),
                StylistName = booking.Stylist?.StylistName ?? "نامشخص",
                Amount = payment?.DiscountedServiceAmount ?? payment?.TotalServiceAmount ?? 0,
                PaidAmount = payment?.PayedAmount ?? 0,
                RemainAmount = payment?.RemainAmount ?? 0,
                Status = GetBookingStatusLabel(booking.Status, booking.IsCancelled),
                IsCancelled = booking.IsCancelled
            };
        }

        private static CustomerPaymentDto ToPaymentDto(Booking booking, Payment payment)
        {
            return new CustomerPaymentDto
            {
                PaymentId = payment.ID,
                BookingId = booking.ID,
                PaymentDate = payment.PaymentDate,
                Date = FormatDate(payment.PaymentDate),
                Services = GetServicesTitle(booking),
                StylistName = booking.Stylist?.StylistName ?? "نامشخص",
                Amount = payment.AllPaymentAmount,
                PaidAmount = payment.PayedAmount,
                RemainAmount = payment.RemainAmount,
                Status = payment.PaymentStatus ?? ""
            };
        }

        private static string GetServicesTitle(Booking booking)
        {
            var names = booking.BookingServices?
                .Select(x => x.ServiceManagement?.ServiceName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList() ?? new List<string?>();

            return names.Any() ? string.Join("، ", names) : "خدمت نامشخص";
        }

        private static decimal GetAllocatedServiceRevenue(Booking booking, BookingService bookingService, List<StylistService> stylistServices)
        {
            var amount = GetPayments(booking).Sum(x => x.StylistAmount);
            if (amount == 0 || booking.BookingServices == null || !booking.BookingServices.Any())
                return 0;

            var totalServicePrice = booking.BookingServices
                .Sum(x => GetBookingServicePrice(booking.StylistID, x, stylistServices));

            if (totalServicePrice <= 0)
                return amount / booking.BookingServices.Count;

            var servicePrice = GetBookingServicePrice(booking.StylistID, bookingService, stylistServices);
            return amount * servicePrice / totalServicePrice;
        }

        private static decimal GetBookingServicePrice(long stylistId, BookingService bookingService, List<StylistService> stylistServices)
        {
            return stylistServices
                .FirstOrDefault(x => x.StylistID == stylistId && x.ServiceManagementID == bookingService.ServiceManagementID)
                ?.ServicePrice ?? 0;
        }

        private static string FormatDate(DateTime date)
        {
            return date.ToString("yyyy/MM/dd");
        }

        private static IEnumerable<DateTime> BuildDateRange(DateTime startDate, DateTime endDate)
        {
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                yield return date;
        }

        private static string GetBookingStatusLabel(string status, bool isCancelled)
        {
            if (isCancelled || status == "2") return "لغو شده";
            return status switch
            {
                "1" => "در انتظار",
                "3" => "عدم حضور",
                "4" => "انجام شده",
                _ => string.IsNullOrWhiteSpace(status) ? "بدون وضعیت" : status
            };
        }

        private static IEnumerable<Payment> GetPayments(Booking booking)
        {
            return booking.PaymentBookings?.Select(x => x.Payment).Where(x => x != null) ?? Enumerable.Empty<Payment>();
        }
    }
}
