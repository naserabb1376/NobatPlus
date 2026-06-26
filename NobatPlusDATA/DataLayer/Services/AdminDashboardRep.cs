using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class AdminDashboardRep : IAdminDashboardRep
    {
        private readonly NobatPlusContext _context;

        public AdminDashboardRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<RowResultObject<AdminDashboardReport>> GetAdminDashboardReportAsync(DateTime? fromDate = null, DateTime? toDate = null, long cityId = 0, long roleId = 0)
        {
            var result = new RowResultObject<AdminDashboardReport>();

            try
            {
                var startDate = (fromDate ?? DateTime.Now.AddDays(-30)).Date.ToShamsi();
                var endDate = (toDate ?? DateTime.Now).Date.AddDays(1).AddTicks(-1).ToShamsi();
                var today = DateTime.Now.ToShamsi().Date;

                var personsQuery = _context.Persons
                    .AsNoTracking()
                    .Include(x => x.Address).ThenInclude(x => x.City)
                    .AsQueryable();

                if (cityId > 0)
                    personsQuery = personsQuery.Where(x => x.Address != null && x.Address.CityID == cityId);

                if (roleId > 0)
                    personsQuery = personsQuery.Where(x => x.RoleId == roleId);

                var hasPersonFilter = cityId > 0 || roleId > 0;
                var persons = await personsQuery.ToListAsync();
                var personIds = persons.Select(x => x.ID).ToList();

                var customers = await _context.Customers
                    .AsNoTracking()
                    .Include(x => x.Person).ThenInclude(x => x.Address).ThenInclude(x => x.City)
                    .Where(x => !hasPersonFilter || personIds.Contains(x.PersonID))
                    .ToListAsync();

                var stylists = await _context.Stylists
                    .AsNoTracking()
                    .Include(x => x.Person).ThenInclude(x => x.Address).ThenInclude(x => x.City)
                    .Where(x => !hasPersonFilter || personIds.Contains(x.PersonID))
                    .ToListAsync();

                var stylistIds = stylists.Select(x => x.ID).ToList();
                var customerIds = customers.Select(x => x.ID).ToList();

                var bookings = await _context.Bookings
                    .AsNoTracking()
                    .Include(x => x.Customer).ThenInclude(x => x.Person).ThenInclude(x => x.Address).ThenInclude(x => x.City)
                    .Include(x => x.Stylist).ThenInclude(x => x.Person).ThenInclude(x => x.Address).ThenInclude(x => x.City)
                    .Include(x => x.PaymentBookings).ThenInclude(x => x.Payment)
                    .Include(x => x.BookingServices).ThenInclude(x => x.ServiceManagement)
                    .Where(x => x.BookingDate >= startDate && x.BookingDate <= endDate)
                    .Where(x => !hasPersonFilter || stylistIds.Contains(x.StylistID) || customerIds.Contains(x.CustomerID))
                    .ToListAsync();

                var bookingIds = bookings.Select(x => x.ID).ToList();

                var bookingStylistIds = bookings.Select(x => x.StylistID).Distinct().ToList();
                var stylistServices = await _context.StylistServices
                    .AsNoTracking()
                    .Where(x => bookingStylistIds.Contains(x.StylistID))
                    .ToListAsync();

                var reviews = await _context.Reviews
                    .AsNoTracking()
                    .Where(x => bookingIds.Contains(x.BookingID))
                    .ToListAsync();

                var settlementRequests = await _context.SettlementRequests
                    .AsNoTracking()
                    .Where(x => x.RequestDate >= startDate && x.RequestDate <= endDate)
                    .ToListAsync();

                var fileUploads = await _context.FileUploads
                    .AsNoTracking()
                    .Where(x => x.CreateDate.HasValue && x.CreateDate.Value >= startDate && x.CreateDate.Value <= endDate)
                    .ToListAsync();

                var payments = bookings.SelectMany(GetPayments).ToList();
                var salons = stylists.Where(IsSalon).ToList();
                var salonIds = salons.Select(x => x.ID).ToList();
                var nonSalonStylists = stylists.Where(x => !IsSalon(x)).ToList();

                var report = new AdminDashboardReport();

                report.Summary = new AdminDashboardSummary
                {
                    TotalUsersCount = persons.Count,
                    ActiveUsersCount = persons.Count(x => x.IsActive),
                    InactiveUsersCount = persons.Count(x => !x.IsActive),
                    CustomersCount = customers.Count,
                    SalonsCount = salons.Count,
                    StylistsCount = nonSalonStylists.Count,
                    PendingStylistsCount = stylists.Count(x => x.AccountStatus == "1"),
                    TotalBookingsCount = bookings.Count,
                    TodayBookingsCount = bookings.Count(x => x.BookingDate.Date == today),
                    CompletedBookingsCount = bookings.Count(x => !x.IsCancelled && x.Status == "4"),
                    CancelledBookingsCount = bookings.Count(x => x.IsCancelled),
                    TotalRevenue = payments.Sum(x => x.TotalServiceAmount),
                    PaidAmount = payments.Sum(x => x.PayedAmount),
                    RemainAmount = payments.Sum(x => x.RemainAmount),
                    StylistAmount = payments.Sum(x => x.StylistAmount),
                    PlatformAmount = payments.Sum(x => x.PlarformAmount),
                    VatAmount = payments.Sum(x => x.VatAmount),
                    DiscountAmount = payments.Sum(x => x.TotalServiceAmount - x.DiscountedServiceAmount),
                    FinishedPaymentsCount = payments.Count(x => x.PaymentFinished),
                    UnfinishedPaymentsCount = payments.Count(x => !x.PaymentFinished),
                    PendingSettlementRequestsCount = settlementRequests.Count(x => x.Status == "pending"),
                    PendingSettlementAmount = settlementRequests.Where(x => x.Status == "pending").Sum(x => x.Amount),
                    PaidSettlementRequestsCount = settlementRequests.Count(x => x.Status == "paid"),
                    PaidSettlementAmount = settlementRequests.Where(x => x.Status == "paid").Sum(x => x.Amount),
                    PendingDocumentsCount = fileUploads.Count(x => x.ReviewStatus == "pending"),
                    ApprovedDocumentsCount = fileUploads.Count(x => x.ReviewStatus == "approved"),
                    RejectedDocumentsCount = fileUploads.Count(x => x.ReviewStatus == "rejected"),
                    AverageRating = reviews.Any() ? (float)reviews.Average(x => x.Rating) : 0,
                    CancellationPercent = bookings.Count == 0 ? 0 : Math.Round(bookings.Count(x => x.IsCancelled) * 100.0 / bookings.Count, 1)
                };

                report.BookingTrend = BuildDateRange(startDate.Date, endDate.Date)
                    .Select(date => new ChartPointDto
                    {
                        Date = date,
                        Label = date.ToString("yyyy/MM/dd"),
                        Count = bookings.Count(x => x.BookingDate.Date == date)
                    })
                    .ToList();

                report.RevenueTrend = BuildDateRange(startDate.Date, endDate.Date)
                    .Select(date => new ChartPointDto
                    {
                        Date = date,
                        Label = date.ToString("yyyy/MM/dd"),
                        Amount = payments.Where(x => x.PaymentDate.Date == date).Sum(x => x.PayedAmount)
                    })
                    .ToList();

                report.BookingStatusBreakdown = bookings
                    .GroupBy(x => GetBookingStatusLabel(x.Status, x.IsCancelled))
                    .Select(x => new NameValueDto { Name = x.Key, Count = x.Count(), Value = x.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                report.SalonPerformance = salons
                    .Select(salon =>
                    {
                        var childIds = stylists.Where(x => x.StylistParentID == salon.ID).Select(x => x.ID).Append(salon.ID).ToList();
                        var salonBookings = bookings.Where(x => childIds.Contains(x.StylistID)).ToList();
                        var salonPayments = salonBookings.SelectMany(GetPayments).ToList();
                        var salonReviews = reviews.Where(x => childIds.Contains(x.StylistID)).ToList();

                        return new SalonPerformanceDto
                        {
                            SalonId = salon.ID,
                            SalonName = salon.StylistName,
                            StylistsCount = childIds.Count - 1,
                            BookingCount = salonBookings.Count,
                            CustomerCount = salonBookings.Select(x => x.CustomerID).Distinct().Count(),
                            Revenue = salonPayments.Sum(x => x.StylistAmount),
                            PaidAmount = salonPayments.Sum(x => x.PayedAmount),
                            RemainAmount = salonPayments.Sum(x => x.RemainAmount),
                            AverageRating = salonReviews.Any() ? (float)salonReviews.Average(x => x.Rating) : 0,
                            CancellationPercent = salonBookings.Count == 0 ? 0 : Math.Round(salonBookings.Count(x => x.IsCancelled) * 100.0 / salonBookings.Count, 1)
                        };
                    })
                    .OrderByDescending(x => x.Revenue)
                    .ThenByDescending(x => x.BookingCount)
                    .Take(10)
                    .ToList();

                report.StylistPerformance = nonSalonStylists
                    .Select(stylist =>
                    {
                        var stylistBookings = bookings.Where(x => x.StylistID == stylist.ID).ToList();
                        var stylistPayments = stylistBookings.SelectMany(GetPayments).ToList();
                        var stylistReviews = reviews.Where(x => x.StylistID == stylist.ID).ToList();

                        return new StylistPerformanceDto
                        {
                            StylistId = stylist.ID,
                            StylistName = stylist.StylistName,
                            BookingCount = stylistBookings.Count,
                            CompletedBookingCount = stylistBookings.Count(x => !x.IsCancelled && x.Status == "4"),
                            CancelledBookingCount = stylistBookings.Count(x => x.IsCancelled),
                            CustomerCount = stylistBookings.Select(x => x.CustomerID).Distinct().Count(),
                            Revenue = stylistPayments.Sum(x => x.StylistAmount),
                            PaidAmount = stylistPayments.Sum(x => x.PayedAmount),
                            RemainAmount = stylistPayments.Sum(x => x.RemainAmount),
                            AverageRating = stylistReviews.Any() ? (float)stylistReviews.Average(x => x.Rating) : 0,
                            CancellationPercent = stylistBookings.Count == 0 ? 0 : Math.Round(stylistBookings.Count(x => x.IsCancelled) * 100.0 / stylistBookings.Count, 1)
                        };
                    })
                    .OrderByDescending(x => x.Revenue)
                    .ThenByDescending(x => x.BookingCount)
                    .Take(10)
                    .ToList();

                report.ServicePerformance = bookings
                    .SelectMany(b => b.BookingServices.Select(bs => new { Booking = b, BookingService = bs }))
                    .GroupBy(x => x.BookingService.ServiceManagementID)
                    .Select(g => new ServicePerformanceDto
                    {
                        ServiceId = g.Key,
                        ServiceName = g.First().BookingService.ServiceManagement?.ServiceName ?? "نامشخص",
                        BookingCount = g.Count(),
                        Revenue = g.Sum(x => GetAllocatedServiceRevenue(x.Booking, x.BookingService, stylistServices)),
                        AveragePrice = g.Count() == 0 ? 0 : g.Sum(x => GetAllocatedServiceRevenue(x.Booking, x.BookingService, stylistServices)) / g.Count(),
                        DiscountAmount = g.Sum(x => GetAllocatedServiceDiscount(x.Booking, x.BookingService, stylistServices))
                    })
                    .OrderByDescending(x => x.Revenue)
                    .Take(10)
                    .ToList();

                report.CustomerPerformance = bookings
                    .GroupBy(x => x.CustomerID)
                    .Select(g => new CustomerPerformanceDto
                    {
                        CustomerId = g.Key,
                        CustomerName = GetPersonName(g.First().Customer?.Person),
                        PhoneNumber = g.First().Customer?.Person?.PhoneNumber ?? "",
                        BookingCount = g.Count(),
                        LastBookingDate = g.Max(x => x.BookingDate),
                        PaidAmount = g.SelectMany(GetPayments).Sum(x => x.PayedAmount)
                    })
                    .OrderByDescending(x => x.PaidAmount)
                    .ThenByDescending(x => x.BookingCount)
                    .Take(10)
                    .ToList();

                report.CityPerformance = persons
                    .Where(x => x.Address?.City != null)
                    .GroupBy(x => x.Address!.CityID)
                    .Select(g =>
                    {
                        var cityPersonIds = g.Select(x => x.ID).ToList();
                        var cityCustomerIds = customers.Where(x => cityPersonIds.Contains(x.PersonID)).Select(x => x.ID).ToList();
                        var cityStylistIds = stylists.Where(x => cityPersonIds.Contains(x.PersonID)).Select(x => x.ID).ToList();
                        var cityBookings = bookings
                            .Where(x => cityCustomerIds.Contains(x.CustomerID) || cityStylistIds.Contains(x.StylistID))
                            .ToList();

                        return new CityPerformanceDto
                        {
                            CityId = g.Key,
                            CityName = g.First().Address?.City?.CityName ?? "نامشخص",
                            UsersCount = g.Count(),
                            BookingsCount = cityBookings.Count,
                            Revenue = cityBookings.SelectMany(GetPayments).Sum(x => x.PayedAmount)
                        };
                    })
                    .OrderByDescending(x => x.Revenue)
                    .ThenByDescending(x => x.UsersCount)
                    .Take(10)
                    .ToList();

                report.SystemHealth = new List<SystemHealthDto>
                {
                    new() { Title = "کاربران غیرفعال", Count = persons.Count(x => !x.IsActive), Severity = "warning" },
                    new() { Title = "آرایشگران در انتظار تایید", Count = stylists.Count(x => x.AccountStatus == "1"), Severity = "warning" },
                    new() { Title = "مدارک در انتظار بررسی", Count = fileUploads.Count(x => x.ReviewStatus == "pending"), Severity = "warning" },
                    new() { Title = "درخواست‌های تسویه معلق", Count = settlementRequests.Count(x => x.Status == "pending"), Severity = "warning" },
                    new() { Title = "سالن‌های بدون آرایشگر", Count = salons.Count(s => !stylists.Any(x => x.StylistParentID == s.ID)), Severity = "danger" },
                    new() { Title = "آرایشگران بدون خدمت", Count = await _context.Stylists.AsNoTracking().CountAsync(s => !s.StylistServices.Any()), Severity = "danger" },
                    new() { Title = "رزروهای دارای مانده", Count = bookings.Count(b => GetPayments(b).Sum(p => p.RemainAmount) > 0), Severity = "warning" },
                    new() { Title = "رزروهای بدون پرداخت", Count = bookings.Count(b => !GetPayments(b).Any()), Severity = "danger" }
                };

                result.Result = report;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        private static bool IsSalon(Stylist stylist)
        {
            return stylist.IsWorkShop || stylist.Person?.RoleId == 3;
        }

        private static decimal GetAllocatedServiceRevenue(Booking booking, BookingService bookingService, List<StylistService> stylistServices)
        {
            var amount = GetPayments(booking).Sum(x => x.StylistAmount);
            return AllocateBookingAmountToService(booking, bookingService, stylistServices, amount);
        }

        private static decimal GetAllocatedServiceDiscount(Booking booking, BookingService bookingService, List<StylistService> stylistServices)
        {
            var amount = GetPayments(booking).Sum(x => x.TotalServiceAmount - x.DiscountedServiceAmount);
            return AllocateBookingAmountToService(booking, bookingService, stylistServices, amount);
        }

        private static decimal AllocateBookingAmountToService(Booking booking, BookingService bookingService, List<StylistService> stylistServices, decimal amount)
        {
            if (amount == 0 || booking.BookingServices == null || !booking.BookingServices.Any())
                return 0;

            var totalServicePrice = booking.BookingServices
                .Sum(x => GetBookingServicePrice(booking.StylistID, x, stylistServices));

            if (totalServicePrice <= 0)
                return amount / booking.BookingServices.Count;

            var servicePrice = GetBookingServicePrice(booking.StylistID, bookingService, stylistServices);
            return amount * servicePrice / totalServicePrice;
        }

        private static IEnumerable<Payment> GetPayments(Booking booking)
        {
            return booking.PaymentBookings?.Select(x => x.Payment).Where(x => x != null) ?? Enumerable.Empty<Payment>();
        }

        private static decimal GetBookingServicePrice(long stylistId, BookingService bookingService, List<StylistService> stylistServices)
        {
            return stylistServices
                .FirstOrDefault(x => x.StylistID == stylistId && x.ServiceManagementID == bookingService.ServiceManagementID)
                ?.ServicePrice ?? 0;
        }

        private static string GetPersonName(Person? person)
        {
            if (person == null) return "";
            return $"{person.FirstName} {person.LastName}".Trim();
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
    }
}
