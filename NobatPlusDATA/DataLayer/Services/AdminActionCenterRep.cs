using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;

namespace NobatPlusDATA.DataLayer.Services
{
    public class AdminActionCenterRep : IAdminActionCenterRep
    {
        private readonly NobatPlusContext _context;

        public AdminActionCenterRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<RowResultObject<AdminActionCenterVM>> GetActionCenterAsync(string type = "", string severity = "", int maxItemsPerType = 20)
        {
            var result = new RowResultObject<AdminActionCenterVM>();
            try
            {
                maxItemsPerType = maxItemsPerType <= 0 ? 20 : Math.Min(maxItemsPerType, 100);
                var now = DateTime.Now.ToShamsi();
                var items = new List<AdminActionItemVM>();

                if (ShouldInclude(type, "documents"))
                    items.AddRange(await GetPendingDocumentsAsync(now, maxItemsPerType));

                if (ShouldInclude(type, "settlements"))
                    items.AddRange(await GetPendingSettlementsAsync(now, maxItemsPerType));

                if (ShouldInclude(type, "bookings"))
                    items.AddRange(await GetRescheduleBookingsAsync(now, maxItemsPerType));

                if (ShouldInclude(type, "payments"))
                    items.AddRange(await GetUnfinishedPaymentsAsync(now, maxItemsPerType));

                if (ShouldInclude(type, "support"))
                    items.AddRange(await GetSupportIssuesAsync(now, maxItemsPerType));

                if (ShouldInclude(type, "providers"))
                    items.AddRange(await GetProviderIssuesAsync(now, maxItemsPerType));

                if (ShouldInclude(type, "services"))
                    items.AddRange(await GetServiceIssuesAsync(now, maxItemsPerType));

                if (!string.IsNullOrWhiteSpace(severity))
                    items = items.Where(x => x.Severity == severity.Trim().ToLower()).ToList();

                items = items
                    .OrderBy(x => SeverityRank(x.Severity))
                    .ThenByDescending(x => x.AgeDays)
                    .ThenByDescending(x => x.CreatedAt)
                    .ToList();

                var vm = new AdminActionCenterVM { Items = items };
                vm.Summary.TotalCount = items.Count;
                vm.Summary.DangerCount = items.Count(x => x.Severity == "danger");
                vm.Summary.WarningCount = items.Count(x => x.Severity == "warning");
                vm.Summary.InfoCount = items.Count(x => x.Severity == "info");
                vm.Summary.PendingDocumentsCount = items.Count(x => x.Type == "documents");
                vm.Summary.PendingSettlementsCount = items.Count(x => x.Type == "settlements");
                vm.Summary.RescheduleBookingsCount = items.Count(x => x.Type == "bookings");
                vm.Summary.UnfinishedPaymentsCount = items.Count(x => x.Type == "payments");
                vm.Summary.ProviderDataIssuesCount = items.Count(x => x.Type == "providers" || x.Type == "services");
                vm.Summary.SupportIssuesCount = items.Count(x => x.Type == "support");

                result.Result = vm;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        private async Task<List<AdminActionItemVM>> GetPendingDocumentsAsync(DateTime now, int take)
        {
            var rows = await _context.FileUploads
                .AsNoTracking()
                .Where(x => x.ReviewStatus == "pending")
                .OrderBy(x => x.CreateDate)
                .Take(take)
                .ToListAsync();

            return rows.Select(x =>
            {
                var age = AgeDays(now, x.CreateDate);
                return new AdminActionItemVM
                {
                    Type = "documents",
                    Severity = age >= 7 ? "danger" : "warning",
                    Title = "مدرک در انتظار بررسی",
                    Description = $"{x.FileName} برای {x.EntityType} هنوز تایید یا رد نشده است.",
                    EntityID = x.ID,
                    EntityName = x.FileName,
                    CreatedAt = x.CreateDate,
                    AgeDays = age,
                    ActionPath = "/admin/documents",
                    ActionLabel = "بررسی مدرک"
                };
            }).ToList();
        }

        private async Task<List<AdminActionItemVM>> GetPendingSettlementsAsync(DateTime now, int take)
        {
            var rows = await _context.SettlementRequests
                .AsNoTracking()
                .Include(x => x.FinancialAccount).ThenInclude(x => x.Stylist).ThenInclude(x => x.Person)
                .Where(x => x.Status == "pending")
                .OrderBy(x => x.RequestDate)
                .Take(take)
                .ToListAsync();

            return rows.Select(x =>
            {
                var age = AgeDays(now, x.RequestDate);
                var stylistName = x.FinancialAccount?.Stylist?.StylistName ?? GetPersonName(x.FinancialAccount?.Stylist?.Person);
                return new AdminActionItemVM
                {
                    Type = "settlements",
                    Severity = age >= 3 || string.IsNullOrWhiteSpace(x.Iban) ? "danger" : "warning",
                    Title = "درخواست تسویه معلق",
                    Description = string.IsNullOrWhiteSpace(x.Iban)
                        ? "درخواست تسویه بدون شبا ثبت شده و نیازمند بررسی دستی است."
                        : "درخواست تسویه هنوز پرداخت یا رد نشده است.",
                    EntityID = x.ID,
                    EntityName = stylistName,
                    CreatedAt = x.RequestDate,
                    AgeDays = age,
                    Amount = x.Amount,
                    ActionPath = "/admin/payments/settlements",
                    ActionLabel = "رسیدگی به تسویه"
                };
            }).ToList();
        }

        private async Task<List<AdminActionItemVM>> GetRescheduleBookingsAsync(DateTime now, int take)
        {
            var rows = await _context.Bookings
                .AsNoTracking()
                .Include(x => x.Customer).ThenInclude(x => x.Person)
                .Include(x => x.Stylist).ThenInclude(x => x.Person)
                .Where(x => !x.IsCancelled && x.Status == "5")
                .OrderBy(x => x.BookingDate)
                .Take(take)
                .ToListAsync();

            return rows.Select(x =>
            {
                var age = AgeDays(now, x.UpdateDate ?? x.CreateDate ?? x.BookingDate);
                return new AdminActionItemVM
                {
                    Type = "bookings",
                    Severity = age >= 2 ? "danger" : "warning",
                    Title = "نوبت نیازمند تعیین تکلیف",
                    Description = $"نوبت {GetPersonName(x.Customer?.Person)} با {GetPersonName(x.Stylist?.Person)} باید جابه‌جا یا تعیین وضعیت شود.",
                    EntityID = x.ID,
                    EntityName = GetPersonName(x.Customer?.Person),
                    CreatedAt = x.BookingDate,
                    AgeDays = age,
                    ActionPath = "/admin/appointments",
                    ActionLabel = "مدیریت نوبت"
                };
            }).ToList();
        }

        private async Task<List<AdminActionItemVM>> GetUnfinishedPaymentsAsync(DateTime now, int take)
        {
            var rows = await _context.Payments
                .AsNoTracking()
                .Include(x => x.PaymentBookings).ThenInclude(x => x.Booking).ThenInclude(x => x.Customer).ThenInclude(x => x.Person)
                .Where(x => !x.PaymentFinished)
                .OrderBy(x => x.PaymentDate)
                .Take(take)
                .ToListAsync();

            return rows.Select(x =>
            {
                var age = AgeDays(now, x.PaymentDate);
                var customer = x.PaymentBookings?.Select(pb => GetPersonName(pb.Booking?.Customer?.Person)).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "";
                return new AdminActionItemVM
                {
                    Type = "payments",
                    Severity = x.RemainAmount > 0 || age >= 2 ? "danger" : "warning",
                    Title = "پرداخت ناتمام",
                    Description = $"پرداخت تکمیل نشده است. وضعیت: {x.PaymentStatus}",
                    EntityID = x.ID,
                    EntityName = customer,
                    CreatedAt = x.PaymentDate,
                    AgeDays = age,
                    Amount = x.RemainAmount > 0 ? x.RemainAmount : x.AllPaymentAmount,
                    ActionPath = "/admin/payments",
                    ActionLabel = "بررسی پرداخت"
                };
            }).ToList();
        }

        private async Task<List<AdminActionItemVM>> GetSupportIssuesAsync(DateTime now, int take)
        {
            var rows = await _context.SupportTickets
                .AsNoTracking()
                .Include(x => x.Person)
                .Where(x => x.Status != "closed")
                .OrderByDescending(x => x.Priority == "urgent")
                .ThenByDescending(x => x.Priority == "high")
                .ThenBy(x => x.LastMessageAt)
                .Take(take)
                .ToListAsync();

            return rows.Select(x =>
            {
                var age = AgeDays(now, x.LastMessageAt);
                return new AdminActionItemVM
                {
                    Type = "support",
                    Severity = x.Priority == "urgent" || x.Priority == "high" || age >= 2 ? "danger" : "warning",
                    Title = "تیکت پشتیبانی باز",
                    Description = $"{x.Title} - وضعیت: {x.Status}",
                    EntityID = x.ID,
                    EntityName = GetPersonName(x.Person),
                    CreatedAt = x.LastMessageAt,
                    AgeDays = age,
                    ActionPath = "/admin/support/tickets",
                    ActionLabel = "پاسخ به تیکت"
                };
            }).ToList();
        }

        private async Task<List<AdminActionItemVM>> GetProviderIssuesAsync(DateTime now, int take)
        {
            var items = new List<AdminActionItemVM>();

            var stylistsWithoutService = await _context.Stylists
                .AsNoTracking()
                .Include(x => x.Person)
                .Where(x => !x.IsWorkShop && !x.StylistServices.Any())
                .OrderBy(x => x.CreateDate)
                .Take(take)
                .ToListAsync();

            items.AddRange(stylistsWithoutService.Select(x => new AdminActionItemVM
            {
                Type = "providers",
                Severity = x.AccountStatus == "2" ? "danger" : "warning",
                Title = "آرایشگر بدون خدمت",
                Description = "این آرایشگر هیچ خدمت قابل رزروی ندارد.",
                EntityID = x.ID,
                EntityName = x.StylistName,
                CreatedAt = x.CreateDate,
                AgeDays = AgeDays(now, x.CreateDate),
                ActionPath = "/admin/stylists",
                ActionLabel = "بررسی آرایشگر"
            }));

            var salonsWithoutStylists = await _context.Stylists
                .AsNoTracking()
                .Where(x => x.IsWorkShop && !_context.Stylists.Any(s => s.StylistParentID == x.ID))
                .OrderBy(x => x.CreateDate)
                .Take(take)
                .ToListAsync();

            items.AddRange(salonsWithoutStylists.Select(x => new AdminActionItemVM
            {
                Type = "providers",
                Severity = "warning",
                Title = "سالن بدون آرایشگر",
                Description = "این سالن هنوز آرایشگر زیرمجموعه ندارد.",
                EntityID = x.ID,
                EntityName = x.StylistName,
                CreatedAt = x.CreateDate,
                AgeDays = AgeDays(now, x.CreateDate),
                ActionPath = "/admin/salons",
                ActionLabel = "بررسی سالن"
            }));

            return items.Take(take).ToList();
        }

        private async Task<List<AdminActionItemVM>> GetServiceIssuesAsync(DateTime now, int take)
        {
            var rows = await _context.ServiceManagements
                .AsNoTracking()
                .Where(x =>
                    !_context.ServiceManagements.Any(child => child.ServiceParentID == x.ID) &&
                    !x.StylistServices.Any())
                .OrderBy(x => x.CreateDate)
                .Take(take)
                .ToListAsync();

            return rows.Select(x => new AdminActionItemVM
            {
                Type = "services",
                Severity = "info",
                Title = "خدمت بدون ارائه‌دهنده",
                Description = "این خدمت نهایی هیچ آرایشگر/سالنی ندارد و قابل رزرو نیست.",
                EntityID = x.ID,
                EntityName = x.ServiceName,
                CreatedAt = x.CreateDate,
                AgeDays = AgeDays(now, x.CreateDate),
                ActionPath = "/admin/services",
                ActionLabel = "مدیریت خدمت"
            }).ToList();
        }

        private static bool ShouldInclude(string selectedType, string itemType)
        {
            return string.IsNullOrWhiteSpace(selectedType) || selectedType.Trim().ToLower() == itemType;
        }

        private static int SeverityRank(string severity)
        {
            return severity switch
            {
                "danger" => 0,
                "warning" => 1,
                _ => 2
            };
        }

        private static int AgeDays(DateTime now, DateTime? date)
        {
            if (!date.HasValue) return 0;
            return Math.Max(0, Convert.ToInt32(Math.Floor((now - date.Value).TotalDays)));
        }

        private static string GetPersonName(NobatPlusDATA.Domain.Person? person)
        {
            if (person == null) return "";
            return $"{person.FirstName} {person.LastName}".Trim();
        }
    }
}
