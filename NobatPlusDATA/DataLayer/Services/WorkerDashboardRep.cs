using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class WorkerDashboardRep : IWorkerDashboardRep
    {
        private readonly NobatPlusContext _context;

        public WorkerDashboardRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<RowResultObject<WorkerDashboardReport>> GetWorkerDashboardReportAsync(long workerId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var result = new RowResultObject<WorkerDashboardReport>();

            try
            {
                var startDate = (fromDate ?? DateTime.Now.AddDays(-30)).Date;
                var endDate = (toDate ?? DateTime.Now).Date.AddDays(1).AddTicks(-1);
                var shamsiStartDate = startDate.ToShamsi();
                var shamsiEndDate = endDate.ToShamsi();
                var now = DateTime.Now;
                var today = now.Date;
                var todayEnd = today.AddDays(1).AddTicks(-1);

                var bookings = await _context.Bookings
                    .AsNoTracking()
                    .Include(x => x.Customer).ThenInclude(x => x.Person)
                    .Include(x => x.PaymentBookings).ThenInclude(x => x.Payment)
                    .Include(x => x.BookingServices).ThenInclude(x => x.ServiceManagement)
                    .Where(x => x.WorkerID == workerId && x.BookingDate >= startDate && x.BookingDate <= endDate)
                    .ToListAsync();

                var workerServices = await _context.WorkerServices
                    .AsNoTracking()
                    .Include(x => x.ServiceManagement)
                    .Where(x => x.WorkerID == workerId)
                    .ToListAsync();

                var reviews = await _context.Reviews
                    .AsNoTracking()
                    .Where(x => x.WorkerID == workerId && x.ReviewDate >= shamsiStartDate && x.ReviewDate <= shamsiEndDate)
                    .ToListAsync();

                var rateHistories = await _context.RateHistories
                    .AsNoTracking()
                    .Include(x => x.RateQuestion)
                    .Where(x => x.WorkerID == workerId && x.RateDate >= shamsiStartDate && x.RateDate <= shamsiEndDate)
                    .ToListAsync();

                var workTimes = await _context.WorkTimes
                    .AsNoTracking()
                    .Where(x => x.WorkerID == workerId)
                    .ToListAsync();

                var todayBookings = await _context.Bookings
                    .AsNoTracking()
                    .Include(x => x.Customer).ThenInclude(x => x.Person)
                    .Include(x => x.PaymentBookings).ThenInclude(x => x.Payment)
                    .Include(x => x.BookingServices).ThenInclude(x => x.ServiceManagement)
                    .Where(x => x.WorkerID == workerId && x.BookingDate >= today && x.BookingDate <= todayEnd)
                    .OrderBy(x => x.BookingDate)
                    .ToListAsync();

                var nextBooking = await _context.Bookings
                    .AsNoTracking()
                    .Include(x => x.Customer).ThenInclude(x => x.Person)
                    .Include(x => x.PaymentBookings).ThenInclude(x => x.Payment)
                    .Include(x => x.BookingServices).ThenInclude(x => x.ServiceManagement)
                    .Where(x => x.WorkerID == workerId && !x.IsCancelled && x.BookingDate >= now)
                    .OrderBy(x => x.BookingDate)
                    .FirstOrDefaultAsync();

                var newBookings = await _context.Bookings
                    .AsNoTracking()
                    .Include(x => x.Customer).ThenInclude(x => x.Person)
                    .Include(x => x.PaymentBookings).ThenInclude(x => x.Payment)
                    .Include(x => x.BookingServices).ThenInclude(x => x.ServiceManagement)
                    .Where(x => x.WorkerID == workerId && !x.IsCancelled && x.Status == "1")
                    .OrderByDescending(x => x.CreateDate)
                    .ThenBy(x => x.BookingDate)
                    .ToListAsync();

                var payments = bookings.SelectMany(GetPayments).ToList();
                var revenueDetails = await _context.PaymentDetails
                    .AsNoTracking()
                    .Include(x => x.Payment)
                    .Where(x =>
                        x.WorkerID == workerId &&
                        x.Payment.PaymentDate >= startDate &&
                        x.Payment.PaymentDate <= endDate &&
                        (x.Payment.PaymentFinished || x.Payment.PayedAmount > 0))
                    .Select(x => new
                    {
                        x.Payment.PaymentDate,
                        Amount = x.WorkerServiceAmount - x.DiscountAmount
                    })
                    .ToListAsync();

                var completedBookingServices = bookings
                    .Where(x => !x.IsCancelled && x.Status == "4")
                    .SelectMany(b => b.BookingServices.Select(bs => new
                    {
                        BookingID = b.ID,
                        bs.ServiceManagementID,
                        bs.ServiceManagement
                    }))
                    .ToList();

                var completedBookingIds = completedBookingServices
                    .Select(x => x.BookingID)
                    .Distinct()
                    .ToList();

                var completedPaymentDetails = await _context.PaymentDetails
                    .AsNoTracking()
                    .Include(x => x.Payment)
                    .Include(x => x.ServiceManagement)
                    .Where(x =>
                        x.WorkerID == workerId &&
                        completedBookingIds.Contains(x.BookingID) &&
                        (x.Payment.PaymentFinished || x.Payment.PayedAmount > 0))
                    .Select(x => new
                    {
                        x.BookingID,
                        x.ServiceManagementID,
                        ServiceName = x.ServiceManagement.ServiceName,
                        Revenue = x.WorkerServiceAmount - x.DiscountAmount,
                        x.DiscountAmount
                    })
                    .ToListAsync();

                var report = new WorkerDashboardReport();

                report.TodayBookings = todayBookings
                    .Select(BuildDashboardBookingDto)
                    .ToList();

                report.NewBookings = newBookings
                    .Select(BuildDashboardBookingDto)
                    .ToList();

                report.NextBooking = nextBooking == null
                    ? null
                    : BuildDashboardBookingDto(nextBooking);

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
                        Amount = revenueDetails
                            .Where(x => x.PaymentDate.Date == date)
                            .Sum(x => x.Amount)
                    })
                    .ToList();

                report.BookingStatusBreakdown = bookings
                    .GroupBy(x => GetBookingStatusLabel(x.Status, x.IsCancelled))
                    .Select(x => new NameValueDto { Name = x.Key, Count = x.Count(), Value = x.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                report.ServicePerformance = completedBookingServices
                    .GroupBy(x => x.ServiceManagementID)
                    .Select(g =>
                    {
                        var service = workerServices.FirstOrDefault(x => x.ServiceManagementID == g.Key);
                        var detailRows = completedPaymentDetails.Where(x => x.ServiceManagementID == g.Key).ToList();
                        var revenue = detailRows.Sum(x => x.Revenue);
                        var discountAmount = detailRows.Sum(x => x.DiscountAmount);

                        return new ServicePerformanceDto
                        {
                            ServiceId = g.Key,
                            ServiceName = service?.ServiceManagement?.ServiceName ?? g.First().ServiceManagement?.ServiceName ?? detailRows.FirstOrDefault()?.ServiceName ?? "نامشخص",
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
                            .Sum(bs => (int)(workerServices.FirstOrDefault(ss => ss.ServiceManagementID == bs.ServiceManagementID)?.ServiceDuration.TotalMinutes ?? 0));
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

                report.Summary = new WorkerDashboardSummary
                {
                    TodayBookingsCount = bookings.Count(x => x.BookingDate.Date == today),
                    NewBookingsCount = newBookings.Count,
                    TotalBookingsCount = bookings.Count,
                    CompletedBookingsCount = bookings.Count(x => !x.IsCancelled && x.Status == "4"),
                    CancelledBookingsCount = bookings.Count(x => x.IsCancelled),
                    TotalCustomersCount = bookings.Select(x => x.CustomerID).Distinct().Count(),
                    NewCustomersCount = bookings.GroupBy(x => x.CustomerID).Count(x => x.Min(b => b.BookingDate) >= startDate),
                    TotalRevenue = payments.Sum(x => x.TotalServiceAmount),
                    PaidAmount = payments.Sum(x => x.PayedAmount),
                    RemainAmount = payments.Sum(x => x.RemainAmount),
                    WorkerAmount = payments.Sum(x => x.WorkerAmount),
                    PlatformAmount = payments.Sum(x => x.PlarformAmount),
                    DiscountAmount = payments.Sum(x => x.TotalServiceAmount - x.DiscountedServiceAmount),
                    AverageRating = rateHistories.Any() ? (float)reviews.Average(x => x.Rating) : 0,
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

        public async Task<RowResultObject<WorkshopDashboardReport>> GetWorkshopDashboardReportAsync(long workshopWorkerId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var result = new RowResultObject<WorkshopDashboardReport>();

            try
            {
                var startDate = (fromDate ?? DateTime.Now.AddDays(-30)).Date.ToShamsi();
                var endDate = (toDate ?? DateTime.Now).Date.AddDays(1).AddTicks(-1).ToShamsi();
                var today = DateTime.Now.ToShamsi().Date;

                var workers = await _context.Workers
                    .AsNoTracking()
                    .Where(x => x.ID == workshopWorkerId || x.WorkerParentID == workshopWorkerId)
                    .ToListAsync();

                var workerIds = workers.Select(x => x.ID).ToList();
                if (!workerIds.Any())
                {
                    result.Status = false;
                    result.ErrorMessage = "محل خدمت یا خدمات دهندههای زیرمجموعه یافت نشد";
                    return result;
                }

                var bookings = await _context.Bookings
                    .AsNoTracking()
                    .Include(x => x.Customer).ThenInclude(x => x.Person)
                    .Include(x => x.PaymentBookings).ThenInclude(x => x.Payment)
                    .Include(x => x.BookingServices).ThenInclude(x => x.ServiceManagement)
                    .Where(x => workerIds.Contains(x.WorkerID) && x.BookingDate >= startDate && x.BookingDate <= endDate)
                    .ToListAsync();

                var workerServices = await _context.WorkerServices
                    .AsNoTracking()
                    .Include(x => x.ServiceManagement)
                    .Where(x => workerIds.Contains(x.WorkerID))
                    .ToListAsync();

                var reviews = await _context.Reviews
                    .AsNoTracking()
                    .Where(x => workerIds.Contains(x.WorkerID) && x.ReviewDate >= startDate && x.ReviewDate <= endDate)
                    .ToListAsync();

                var rateHistories = await _context.RateHistories
                    .AsNoTracking()
                    .Include(x => x.RateQuestion)
                    .Where(x => workerIds.Contains(x.WorkerID) && x.RateDate >= startDate && x.RateDate <= endDate)
                    .ToListAsync();

                var workTimes = await _context.WorkTimes
                    .AsNoTracking()
                    .Where(x => workerIds.Contains(x.WorkerID))
                    .ToListAsync();

                var payments = bookings.SelectMany(GetPayments).ToList();
                var bookingIds = bookings.Select(x => x.ID).Distinct().ToList();
                var paymentDetails = await _context.PaymentDetails
                    .AsNoTracking()
                    .Include(x => x.Payment)
                    .Where(x =>
                        workerIds.Contains(x.WorkerID) &&
                        bookingIds.Contains(x.BookingID) &&
                        (x.Payment.PaymentFinished || x.Payment.PayedAmount > 0))
                    .Select(x => new
                    {
                        x.PaymentID,
                        x.BookingID,
                        x.WorkerID,
                        NetAmount = x.WorkerServiceAmount - x.DiscountAmount,
                        x.Payment.PayedAmount,
                        x.Payment.RemainAmount
                    })
                    .ToListAsync();

                var paymentIds = paymentDetails.Select(x => x.PaymentID).Distinct().ToList();
                var paymentNetAmounts = await _context.PaymentDetails
                    .AsNoTracking()
                    .Where(x => paymentIds.Contains(x.PaymentID))
                    .GroupBy(x => x.PaymentID)
                    .Select(g => new
                    {
                        PaymentID = g.Key,
                        NetAmount = g.Sum(x => x.WorkerServiceAmount - x.DiscountAmount)
                    })
                    .ToDictionaryAsync(x => x.PaymentID, x => x.NetAmount);

                var report = new WorkshopDashboardReport();

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
                        Amount = payments.Where(x => x.PaymentDate.Date == date).Sum(x => x.WorkerAmount)
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
                        var service = workerServices.FirstOrDefault(x => x.ServiceManagementID == g.Key);
                        var revenue = g.Sum(x => GetAllocatedServiceRevenue(x.Booking, x.BookingService, workerServices));
                        var discountAmount = g.Sum(x => GetAllocatedServiceDiscount(x.Booking, x.BookingService, workerServices));

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

                report.WorkerPerformance = workers
                    .Where(x => x.ID != workshopWorkerId)
                    .Select(worker =>
                    {
                        var workerBookings = bookings.Where(x => x.WorkerID == worker.ID).ToList();
                        var workerPaymentDetails = paymentDetails.Where(x => x.WorkerID == worker.ID).ToList();
                        var workerReviews = reviews.Where(x => x.WorkerID == worker.ID).ToList();

                        return new WorkerPerformanceDto
                        {
                            WorkerId = worker.ID,
                            WorkerName = worker.WorkerName,
                            BookingCount = workerBookings.Count,
                            CompletedBookingCount = workerBookings.Count(x => !x.IsCancelled && x.Status == "4"),
                            CancelledBookingCount = workerBookings.Count(x => x.IsCancelled),
                            CustomerCount = workerBookings.Select(x => x.CustomerID).Distinct().Count(),
                            Revenue = workerPaymentDetails.Sum(x => x.NetAmount),
                            PaidAmount = workerPaymentDetails.Sum(x => GetPaymentDetailAllocatedAmount(x.NetAmount, x.PayedAmount, paymentNetAmounts.GetValueOrDefault(x.PaymentID))),
                            RemainAmount = workerPaymentDetails.Sum(x => GetPaymentDetailAllocatedAmount(x.NetAmount, x.RemainAmount, paymentNetAmounts.GetValueOrDefault(x.PaymentID))),
                            AverageRating = workerReviews.Any() ? (float)workerReviews.Average(x => x.Rating) : 0,
                            CancellationPercent = workerBookings.Count == 0 ? 0 : Math.Round(workerBookings.Count(x => x.IsCancelled) * 100.0 / workerBookings.Count, 1)
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
                        var dayBookings = bookings.Where(x => x.WorkerID == workTime.WorkerID && MatchDayOfWeek(workTime.DayOfWeek, x.BookingDate.DayOfWeek)).ToList();
                        var bookedMinutes = dayBookings
                            .SelectMany(x => x.BookingServices)
                            .Sum(bs => (int)(workerServices.FirstOrDefault(ss => ss.WorkerID == workTime.WorkerID && ss.ServiceManagementID == bs.ServiceManagementID)?.ServiceDuration.TotalMinutes ?? 0));
                        var workMinutes = Math.Max(0, (workTime.WorkEndTime - workTime.WorkStartTime).TotalMinutes);
                        var dayCount = BuildDateRange(startDate.Date, endDate.Date)
                            .Count(date => MatchDayOfWeek(workTime.DayOfWeek, date.DayOfWeek));
                        var totalWorkMinutes = workMinutes * dayCount;

                        return new WorkTimeCapacityDto
                        {
                            DayOfWeek = $"{workers.FirstOrDefault(x => x.ID == workTime.WorkerID)?.WorkerName ?? "خدمات دهنده"} - {workTime.DayOfWeek}",
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

                report.Summary = new WorkerDashboardSummary
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
                    WorkerAmount = payments.Sum(x => x.WorkerAmount),
                    PlatformAmount = payments.Sum(x => x.PlarformAmount),
                    DiscountAmount = payments.Sum(x => x.TotalServiceAmount - x.DiscountedServiceAmount),
                    AverageRating = reviews.Any() ? (float)reviews.Average(x => x.Rating) : 0,
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

        private static decimal GetAllocatedServiceRevenue(Booking booking, BookingService bookingService, List<WorkerService> workerServices)
        {
            var serviceAmount = GetPayments(booking).Sum(x => x.WorkerAmount);
            return AllocateBookingAmountToService(booking, bookingService, workerServices, serviceAmount);
        }

        private static decimal GetAllocatedServiceDiscount(Booking booking, BookingService bookingService, List<WorkerService> workerServices)
        {
            var discountAmount = GetPayments(booking).Sum(x => x.TotalServiceAmount - x.DiscountedServiceAmount);
            return AllocateBookingAmountToService(booking, bookingService, workerServices, discountAmount);
        }

        private static decimal GetPaymentDetailAllocatedAmount(decimal detailNetAmount, decimal paymentAmount, decimal paymentNetAmount)
        {
            if (detailNetAmount <= 0 || paymentAmount <= 0)
                return 0;

            if (paymentNetAmount <= 0)
                return paymentAmount;

            return paymentAmount * detailNetAmount / paymentNetAmount;
        }

        private static WorkerDashboardBookingDto BuildDashboardBookingDto(Booking booking)
        {
            var payments = GetPayments(booking).DistinctBy(x => x.ID).ToList();
            var services = booking.BookingServices?
                .Where(x => x.ServiceManagement != null && !string.IsNullOrWhiteSpace(x.ServiceManagement.ServiceName))
                .OrderBy(x => x.ServiceManagement.ServiceName)
                .ToList() ?? new List<BookingService>();

            return new WorkerDashboardBookingDto
            {
                BookingId = booking.ID,
                CustomerId = booking.CustomerID,
                CustomerName = GetPersonName(booking.Customer?.Person),
                CustomerPhoneNumber = booking.Customer?.Person?.PhoneNumber ?? "",
                BookingDate = booking.BookingDate,
                BookingDateText = booking.BookingDate.ToShamsiString().Split(' ')[0],
                BookingTime = booking.BookingDate.ToString("HH:mm"),
                Services = string.Join("، ", services.Select(x => x.ServiceManagement.ServiceName).Distinct()),
                ServiceIds = services.Select(x => x.ServiceManagementID).Distinct().ToList(),
                Status = GetBookingStatusLabel(booking.Status, booking.IsCancelled),
                IsCancelled = booking.IsCancelled,
                TotalAmount = payments.Sum(x => x.TotalServiceAmount),
                PaidAmount = payments.Sum(x => x.PayedAmount),
                RemainAmount = payments.Sum(x => x.RemainAmount),
                WorkerAmount = payments.Sum(x => x.WorkerAmount)
            };
        }

        private static decimal AllocateBookingAmountToService(Booking booking, BookingService bookingService, List<WorkerService> workerServices, decimal amount)
        {
            if (amount == 0 || booking.BookingServices == null || !booking.BookingServices.Any())
                return 0;

            var totalServicePrice = booking.BookingServices
                .Sum(x => GetBookingServicePrice(booking.WorkerID, x, workerServices));

            if (totalServicePrice <= 0)
                return amount / booking.BookingServices.Count;

            var servicePrice = GetBookingServicePrice(booking.WorkerID, bookingService, workerServices);
            return amount * servicePrice / totalServicePrice;
        }

        private static IEnumerable<Payment> GetPayments(Booking booking)
        {
            return booking.PaymentBookings?.Select(x => x.Payment).Where(x => x != null) ?? Enumerable.Empty<Payment>();
        }

        private static decimal GetBookingServicePrice(long workerId, BookingService bookingService, List<WorkerService> workerServices)
        {
            return workerServices
                .FirstOrDefault(x => x.WorkerID == workerId && x.ServiceManagementID == bookingService.ServiceManagementID)
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
                "5" => "مرخصی",
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
