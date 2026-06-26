using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;

namespace NobatPlusDATA.DataLayer.Services
{
    public class AdminMonitoringRep : IAdminMonitoringRep
    {
        private readonly NobatPlusContext _context;

        public AdminMonitoringRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<RowResultObject<AdminMonitoringReportVM>> GetMonitoringReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var result = new RowResultObject<AdminMonitoringReportVM>();
            try
            {
                var startDate = (fromDate ?? DateTime.Now.AddDays(-7)).Date.ToShamsi();
                var endDate = (toDate ?? DateTime.Now).Date.AddDays(1).AddTicks(-1).ToShamsi();

                var failedAuditQuery = _context.AdminAuditLogs
                    .AsNoTracking()
                    .Where(x => x.OccurredAt >= startDate && x.OccurredAt <= endDate && !x.Succeeded);

                var failedPaymentsQuery = _context.PaymentHistories
                    .AsNoTracking()
                    .Where(x => x.PaymentDate >= startDate && x.PaymentDate <= endDate && !x.PaymentStatus);

                var failedSmsQuery = _context.SMSMessages
                    .AsNoTracking()
                    .Include(x => x.Person)
                    .Where(x => x.SentDate >= startDate && x.SentDate <= endDate && !x.SentStatus);

                var openTicketsQuery = _context.SupportTickets
                    .AsNoTracking()
                    .Include(x => x.Person)
                    .Where(x => x.Status != "closed");

                var pendingDocumentsQuery = _context.FileUploads
                    .AsNoTracking()
                    .Where(x => x.ReviewStatus == "pending");

                var pendingSettlementsQuery = _context.SettlementRequests
                    .AsNoTracking()
                    .Where(x => x.Status == "pending");

                var rescheduleBookingsQuery = _context.Bookings
                    .AsNoTracking()
                    .Where(x => !x.IsCancelled && x.Status == "5");

                var unfinishedPaymentsQuery = _context.Payments
                    .AsNoTracking()
                    .Where(x => x.PaymentDate >= startDate && x.PaymentDate <= endDate && !x.PaymentFinished);

                var report = new AdminMonitoringReportVM();

                report.Summary.FailedAdminOperations = await failedAuditQuery.CountAsync();
                report.Summary.FailedPayments = await failedPaymentsQuery.CountAsync();
                report.Summary.FailedPaymentAmount = await failedPaymentsQuery.SumAsync(x => x.Amount);
                report.Summary.FailedSmsMessages = await failedSmsQuery.CountAsync();
                report.Summary.OpenSupportTickets = await openTicketsQuery.CountAsync();
                report.Summary.UrgentSupportTickets = await openTicketsQuery.CountAsync(x => x.Priority == "urgent" || x.Priority == "high");
                report.Summary.PendingDocuments = await pendingDocumentsQuery.CountAsync();
                report.Summary.PendingSettlements = await pendingSettlementsQuery.CountAsync();
                report.Summary.PendingSettlementAmount = await pendingSettlementsQuery.SumAsync(x => x.Amount);
                report.Summary.RescheduleRequiredBookings = await rescheduleBookingsQuery.CountAsync();
                report.Summary.UnfinishedPayments = await unfinishedPaymentsQuery.CountAsync();

                report.Indicators = new List<AdminMonitoringIndicatorVM>
                {
                    Indicator("خطاهای عملیات ادمین", "عملیات ناموفق ثبت شده در audit trail", report.Summary.FailedAdminOperations, 0, report.Summary.FailedAdminOperations > 10 ? "danger" : report.Summary.FailedAdminOperations > 0 ? "warning" : "ok"),
                    Indicator("پرداخت های ناموفق", "تلاش های ناموفق درگاه پرداخت در بازه انتخابی", report.Summary.FailedPayments, report.Summary.FailedPaymentAmount, report.Summary.FailedPayments > 10 ? "danger" : report.Summary.FailedPayments > 0 ? "warning" : "ok"),
                    Indicator("پیامک های ناموفق", "پیامک هایی که ارسال موفق نداشته اند", report.Summary.FailedSmsMessages, 0, report.Summary.FailedSmsMessages > 20 ? "danger" : report.Summary.FailedSmsMessages > 0 ? "warning" : "ok"),
                    Indicator("تیکت های باز", "تیکت های پشتیبانی که هنوز بسته نشده اند", report.Summary.OpenSupportTickets, 0, report.Summary.UrgentSupportTickets > 0 ? "danger" : report.Summary.OpenSupportTickets > 0 ? "warning" : "ok"),
                    Indicator("مدارک در انتظار", "مدارک upload شده که هنوز بررسی نشده اند", report.Summary.PendingDocuments, 0, report.Summary.PendingDocuments > 20 ? "danger" : report.Summary.PendingDocuments > 0 ? "warning" : "ok"),
                    Indicator("تسویه های معلق", "درخواست های تسویه در انتظار اقدام", report.Summary.PendingSettlements, report.Summary.PendingSettlementAmount, report.Summary.PendingSettlements > 10 ? "danger" : report.Summary.PendingSettlements > 0 ? "warning" : "ok"),
                    Indicator("نوبت های نیازمند تعیین تکلیف", "رزروهایی که به دلیل تغییر برنامه نیازمند reschedule هستند", report.Summary.RescheduleRequiredBookings, 0, report.Summary.RescheduleRequiredBookings > 0 ? "warning" : "ok"),
                    Indicator("پرداخت های ناتمام", "پرداخت های بازه انتخابی که تکمیل نشده اند", report.Summary.UnfinishedPayments, 0, report.Summary.UnfinishedPayments > 20 ? "danger" : report.Summary.UnfinishedPayments > 0 ? "warning" : "ok")
                };

                report.RecentAuditFailures = await failedAuditQuery
                    .OrderByDescending(x => x.OccurredAt)
                    .Take(10)
                    .Select(x => new AdminMonitoringAuditFailureVM
                    {
                        ID = x.ID,
                        ActorFullName = x.ActorFullName,
                        ActionName = x.ActionName,
                        EntityName = x.EntityName,
                        RequestPath = x.RequestPath,
                        StatusCode = x.StatusCode,
                        ErrorMessage = x.ErrorMessage ?? "",
                        OccurredAt = x.OccurredAt
                    })
                    .ToListAsync();

                report.RecentPaymentFailures = await failedPaymentsQuery
                    .OrderByDescending(x => x.PaymentDate)
                    .Take(10)
                    .Select(x => new AdminMonitoringPaymentFailureVM
                    {
                        ID = x.ID,
                        PaymentID = x.PaymentID,
                        PaymentDate = x.PaymentDate,
                        Amount = x.Amount,
                        GatewayName = x.GatewayName ?? "",
                        GatewayMessage = x.GatewayMessage ?? "",
                        TrackingNumber = x.TrackingNumber ?? ""
                    })
                    .ToListAsync();

                report.RecentSmsFailures = await failedSmsQuery
                    .OrderByDescending(x => x.SentDate)
                    .Take(10)
                    .Select(x => new AdminMonitoringSmsFailureVM
                    {
                        ID = x.ID,
                        PhoneNumber = x.PhoneNumber,
                        PersonFullName = (x.Person.FirstName + " " + x.Person.LastName).Trim(),
                        Message = x.Message,
                        SentDate = x.SentDate
                    })
                    .ToListAsync();

                report.OpenTickets = await openTicketsQuery
                    .OrderByDescending(x => x.Priority == "urgent")
                    .ThenByDescending(x => x.Priority == "high")
                    .ThenByDescending(x => x.LastMessageAt)
                    .Take(10)
                    .Select(x => new AdminMonitoringTicketVM
                    {
                        ID = x.ID,
                        Title = x.Title,
                        PersonFullName = (x.Person.FirstName + " " + x.Person.LastName).Trim(),
                        PersonPhoneNumber = x.Person.PhoneNumber,
                        Priority = x.Priority,
                        Status = x.Status,
                        LastMessageAt = x.LastMessageAt
                    })
                    .ToListAsync();

                result.Result = report;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        private static AdminMonitoringIndicatorVM Indicator(string title, string description, int count, decimal amount, string severity)
        {
            return new AdminMonitoringIndicatorVM
            {
                Title = title,
                Description = description,
                Count = count,
                Amount = amount,
                Severity = severity
            };
        }
    }
}
