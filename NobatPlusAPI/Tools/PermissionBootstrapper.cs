using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MTPermissionCenter.EFCore.Entities;
using NobatPlusDATA.DataLayer;
using NobatPlusDATA.Tools;
using static NobatPlusDATA.Tools.DbTools;

namespace NobatPlusAPI.Tools
{
    public static class PermissionBootstrapper
    {
        private const string ActionPermissionType = "Action";
        private const string MenuPermissionType = "Menu";

        public static async Task SyncAsync(IServiceProvider services, ILogger logger)
        {
            try
            {
                var context = services.GetRequiredService<NobatPlusContext>();
                var actionProvider = services.GetRequiredService<IActionDescriptorCollectionProvider>();
                var now = DateTime.Now.ToShamsi();

                var permissions = await context.Permissions.ToListAsync();
                var existingActionByKey = permissions
                    .Where(x => string.Equals(x.PermissionType, ActionPermissionType, StringComparison.OrdinalIgnoreCase))
                    .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                    .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

                var existingMenuByRoute = permissions
                    .Where(x => string.Equals(x.PermissionType, MenuPermissionType, StringComparison.OrdinalIgnoreCase))
                    .Where(x => !string.IsNullOrWhiteSpace(x.Routename))
                    .GroupBy(x => x.Routename, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

                var addedPermissions = 0;
                var addedLinks = 0;
                var removedLinks = 0;

                foreach (var action in GetProtectedControllerActions(actionProvider))
                {
                    var roleIds = ResolveActionRoles(action);
                    var keys = new[]
                    {
                        $"{action.ControllerName}/{action.ActionName}",
                        $"{action.ControllerName}.{action.ActionName}".ToLowerInvariant()
                    };

                    foreach (var key in keys.Distinct(StringComparer.OrdinalIgnoreCase))
                    {
                        if (!existingActionByKey.TryGetValue(key, out var permission))
                        {
                            permission = new MTPermissionCenter_Permission
                            {
                                CreateDate = now,
                                UpdateDate = now,
                                Description = $"API action: {key}",
                                Name = key,
                                Key = key,
                                Icon = "Shield",
                                Routename = key,
                                PermissionType = ActionPermissionType,
                                OtherLangs = "",
                                IsActive = true
                            };

                            context.Permissions.Add(permission);
                            existingActionByKey[key] = permission;
                            addedPermissions++;
                        }

                        var linkSync = await SyncRoleLinksAsync(context, permission, roleIds, now);
                        addedLinks += linkSync.Added;
                        removedLinks += linkSync.Removed;
                    }
                }

                foreach (var menu in MenuDefinitions())
                {
                    if (!existingMenuByRoute.TryGetValue(menu.Route, out var permission))
                    {
                        permission = new MTPermissionCenter_Permission
                        {
                            CreateDate = now,
                            UpdateDate = now,
                            Description = menu.Description,
                            Name = menu.Name,
                            Key = $"menu:{menu.Route}",
                            Icon = menu.Icon,
                            Routename = menu.Route,
                            PermissionType = MenuPermissionType,
                            OtherLangs = "",
                            IsActive = true
                        };

                        context.Permissions.Add(permission);
                        existingMenuByRoute[menu.Route] = permission;
                        addedPermissions++;
                    }

                    var linkSync = await SyncRoleLinksAsync(context, permission, menu.RoleIds, now);
                    addedLinks += linkSync.Added;
                    removedLinks += linkSync.Removed;
                }

                if (addedPermissions > 0 || addedLinks > 0 || removedLinks > 0)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation("Permission bootstrap completed. Added permissions: {Permissions}; added role links: {RoleLinks}; removed role links: {RemovedRoleLinks}", addedPermissions, addedLinks, removedLinks);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Permission bootstrap failed.");
            }
        }

        private static IEnumerable<ControllerActionDescriptor> GetProtectedControllerActions(IActionDescriptorCollectionProvider actionProvider)
        {
            return actionProvider.ActionDescriptors.Items
                .OfType<ControllerActionDescriptor>()
                .Where(action => !HasAllowAnonymous(action) && HasAuthorize(action))
                .GroupBy(action => $"{action.ControllerName}/{action.ActionName}", StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First());
        }

        private static bool HasAllowAnonymous(ControllerActionDescriptor action)
        {
            return action.EndpointMetadata.OfType<IAllowAnonymous>().Any()
                || action.MethodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any()
                || action.ControllerTypeInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any();
        }

        private static bool HasAuthorize(ControllerActionDescriptor action)
        {
            return action.EndpointMetadata.OfType<IAuthorizeData>().Any()
                || action.MethodInfo.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any()
                || action.ControllerTypeInfo.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any();
        }

        private static async Task<RoleLinkSyncResult> SyncRoleLinksAsync(NobatPlusContext context, MTPermissionCenter_Permission permission, IEnumerable<long> roleIds, DateTime now)
        {
            var added = 0;
            var removed = 0;
            var desiredRoleIds = roleIds.Distinct().ToHashSet();

            if (permission.ID > 0)
            {
                var managedLinks = await context.PermissionRoles
                    .Where(x => x.PermissionId == permission.ID && ManagedBaseRoleIds.Contains(x.RoleId))
                    .ToListAsync();

                var linksToRemove = managedLinks
                    .Where(x => !desiredRoleIds.Contains(x.RoleId))
                    .ToList();

                if (linksToRemove.Count > 0)
                {
                    context.PermissionRoles.RemoveRange(linksToRemove);
                    removed += linksToRemove.Count;
                }
            }

            foreach (var roleId in desiredRoleIds)
            {
                if (permission.ID > 0)
                {
                    var exists = await context.PermissionRoles.AnyAsync(x => x.PermissionId == permission.ID && x.RoleId == roleId);
                    if (exists) continue;
                }

                var roleLink = new MTPermissionCenter_PermissionRole
                {
                    CreateDate = now,
                    UpdateDate = now,
                    IsActive = true,
                    PermissionId = permission.ID,
                    RoleId = roleId,
                    OwnerOnly = false
                };

                if (permission.ID == 0)
                {
                    roleLink.Permission = permission;
                }

                context.PermissionRoles.Add(roleLink);
                added++;
            }

            return new RoleLinkSyncResult(added, removed);
        }

        private static IReadOnlyCollection<long> ResolveActionRoles(ControllerActionDescriptor action)
        {
            var controller = action.ControllerName;
            var actionName = action.ActionName;

            var requiredRoles = GetRequireRoleIds(action);
            if (requiredRoles.Count > 0)
                return requiredRoles;

            if (controller.Equals("SMSMessage", StringComparison.OrdinalIgnoreCase)
                && actionName.Equals("AddSMSMessage_Base", StringComparison.OrdinalIgnoreCase))
                return AllPanelRoles;

            if ((controller.Equals("ServiceOption", StringComparison.OrdinalIgnoreCase)
                    || controller.Equals("ServiceOptionValue", StringComparison.OrdinalIgnoreCase))
                && actionName.StartsWith("Get", StringComparison.OrdinalIgnoreCase))
                return AllPanelRoles;

            if (IsAdminOnlyAction(controller, actionName))
                return AdminOnly;

            return controller switch
            {
                "CustomerDashboard" => CustomerOnly,
                "StylistDashboard" when actionName.Contains("Salon", StringComparison.OrdinalIgnoreCase) => SalonOnly,
                "StylistDashboard" => StylistOnly,
                "Address" when actionName.StartsWith("Delete", StringComparison.OrdinalIgnoreCase)
                    || actionName.StartsWith("Exist", StringComparison.OrdinalIgnoreCase) => AdminOnly,
                "Address" => AllPanelRoles,
                "Booking" when actionName.StartsWith("Delete", StringComparison.OrdinalIgnoreCase)
                    || actionName.StartsWith("Exist", StringComparison.OrdinalIgnoreCase) => AdminOnly,
                "Booking" => AllPanelRoles,
                "BookingService" => AdminOnly,
                "BookingServiceOptionValue" => AdminOnly,
                "CustomerDiscount" when actionName.StartsWith("Get", StringComparison.OrdinalIgnoreCase) => CustomerOnly,
                "CustomerDiscount" => AdminOnly,
                "Wallet" when actionName.Equals("ChargeWallet_Base", StringComparison.OrdinalIgnoreCase) => NoRoles,
                "Wallet" when actionName.Contains("Customer", StringComparison.OrdinalIgnoreCase)
                    || actionName.Contains("Transactions", StringComparison.OrdinalIgnoreCase) => AdminOnly,
                "Wallet" => CustomerOnly,
                "Customer" when actionName.Equals("GetAllCustomers_Base", StringComparison.OrdinalIgnoreCase) => ProviderOnly,
                "Customer" when actionName.Equals("GetSalonBookingCustomers_Base", StringComparison.OrdinalIgnoreCase) => ProviderOnly,
                "Customer" when actionName.Equals("QuickAddCustomer", StringComparison.OrdinalIgnoreCase) => ProviderOnly,
                "Customer" when actionName.StartsWith("Delete", StringComparison.OrdinalIgnoreCase) => AdminOnly,
                "Notification" when actionName.StartsWith("Add", StringComparison.OrdinalIgnoreCase)
                    || actionName.StartsWith("Edit", StringComparison.OrdinalIgnoreCase)
                    || actionName.StartsWith("Delete", StringComparison.OrdinalIgnoreCase) => AdminOnly,
                "Payment" when actionName.StartsWith("Delete", StringComparison.OrdinalIgnoreCase)
                    || actionName.StartsWith("Edit", StringComparison.OrdinalIgnoreCase)
                    || actionName.StartsWith("Exist", StringComparison.OrdinalIgnoreCase) => AdminOnly,
                "Payment" => CustomerOnly,
                "PaymentDetail" when actionName.StartsWith("Add", StringComparison.OrdinalIgnoreCase)
                    || actionName.StartsWith("Edit", StringComparison.OrdinalIgnoreCase)
                    || actionName.StartsWith("Delete", StringComparison.OrdinalIgnoreCase)
                    || actionName.StartsWith("Exist", StringComparison.OrdinalIgnoreCase) => AdminOnly,
                "PaymentDetail" => CustomerOnly,
                "PaymentHistory" => CustomerOnly,
                "PaymentBooking" => AdminOnly,
                "PaymentDetailOptionValue" => AdminOnly,
                "Person" when actionName.Equals("GetPersonById_Base", StringComparison.OrdinalIgnoreCase)
                    || actionName.Equals("EditPerson", StringComparison.OrdinalIgnoreCase)
                    || actionName.Equals("EditPerson_Base", StringComparison.OrdinalIgnoreCase) => AllPanelRoles,
                "Person" => AdminOnly,
                "RateQuestion" when actionName.StartsWith("Get", StringComparison.OrdinalIgnoreCase) => AllPanelRoles,
                "RateQuestion" => AdminOnly,
                "Review" when actionName.StartsWith("Get", StringComparison.OrdinalIgnoreCase) => AllPanelRoles,
                "Review" when actionName.StartsWith("Add", StringComparison.OrdinalIgnoreCase)
                    || actionName.StartsWith("Edit", StringComparison.OrdinalIgnoreCase) => CustomerOnly,
                "Review" => AdminOnly,
                "ServiceDiscount" => AdminOnly,
                "SupportTicket" when actionName.StartsWith("Update", StringComparison.OrdinalIgnoreCase)
                    || actionName.StartsWith("Delete", StringComparison.OrdinalIgnoreCase) => AdminOnly,
                "FinancialAccount" when actionName.Contains("Admin", StringComparison.OrdinalIgnoreCase)
                    || actionName.Contains("SettlementRequests", StringComparison.OrdinalIgnoreCase)
                    || actionName.Contains("UpdateSettlementStatus", StringComparison.OrdinalIgnoreCase) => AdminOnly,
                "FinancialAccount" => ProviderOnly,
                _ when CustomerControllers.Contains(controller) => CustomerOnly,
                _ when ProviderControllers.Contains(controller) => ProviderOnly,
                _ when SharedPanelControllers.Contains(controller) => AllPanelRoles,
                _ => AdminOnly,
            };
        }

        private static IReadOnlyCollection<long> GetRequireRoleIds(ControllerActionDescriptor action)
        {
            var methodAttribute = action.MethodInfo
                .GetCustomAttributes(typeof(RequireRoleAttribute), true)
                .OfType<RequireRoleAttribute>()
                .FirstOrDefault();

            if (methodAttribute != null)
                return methodAttribute.RoleIds.ToArray();

            var controllerAttribute = action.ControllerTypeInfo
                .GetCustomAttributes(typeof(RequireRoleAttribute), true)
                .OfType<RequireRoleAttribute>()
                .FirstOrDefault();

            return controllerAttribute?.RoleIds.ToArray() ?? NoRoles;
        }

        private static bool IsAdminOnlyAction(string controller, string actionName)
        {
            return controller.StartsWith("Admin", StringComparison.OrdinalIgnoreCase)
                || AdminOnlyControllers.Contains(controller)
                || actionName.Contains("Admin", StringComparison.OrdinalIgnoreCase)
                || actionName.StartsWith("GetAll", StringComparison.OrdinalIgnoreCase) && AdminReadMostlyControllers.Contains(controller)
                || actionName.StartsWith("Delete", StringComparison.OrdinalIgnoreCase) && AdminDeleteControllers.Contains(controller);
        }

        private static IEnumerable<MenuPermissionDefinition> MenuDefinitions()
        {
            return new[]
            {
                Menu("داشبورد ادمین", "/admin/dashboard", "LayoutDashboard", BaseRole.Admin),
                Menu("مدیریت کاربران", "/admin/users", "Users", BaseRole.Admin),
                Menu("مدیریت سالن‌ها", "/admin/salons", "Building2", BaseRole.Admin),
                Menu("آرایشگران", "/admin/stylists", "UserCog", BaseRole.Admin),
                Menu("مشتریان", "/admin/customers", "Users", BaseRole.Admin),
                Menu("نقش‌ها و دسترسی‌ها", "/admin/roles", "Shield", BaseRole.Admin),
                Menu("مدیریت خدمات", "/admin/services", "Scissors", BaseRole.Admin),
                Menu("مدیریت نوبت‌ها", "/admin/appointments", "CalendarDays", BaseRole.Admin),
                Menu("پرداخت‌ها", "/admin/payments", "CreditCard", BaseRole.Admin),
                Menu("تراکنش‌های کیف پول", "/admin/wallet-transactions", "Wallet2", BaseRole.Admin),
                Menu("تسویه حساب‌ها", "/admin/payments/settlements", "CircleDollarSign", BaseRole.Admin),
                Menu("سیاست‌های مالی", "/admin/financial-policies", "CircleDollarSign", BaseRole.Admin),
                Menu("کدهای تخفیف", "/admin/discounts/coupons", "BadgePercent", BaseRole.Admin),
                Menu("پشتیبانی", "/admin/support/tickets", "MessageSquare", BaseRole.Admin),
                Menu("گزارش‌ها", "/admin/reports", "BarChart3", BaseRole.Admin),
                Menu("مانیتورینگ سیستم", "/admin/monitoring", "Activity", BaseRole.Admin),
                Menu("مرکز رسیدگی", "/admin/action-center", "ClipboardList", BaseRole.Admin),
                Menu("اعلان‌ها", "/admin/notifications", "Bell", BaseRole.Admin),
                Menu("گزارش عملیات ادمین", "/admin/audit-logs", "Activity", BaseRole.Admin),
                Menu("بررسی مدارک", "/admin/documents", "FileCheck2", BaseRole.Admin),
                Menu("تنظیمات سیستم", "/admin/settings", "Settings", BaseRole.Admin),
                Menu("راهنمای API", "/admin/api-guide", "BookOpenText", BaseRole.Admin),

                Menu("داشبورد مشتری", "/customer/dashboard", "LayoutDashboard", BaseRole.Customer),
                Menu("پروفایل مشتری", "/customer/profile", "User", BaseRole.Customer),
                Menu("نوبت‌های مشتری", "/customer/appointments", "CalendarDays", BaseRole.Customer),
                Menu("آرایشگران مشتری", "/customer/stylists", "Users", BaseRole.Customer),
                Menu("پرداخت‌های مشتری", "/customer/payments", "CreditCard", BaseRole.Customer),
                Menu("کیف پول مشتری", "/customer/wallet", "Wallet2", BaseRole.Customer),
                Menu("تخفیف‌های مشتری", "/customer/discounts", "BadgePercent", BaseRole.Customer),
                Menu("اعلان‌های مشتری", "/customer/notifications", "Bell", BaseRole.Customer),
                Menu("تنظیمات مشتری", "/customer/settings", "Settings", BaseRole.Customer),

                Menu("داشبورد آرایشگر", "/stylist/dashboard", "LayoutDashboard", BaseRole.Stylist),
                Menu("پروفایل آرایشگر", "/stylist/profile", "IdCard", BaseRole.Stylist),
                Menu("اطلاعات شغلی آرایشگر", "/stylist/workInfo", "Briefcase", BaseRole.Stylist),
                Menu("شبکه‌های اجتماعی آرایشگر", "/stylist/SocialNetwork", "Share2", BaseRole.Stylist),
                Menu("نوبت‌های مشتریان آرایشگر", "/stylist/customer-appointments", "CalendarDays", BaseRole.Stylist),
                Menu("نوبت‌های آرایشگر", "/stylist/stylist-appointments", "CalendarCheck", BaseRole.Stylist),
                Menu("خدمات آرایشگر", "/stylist/services", "Scissors", BaseRole.Stylist),
                Menu("ساعات کاری آرایشگر", "/stylist/worktime", "Clock", BaseRole.Stylist),
                Menu("مرخصی‌های آرایشگر", "/stylist/leaves", "CalendarClock", BaseRole.Stylist),
                Menu("مشتریان آرایشگر", "/stylist/customers", "Users", BaseRole.Stylist),
                Menu("نظرات آرایشگر", "/stylist/reviews", "MessageSquareText", BaseRole.Stylist),
                Menu("QR تبلیغاتی آرایشگر", "/stylist/promotion-qr", "QrCode", BaseRole.Stylist),
                Menu("گزارش‌های آرایشگر", "/stylist/reports", "BarChart3", BaseRole.Stylist),
                Menu("تقویم آرایشگر", "/stylist/calendar", "CalendarClock", BaseRole.Stylist),
                Menu("حساب مالی آرایشگر", "/stylist/financial", "CircleDollarSign", BaseRole.Stylist),

                Menu("داشبورد سالن", "/salon/dashboard", "LayoutDashboard", BaseRole.Salon),
                Menu("پروفایل سالن", "/salon/profile", "IdCard", BaseRole.Salon),
                Menu("اطلاعات شغلی سالن", "/salon/workInfo", "Briefcase", BaseRole.Salon),
                Menu("شبکه‌های اجتماعی سالن", "/salon/SocialNetwork", "Share2", BaseRole.Salon),
                Menu("ساعات کاری سالن", "/salon/SalonWorkTime", "Clock", BaseRole.Salon),
                Menu("نوبت‌های سالن", "/salon/stylist-appointmentsSalon", "CalendarDays", BaseRole.Salon),
                Menu("لیست آرایشگران سالن", "/salon/stylists", "Users", BaseRole.Salon),
                Menu("برنامه کاری آرایشگران سالن", "/salon/work-time-list", "CalendarClock", BaseRole.Salon),
                Menu("خدمات آرایشگران سالن", "/salon/stylist-services-list", "Scissors", BaseRole.Salon),
                Menu("شبکه‌های اجتماعی آرایشگران سالن", "/salon/StylistSocialNetwork", "Share2", BaseRole.Salon),
                Menu("مشتریان سالن", "/salon/customerStylistList", "Users", BaseRole.Salon),
                Menu("تقویم سالن", "/salon/calendar", "CalendarClock", BaseRole.Salon),
                Menu("مرخصی‌های سالن", "/salon/leaves", "CalendarClock", BaseRole.Salon),
                Menu("QR تبلیغاتی سالن", "/salon/promotion-qr", "QrCode", BaseRole.Salon),
                Menu("نظرات سالن", "/salon/reviews", "MessageCircle", BaseRole.Salon),
                Menu("حساب مالی سالن", "/salon/financial", "CircleDollarSign", BaseRole.Salon),
            };
        }

        private static MenuPermissionDefinition Menu(string name, string route, string icon, BaseRole role)
        {
            return new MenuPermissionDefinition(name, route, icon, $"Menu route: {route}", new[] { (long)role, (long)BaseRole.Admin });
        }

        private static readonly long[] AdminOnly = { (long)BaseRole.Admin };
        private static readonly long[] NoRoles = Array.Empty<long>();
        private static readonly long[] CustomerOnly = { (long)BaseRole.Customer, (long)BaseRole.Admin };
        private static readonly long[] StylistOnly = { (long)BaseRole.Stylist, (long)BaseRole.Admin };
        private static readonly long[] SalonOnly = { (long)BaseRole.Salon, (long)BaseRole.Admin };
        private static readonly long[] ProviderOnly = { (long)BaseRole.Stylist, (long)BaseRole.Salon, (long)BaseRole.Admin };
        private static readonly long[] AllPanelRoles = { (long)BaseRole.Customer, (long)BaseRole.Stylist, (long)BaseRole.Salon, (long)BaseRole.Admin };
        private static readonly HashSet<long> ManagedBaseRoleIds = new(AllPanelRoles);

        private static readonly HashSet<string> AdminOnlyControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "ApiGuide", "City", "Discount", "DiscountAssignment", "FileUpload", "JobType", "Log", "Login",
            "Permission", "PermissionRole", "Register", "Role", "Setting", "SMSMessage", "UserPermission",
            "ServiceManagement", "ServiceOption", "ServiceOptionValue"
        };

