using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class StylistDashboardRep : IStylistDashboardRep
    {
        private readonly NobatPlusContext _context;

        public StylistDashboardRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<RowResultObject<StylistDashboardReport>> GetStylistDashboardReportAsync(long stylistId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var result = new RowResultObject<StylistDashboardReport>();

            try
            {
                var startDate = (fromDate ?? DateTime.Now.AddDays(-30)).Date;
                var endDate = (toDate ?? DateTime.Now).Date.AddDays(1).AddTicks(-1);
                var shamsiStartDate = startDate.ToShamsi();
                var shamsiEndDate = endDate.ToShamsi();
                var today = DateTime.Now.Date;

                var bookings = await _context.Bookings
                    .AsNoTracking()
                    .Include(x => x.Customer).ThenInclude(x => x.Person)
                    .Include(x => x.PaymentBookings).ThenInclude(x => x.Payment)
                    .Include(x => x.BookingServices).ThenInclude(x => x.ServiceManagement)
                    .Where(x => x.StylistID == stylistId && x.BookingDate >= startDate && x.BookingDate <= endDate)
                    .ToListAsync();

                var stylistServices = await _context.StylistServices
                    .AsNoTracking()
                    .Include(x => x.ServiceManagement)
                    .Where(x => x.StylistID == stylistId)
                    .ToListAsync();

                var reviews = await _context.Reviews
                    .AsNoTracking()
                    .Where(x => x.StylistID == stylistId && x.ReviewDate >= shamsiStartDate && x.ReviewDate <= shamsiEndDate)
                    .ToListAsync();

                var rateHistories = await _context.RateHistories
                    .AsNoTracking()
                    .Include(x => x.RateQuestion)
                    .Where(x => x.StylistID == stylistId && x.RateDate >= shamsiStartDate && x.RateDate <= shamsiEndDate)
                    .ToListAsync();

                var workTimes = await _context.WorkTimes
                    .AsNoTracking()
                    .Where(x => x.StylistID == stylistId)
                    .ToListAsync();

                var payments = bookings.SelectMany(GetPayments).ToList();
                var report = new StylistDashboardReport();

                report.BookingTrend = BuildDateRange(startDate.Date, endDate.Date)
                    .Select(date => new ChartPointDto
                    {
                        Date = date,
                        Label = date.ToShamsiString().Split(' ')[0],
                        Count = bookings.Count(x => x.BookingDate.Date == date)
                    })
                    .ToList();

                report.RevenueTrend = BuildDateRange(startDate.Date, endDate.Date)
                    .Select(date => new ChartPointDto
                    {
                        Date = date,
                        Label = date.ToShamsiString().Split(' ')[0],
                        Amount = bookings
                            .Where(x => x.BookingDate.Date == date)
                            .SelectMany(GetPayments)
                            .Sum(x => x.StylistAmount)
                    })
                    .ToList();

                report.BookingStatusBreakdown = bookings
                    .GroupBy(x => GetBookingStatusLabel(x.Status, x.IsCancelled))
                    .Select(x => new NameValueDto { Name = x.Key, Count = x.Count(), Value = x.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                report.ServicePerformance = bookings
                    .SelectMany(b => b.BookingServices.Select(bs => new { Booking = b, BookingService = bs }))
                    .GroupBy(x => x.BookingService.ServiceManagementID)
                    .Select(g =>
                    {
                        var service = stylistServices.FirstOrDefault(x => x.ServiceManagementID == g.Key);
                        var revenue = g.Sum(x => GetAllocatedServiceRevenue(x.Booking, x.BookingService, stylistServices));
                        var discountAmount = g.Sum(x => GetAllocatedServiceDiscount(x.Booking, x.BookingService, stylistServices));

                        return new ServicePerformanceDto
                        {
                            ServiceId = g.Key,
                            ServiceName = service?.ServiceManagement?.ServiceName ?? g.First().BookingService.ServiceManagement?.ServiceName ?? "نامشخص",
                            BookingCount = g.Count(),
                            Revenue = revenue,
                            AveragePrice = g.Count() == 0 ? 0 : revenue / g.Count(),
                            DiscountAmount = discountAmount
                        };
                    })
                    .OrderByDescending(x => x.BookingCount)
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
                    .OrderByDescending(x => x.BookingCount)
                    .Take(10)
                    .ToList();

                report.RatingDistribution = Enumerable.Range(1, 5)
                    .Select(score => new NameValueDto
                    {
                        Name = $"{score} ستاره",
                        Count = reviews.Count(x => x.Rating == score),
                        Value = reviews.Count(x => x.Rating == score)
                    })
                    .ToList();

                report.RatingQuestions = rateHistories
                    .GroupBy(x => x.RateQuestionID)
                    .Select(g => new RatingQuestionDto
                    {
                        RateQuestionId = g.Key,
                        QuestionText = g.First().RateQuestion?.RateQuestionText ?? "",
                        AverageScore = g.Any() ? g.Average(x => x.RateScore) : 0,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.AverageScore)
                    .ToList();

                report.CancellationTrend = BuildDateRange(startDate.Date, endDate.Date)
                    .Select(date => new ChartPointDto
                    {
                        Date = date,
                        Label = date.ToShamsiString().Split(' ')[0],
                        Count = bookings.Count(x => x.BookingDate.Date == date && x.IsCancelled)
                    })
                    .ToList();

                report.CancellationReasons = bookings
                    .Where(x => x.IsCancelled)
                    .GroupBy(x => string.IsNullOrWhiteSpace(x.CancelReason) ? "بدون دلیل" : x.CancelReason)
                    .Select(x => new NameValueDto { Name = x.Key, Count = x.Count(), Value = x.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(8)
                    .ToList();

                report.WorkTimeCapacity = workTimes
                    .Select(workTime =>
                    {
                        var dayBookings = bookings.Where(x => MatchDayOfWeek(workTime.DayOfWeek, x.BookingDate.DayOfWeek)).ToList();
                        var bookedMinutes = dayBookings
                            .SelectMany(x => x.BookingServices)
                            .Sum(bs => (int)(stylistServices.FirstOrDefault(ss => ss.ServiceManagementID == bs.ServiceManagementID)?.ServiceDuration.TotalMinutes ?? 0));
                        var workMinutes = Math.Max(0, (workTime.WorkEndTime - workTime.WorkStartTime).TotalMinutes);
                        var dayCount = BuildDateRange(startDate.Date, endDate.Date)
                            .Count(date => MatchDayOfWeek(workTime.DayOfWeek, date.DayOfWeek));
                        var totalWorkMinutes = workMinutes * dayCount;

                        return new WorkTimeCapacityDto
                        {
                            DayOfWeek = workTime.DayOfWeek,
                            WorkHours = Math.Round(totalWorkMinutes / 60, 1),
                            BookingCount = dayBookings.Count,
                            BookedMinutes = bookedMinutes,
                            CapacityUsagePercent = totalWorkMinutes == 0 ? 0 : Math.Round(bookedMinutes * 100 / totalWorkMinutes, 1)
                        };
                    })
                    .ToList();

                report.DiscountBreakdown = new List<NameValueDto>
                {
                    new() { Name = "مبلغ پرداخت‌شده", Value = payments.Sum(x => x.PayedAmount), Count = payments.Count },
                    new() { Name = "مانده پرداخت", Value = payments.Sum(x => x.RemainAmount), Count = payments.Count },
                    new() { Name = "تخفیف", Value = payments.Sum(x => x.TotalServiceAmount - x.DiscountedServiceAmount), Count = payments.Count },
                    new() { Name = "سهم پلتفرم", Value = payments.Sum(x => x.PlarformAmount), Count = payments.Count }
                };

                report.Summary = new StylistDashboardSummary
                {
                    TodayBookingsCount = bookings.Count(x => x.BookingDate.Date == today),
                    TotalBookingsCount = bookings.Count,
                    CompletedBookingsCount = bookings.Count(x => !x.IsCancelled && x.Status == "4"),
                    CancelledBookingsCount = bookings.Count(x => x.IsCancelled),
                    TotalCustomersCount = bookings.Select(x => x.CustomerID).Distinct().Count(),
                    NewCustomersCount = bookings.GroupBy(x => x.CustomerID).Count(x => x.Min(b => b.BookingDate) >= startDate),
                    TotalRevenue = payments.Sum(x => x.TotalServiceAmount),
                    PaidAmount = payments.Sum(x => x.PayedAmount),
                    RemainAmount = payments.Sum(x => x.RemainAmount),
                    StylistAmount = payments.Sum(x => x.StylistAmount),
                    PlatformAmount = payments.Sum(x => x.PlarformAmount),
                    DiscountAmount = payments.Sum(x => x.TotalServiceAmount - x.DiscountedServiceAmount),
                    AverageRating = rateHistories.Any() ? rateHistories.Average(x => x.RateScore) : 0,
                    RecommendPercent = GetRecommendPercent(rateHistories),
                    ReviewCount = reviews.Count,
                    CancellationPercent = bookings.Count == 0 ? 0 : Math.Round(bookings.Count(x => x.IsCancelled) * 100.0 / bookings.Count, 1),
                    CapacityUsagePercent = report.WorkTimeCapacity.Any() ? Math.Round(report.WorkTimeCapacity.Average(x => x.CapacityUsagePercent), 1) : 0
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

        public async Task<RowResultObject<SalonDashboardReport>> GetSalonDashboardReportAsync(long salonStylistId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var result = new RowResultObject<SalonDashboardReport>();

            try
            {
                var startDate = (fromDate ?? DateTime.Now.AddDays(-30)).Date.ToShamsi();
                var endDate = (toDate ?? DateTime.Now).Date.AddDays(1).AddTicks(-1).ToShamsi();
                var today = DateTime.Now.ToShamsi().Date;

                var stylists = await _context.Stylists
                    .AsNoTracking()
                    .Where(x => x.ID == salonStylistId || x.StylistParentID == salonStylistId)
                    .ToListAsync();

                var stylistIds = stylists.Select(x => x.ID).ToList();
                if (!stylistIds.Any())
                {
                    result.Status = false;
                    result.ErrorMessage = "سالن یا آرایشگرهای زیرمجموعه یافت نشد";
                    return result;
                }

                var bookings = await _context.Bookings
                    .AsNoTracking()
                    .Include(x => x.Customer).ThenInclude(x => x.Person)
                    .Include(x => x.PaymentBookings).ThenInclude(x => x.Payment)
                    .Include(x => x.BookingServices).ThenInclude(x => x.ServiceManagement)
                    .Where(x => stylistIds.Contains(x.StylistID) && x.BookingDate >= startDate && x.BookingDate <= endDate)
                    .ToListAsync();

                var stylistServices = await _context.StylistServices
                    .AsNoTracking()
                    .Include(x => x.ServiceManagement)
                    .Where(x => stylistIds.Contains(x.StylistID))
                    .ToListAsync();

                var reviews = await _context.Reviews
                    .AsNoTracking()
                    .Where(x => stylistIds.Contains(x.StylistID) && x.ReviewDate >= startDate && x.ReviewDate <= endDate)
                    .ToListAsync();

                var rateHistories = await _context.RateHistories
                    .AsNoTracking()
                    .Include(x => x.RateQuestion)
                    .Where(x => stylistIds.Contains(x.StylistID) && x.RateDate >= startDate && x.RateDate <= endDate)
                    .ToListAsync();

                var workTimes = await _context.WorkTimes
                    .AsNoTracking()
                    .Where(x => stylistIds.Contains(x.StylistID))
                    .ToListAsync();

                var payments = bookings.SelectMany(GetPayments).ToList();
                var report = new SalonDashboardReport();

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
                        Amount = payments.Where(x => x.PaymentDate.Date == date).Sum(x => x.StylistAmount)
                    })
                    .ToList();

                report.BookingStatusBreakdown = bookings
                    .GroupBy(x => GetBookingStatusLabel(x.Status, x.IsCancelled))
                    .Select(x => new NameValueDto { Name = x.Key, Count = x.Count(), Value = x.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                report.ServicePerformance = bookings
                    .SelectMany(b => b.BookingServices.Select(bs => new { Booking = b, BookingService = bs }))
                    .GroupBy(x => x.BookingService.ServiceManagementID)
                    .Select(g =>
                    {
                        var service = stylistServices.FirstOrDefault(x => x.ServiceManagementID == g.Key);
                        var revenue = g.Sum(x => GetAllocatedServiceRevenue(x.Booking, x.BookingService, stylistServices));
                        var discountAmount = g.Sum(x => GetAllocatedServiceDiscount(x.Booking, x.BookingService, stylistServices));

                        return new ServicePerformanceDto
                        {
                            ServiceId = g.Key,
                            ServiceName = service?.ServiceManagement?.ServiceName ?? g.First().BookingService.ServiceManagement?.ServiceName ?? "نامشخص",
                            BookingCount = g.Count(),
                            Revenue = revenue,
                            AveragePrice = g.Count() == 0 ? 0 : revenue / g.Count(),
                            DiscountAmount = discountAmount
                        };
                    })
                    .OrderByDescending(x => x.BookingCount)
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
                    .OrderByDescending(x => x.BookingCount)
                    .Take(10)
                    .ToList();

                report.StylistPerformance = stylists
                    .Where(x => x.ID != salonStylistId)
                    .Select(stylist =>
                    {
                        var stylistBookings = bookings.Where(x => x.StylistID == stylist.ID).ToList();
                        var stylistPayments = stylistBookings.SelectMany(GetPayments).ToList();
                        var stylistRates = rateHistories.Where(x => x.StylistID == stylist.ID).ToList();

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
                            AverageRating = stylistRates.Any() ? stylistRates.Average(x => x.RateScore) : 0,
                            CancellationPercent = stylistBookings.Count == 0 ? 0 : Math.Round(stylistBookings.Count(x => x.IsCancelled) * 100.0 / stylistBookings.Count, 1)
                        };
                    })
                    .OrderByDescending(x => x.Revenue)
                    .ThenByDescending(x => x.BookingCount)
                    .ToList();

                report.RatingDistribution = Enumerable.Range(1, 5)
                    .Select(score => new NameValueDto
                    {
                        Name = $"{score} ستاره",
                        Count = reviews.Count(x => x.Rating == score),
                        Value = reviews.Count(x => x.Rating == score)
                    })
                    .ToList();

                report.RatingQuestions = rateHistories
                    .GroupBy(x => x.RateQuestionID)
                    .Select(g => new RatingQuestionDto
                    {
                        RateQuestionId = g.Key,
                        QuestionText = g.First().RateQuestion?.RateQuestionText ?? "",
                        AverageScore = g.Any() ? g.Average(x => x.RateScore) : 0,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.AverageScore)
                    .ToList();

                report.CancellationTrend = BuildDateRange(startDate.Date, endDate.Date)
                    .Select(date => new ChartPointDto
                    {
                        Date = date,
                        Label = date.ToString("yyyy/MM/dd"),
                        Count = bookings.Count(x => x.BookingDate.Date == date && x.IsCancelled)
                    })
                    .ToList();

                report.CancellationReasons = bookings
                    .Where(x => x.IsCancelled)
                    .GroupBy(x => string.IsNullOrWhiteSpace(x.CancelReason) ? "بدون دلیل" : x.CancelReason)
                    .Select(x => new NameValueDto { Name = x.Key, Count = x.Count(), Value = x.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(8)
                    .ToList();

                report.WorkTimeCapacity = workTimes
                    .Select(workTime =>
                    {
                        var dayBookings = bookings.Where(x => x.StylistID == workTime.StylistID && MatchDayOfWeek(workTime.DayOfWeek, x.BookingDate.DayOfWeek)).ToList();
                        var bookedMinutes = dayBookings
                            .SelectMany(x => x.BookingServices)
                            .Sum(bs => (int)(stylistServices.FirstOrDefault(ss => ss.StylistID == workTime.StylistID && ss.ServiceManagementID == bs.ServiceManagementID)?.ServiceDuration.TotalMinutes ?? 0));
                        var workMinutes = Math.Max(0, (workTime.WorkEndTime - workTime.WorkStartTime).TotalMinutes);
                        var dayCount = BuildDateRange(startDate.Date, endDate.Date)
                            .Count(date => MatchDayOfWeek(workTime.DayOfWeek, date.DayOfWeek));
                        var totalWorkMinutes = workMinutes * dayCount;

                        return new WorkTimeCapacityDto
                        {
                            DayOfWeek = $"{stylists.FirstOrDefault(x => x.ID == workTime.StylistID)?.StylistName ?? "آرایشگر"} - {workTime.DayOfWeek}",
                            WorkHours = Math.Round(totalWorkMinutes / 60, 1),
                            BookingCount = dayBookings.Count,
                            BookedMinutes = bookedMinutes,
                            CapacityUsagePercent = totalWorkMinutes == 0 ? 0 : Math.Round(bookedMinutes * 100 / totalWorkMinutes, 1)
                        };
                    })
                    .ToList();

                report.DiscountBreakdown = new List<NameValueDto>
                {
                    new() { Name = "مبلغ پرداخت‌شده", Value = payments.Sum(x => x.PayedAmount), Count = payments.Count },
                    new() { Name = "مانده پرداخت", Value = payments.Sum(x => x.RemainAmount), Count = payments.Count },
                    new() { Name = "تخفیف", Value = payments.Sum(x => x.TotalServiceAmount - x.DiscountedServiceAmount), Count = payments.Count },
                    new() { Name = "سهم پلتفرم", Value = payments.Sum(x => x.PlarformAmount), Count = payments.Count }
                };

                report.Summary = new StylistDashboardSummary
                {
                    TodayBookingsCount = bookings.Count(x => x.BookingDate.Date == today),
                    TotalBookingsCount = bookings.Count,
                    CompletedBookingsCount = bookings.Count(x => !x.IsCancelled && x.Status == "4"),
                    CancelledBookingsCount = bookings.Count(x => x.IsCancelled),
                    TotalCustomersCount = bookings.Select(x => x.CustomerID).Distinct().Count(),
                    NewCustomersCount = bookings.GroupBy(x => x.CustomerID).Count(x => x.Min(b => b.BookingDate) >= startDate),
                    TotalRevenue = payments.Sum(x => x.TotalServiceAmount),
                    PaidAmount = payments.Sum(x => x.PayedAmount),
                    RemainAmount = payments.Sum(x => x.RemainAmount),
                    StylistAmount = payments.Sum(x => x.StylistAmount),
                    PlatformAmount = payments.Sum(x => x.PlarformAmount),
                    DiscountAmount = payments.Sum(x => x.TotalServiceAmount - x.DiscountedServiceAmount),
                    AverageRating = rateHistories.Any() ? rateHistories.Average(x => x.RateScore) : 0,
                    RecommendPercent = GetRecommendPercent(rateHistories),
                    ReviewCount = reviews.Count,
                    CancellationPercent = bookings.Count == 0 ? 0 : Math.Round(bookings.Count(x => x.IsCancelled) * 100.0 / bookings.Count, 1),
                    CapacityUsagePercent = report.WorkTimeCapacity.Any() ? Math.Round(report.WorkTimeCapacity.Average(x => x.CapacityUsagePercent), 1) : 0
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

        private static IEnumerable<DateTime> BuildDateRange(DateTime startDate, DateTime endDate)
        {
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                yield return date;
        }

        private static string GetPersonName(Person? person)
        {
            if (person == null) return "";
            return $"{person.FirstName} {person.LastName}".Trim();
        }

        private static double GetRecommendPercent(List<RateHistory> rateHistories)
        {
            var recommendRates = rateHistories.Where(x => x.RateQuestionID == 5).ToList();
            return recommendRates.Count == 0 ? 0 : Math.Round(recommendRates.Count(x => x.RateScore == 5) * 100.0 / recommendRates.Count, 1);
        }

        private static decimal GetAllocatedServiceRevenue(Booking booking, BookingService bookingService, List<StylistService> stylistServices)
        {
            var serviceAmount = GetPayments(booking).Sum(x => x.StylistAmount);
            return AllocateBookingAmountToService(booking, bookingService, stylistServices, serviceAmount);
        }

        private static decimal GetAllocatedServiceDiscount(Booking booking, BookingService bookingService, List<StylistService> stylistServices)
        {
            var discountAmount = GetPayments(booking).Sum(x => x.TotalServiceAmount - x.DiscountedServiceAmount);
            return AllocateBookingAmountToService(booking, bookingService, stylistServices, discountAmount);
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

        private static bool MatchDayOfWeek(string dayName, DayOfWeek dayOfWeek)
        {
            var normalized = (dayName ?? "").Trim();
            return dayOfWeek switch
            {
                DayOfWeek.Saturday => normalized.Contains("شنبه") && !normalized.Contains("یک") && !normalized.Contains("دو") && !normalized.Contains("سه") && !normalized.Contains("چهار") && !normalized.Contains("پنج"),
                DayOfWeek.Sunday => normalized.Contains("یکشنبه") || normalized.Contains("يكشنبه"),
                DayOfWeek.Monday => normalized.Contains("دوشنبه"),
                DayOfWeek.Tuesday => normalized.Contains("سه") || normalized.Contains("سه‌شنبه"),
                DayOfWeek.Wednesday => normalized.Contains("چهار"),
                DayOfWeek.Thursday => normalized.Contains("پنج"),
                DayOfWeek.Friday => normalized.Contains("جمعه"),
                _ => false
            };
        }
    }
}