        private static readonly HashSet<string> AdminReadMostlyControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "Person", "Address", "Customer", "Stylist", "Notification", "Payment", "Review"
        };

        private static readonly HashSet<string> AdminDeleteControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "Person", "Address", "Customer", "Stylist", "Notification", "Payment", "Review"
        };

        private static readonly HashSet<string> CustomerControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "CustomerDashboard", "CustomerDiscount", "Wallet"
        };

        private static readonly HashSet<string> ProviderControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "CheckAvailability", "FinancialAccount", "SocialNetwork", "StylistDashboard", "StylistPacific",
            "StylistService", "StylistServicePriceVariant", "StylistServicePriceVariantOptionValue", "WorkTime"
        };

        private static readonly HashSet<string> SharedPanelControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "Address", "Booking", "BookingService", "BookingServiceOptionValue", "Customer", "FileCenter",
            "Image", "Notification", "Payment", "PaymentBooking", "PaymentDetail", "PaymentDetailOptionValue",
            "PaymentHistory", "Person", "RateHistory", "RateQuestion", "Review", "ServiceDiscount",
            "SupportTicket", "Stylist"
        };

        private sealed record MenuPermissionDefinition(string Name, string Route, string Icon, string Description, IReadOnlyCollection<long> RoleIds);
        private sealed record RoleLinkSyncResult(int Added, int Removed);
    }
}
