using AutoMapper;
using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusAPI.Models.Payment;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;
using Parbad;
using Parbad.Gateway.ZarinPal;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static NobatPlusDATA.Tools.DbTools;

namespace NobatPlusAPI.Controllers
{
    [Route("Payment")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class PaymentController : ControllerBase
    {
        IPaymentRep _PaymentRep;
        IPaymentHistoryRep _PaymentHistoryRep;
        ICustomerRep _customerRep;
        IBookingRep _bookingRep;
        ISettingRep _settingRep;
        IWorkerServiceRep _workerServiceRep;
        ILogRep _logRep;
        private readonly IOnlinePayment _onlinePayment;
        private readonly IMapper _mapper;
        private readonly IBillingRep _billingRep;

        public PaymentController(IPaymentRep PaymentRep,IPaymentHistoryRep paymentHistoryRep,ICustomerRep customerRep,IWorkerServiceRep workerServiceRep,ILogRep logRep, IOnlinePayment onlinePayment, IMapper mapper, IBillingRep billingRep,ISettingRep settingRep,IBookingRep bookingRep)
        {
           _PaymentRep = PaymentRep;
            _PaymentHistoryRep = paymentHistoryRep;
            _customerRep = customerRep;
            _workerServiceRep = workerServiceRep;
           _logRep = logRep;
            _onlinePayment = onlinePayment;
            _mapper = mapper;
            _billingRep = billingRep;
            _settingRep = settingRep;
            _bookingRep = bookingRep;
        }

        [HttpPost("GetAllPayments")]
        public async Task<ActionResult<ListResultObject<PaymentVM>>> GetAllPayments_Base(GetPaymentListRequestBody requestBody)
        {
            return await GetAllPaymentsInternal(requestBody);
        }

        [HttpPost("GetPaymentById_Base")]
        public async Task<ActionResult<RowResultObject<PaymentVM>>> GetPaymentById_Base(GetRowRequestBody requestBody)
        {
            return await GetPaymentByIdInternal(requestBody);
        }

        [HttpPost("ExistPayment_Base")]
        public async Task<ActionResult<BitResultObject>> ExistPayment_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _PaymentRep.ExistPaymentAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddPayment")]
        public async Task<ActionResult<BitResultObject>> AddPayment(AddEditPaymentRequestBody requestBody)
        {
            var result = new BitResultObject();

            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            long customerId;
            if (User.GetCurrentRoleId() == (long) BaseRole.Admin && requestBody.CustomerID > 0)
            {
                customerId = requestBody.CustomerID;
            }
            else
            {
                var userId = User.GetCurrentUserId();
                var dbcustomer = await _customerRep.ExistCustomerAsync(userId.ToString(), "personid");
                if (!dbcustomer.Status)
                {
                    return Forbid();
                }
                customerId = dbcustomer.ID;
            }
            var discountId = requestBody.DiscountID ?? 0;
            var bookingIds = NormalizeBookingIds(requestBody);
            if (!bookingIds.Any())
            {
                result.Status = false;
                result.ErrorMessage = "حداقل یک رزرو برای پرداخت انتخاب کنید";
                return BadRequest(result);
            }

            var calcPayment = await _PaymentRep.CalculatePaymentAsync(customerId,bookingIds,discountId);

            if (!calcPayment.Status)
            {
                result.Status = calcPayment.Status;
                result.ErrorMessage = calcPayment.ErrorMessage;
                return BadRequest(result);

            }

            Payment Payment = new Payment()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                DepositAmount = calcPayment.Result.PayedAmount,
                TotalServiceAmount = calcPayment.Result.TotalServiceAmount,
                PlarformAmount = calcPayment.Result.PlatformAmount,
                WorkerAmount = calcPayment.Result.WorkerAmount,
                AllPaymentAmount = calcPayment.Result.AllPaymentAmount,
                PayedAmount = 0,
                RemainAmount = calcPayment.Result.AllPaymentAmount,
                DiscountedServiceAmount = calcPayment.Result.DiscountedServiceAmount,
                VatAmount = calcPayment.Result.VatAmount,
                PaymentStatus = "pending",
                PaymentDate = requestBody.PaymentDate ?? DateTime.Now.ToShamsi(),
                PaymentLevel = requestBody.PaymentLevel,
                PaymentFinished = false,
                OwnerPersonID = User.GetCurrentRoleId() == (long) BaseRole.Admin ? null : User.GetCurrentUserId(),
                DiscountID = discountId,
                Description = requestBody.Description,
                PaymentBookings = bookingIds.Select(bookingId => new PaymentBooking
                {
                    BookingID = bookingId
                }).ToList(),
                PaymentDetails = calcPayment.Result.workerServiceWithDiscountDtos.Select(d=> new PaymentDetail()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    Description = requestBody.Description,
                    BookingID = d.BookingID,
                    WorkerID = d.WorkerID,
                    ServiceManagementID = d.ServiceManagementID,
                    WorkerServicePriceVariantID = d.WorkerServicePriceVariantID,
                    AppliedOptionSummary = d.AppliedOptionSummary,
                    WorkerServiceAmount = d.ServicePrice,
                    DiscountAmount = d.ServicePrice - d.PriceAfterDiscount,
                    DiscountPercent = d.DiscountPercent,
                    OptionValues = d.AppliedOptionValueIDs.Select(optionValueId => new PaymentDetailOptionValue
                    {
                        ServiceOptionValueID = optionValueId
                    }).ToList()
                }).ToList()
            };

            result = await _PaymentRep.AddPaymentAsync(Payment);

            if (!result.Status)
            {
                return BadRequest(result);
            }


            if (result.Status)
            {
                #region AddLog

                Log log = new Log()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    LogTime = DateTime.Now.ToShamsi(),
                    ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),

                };
                await _logRep.AddLogAsync(log);

                #endregion


                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPut("EditPayment_Base")]
        public async Task<ActionResult<BitResultObject>> EditPayment_Base(AddEditPaymentRequestBody requestBody)
        {
            if (!await _billingRep.CanAccessPaymentAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin)) return Forbid();
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var theRow = await _PaymentRep.GetPaymentByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
                return BadRequest(result);
            }

            long customerId;
            if (User.GetCurrentRoleId() == (long) BaseRole.Admin && requestBody.CustomerID > 0)
            {
                customerId = requestBody.CustomerID;
            }
            else
            {
                var userId = User.GetCurrentUserId();
                var dbcustomer = await _customerRep.ExistCustomerAsync(userId.ToString(), "personid");
                if (!dbcustomer.Status)
                {
                    return Forbid();
                }
                customerId = dbcustomer.ID;
            }
            var discountId = requestBody.DiscountID ?? 0;
            var bookingIds = NormalizeBookingIds(requestBody);
            if (!bookingIds.Any())
            {
                result.Status = false;
                result.ErrorMessage = "حداقل یک رزرو برای پرداخت انتخاب کنید";
                return BadRequest(result);
            }

            var calcPayment = await _PaymentRep.CalculatePaymentAsync(customerId, bookingIds, discountId);

            if (!calcPayment.Status)
            {
                result.Status = calcPayment.Status;
                result.ErrorMessage = calcPayment.ErrorMessage;
                return BadRequest(result);

            }

            Payment Payment = new Payment()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ID = requestBody.ID,
                DepositAmount = calcPayment.Result.PayedAmount,
                TotalServiceAmount = calcPayment.Result.TotalServiceAmount,
                PlarformAmount = calcPayment.Result.PlatformAmount,
                WorkerAmount = calcPayment.Result.WorkerAmount,
                AllPaymentAmount = calcPayment.Result.AllPaymentAmount,
                PayedAmount = 0,
                RemainAmount = calcPayment.Result.AllPaymentAmount,
                DiscountedServiceAmount = calcPayment.Result.DiscountedServiceAmount,
                VatAmount = calcPayment.Result.VatAmount,
                PaymentStatus = "pending",
                PaymentDate = requestBody.PaymentDate ?? DateTime.Now.ToShamsi(),
                PaymentLevel = requestBody.PaymentLevel,
                PaymentFinished = false,
                DiscountID = discountId,
                Description = requestBody.Description,
                PaymentBookings = bookingIds.Select(bookingId => new PaymentBooking
                {
                    PaymentID = requestBody.ID,
                    BookingID = bookingId
                }).ToList(),
                PaymentDetails = calcPayment.Result.workerServiceWithDiscountDtos.Select(d => new PaymentDetail()
                {
                    CreateDate = theRow.Result.CreateDate,
                    UpdateDate = DateTime.Now.ToShamsi(),
                    Description = requestBody.Description,
                    PaymentID = requestBody.ID,
                    BookingID = d.BookingID,
                    WorkerID = d.WorkerID,
                    ServiceManagementID = d.ServiceManagementID,
                    WorkerServicePriceVariantID = d.WorkerServicePriceVariantID,
                    AppliedOptionSummary = d.AppliedOptionSummary,
                    WorkerServiceAmount = d.ServicePrice,
                    DiscountAmount = d.ServicePrice - d.PriceAfterDiscount,
                    DiscountPercent = d.DiscountPercent,
                    OptionValues = d.AppliedOptionValueIDs.Select(optionValueId => new PaymentDetailOptionValue
                    {
                        ServiceOptionValueID = optionValueId
                    }).ToList()
                }).ToList()
            };
            result = await _PaymentRep.EditPaymentAsync(Payment);

            if (result.Status)
            {

                #region AddLog

                Log log = new Log()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    LogTime = DateTime.Now.ToShamsi(),
                    ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),

                };
                await _logRep.AddLogAsync(log);

                #endregion

                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpDelete("DeletePayment")]
        public async Task<ActionResult<BitResultObject>> DeletePayment(GetRowRequestBody requestBody)
        {
            return await DeletePaymentInternal(requestBody);
        }


        [HttpPost("RequestPayment")]
        public async Task<ActionResult<RowResultObject<RequestPaymentResultBody>>> RequestPayment(RequestPaymentRequestBody requestBody)
        {
            var result = new RowResultObject<RequestPaymentResultBody>();
            long attemptId = 0;
            try
            {
                if (!ModelState.IsValid) return BadRequest(requestBody);
                var gateway = ToolBox.GetActivePayGateway();
                if (gateway == null || string.IsNullOrWhiteSpace(gateway.GatewayName))
                    throw new InvalidOperationException("درگاه پرداخت فعال تنظیم نشده است");
                var attempt = await _billingRep.CreateGatewayAttemptAsync(requestBody.PaymentID, gateway.GatewayName,
                    User.GetCurrentUserId(), User.GetCurrentRoleId() == (long)BaseRole.Admin);
                if (!attempt.Status) return BadRequest(new RowResultObject<RequestPaymentResultBody> { Status = false, ErrorMessage = attempt.ErrorMessage });
                attemptId = attempt.ID;
                var historyRow = await _PaymentHistoryRep.GetPaymentHistoryByIdAsync(attemptId);
                if (!historyRow.Status || historyRow.Result == null) throw new InvalidOperationException(historyRow.ErrorMessage);
                var history = historyRow.Result;
                var callbackUrl = Url.Action(nameof(VerifyPayment), "Payment", new { PayId = history.ID, browserCallback = true }, Request.Scheme);
                var invoice = await _onlinePayment.RequestAsync(builder =>
                {
                    builder.SetCallbackUrl(callbackUrl).SetAmount(history.Amount).SetTrackingNumber(history.ID);
                    if (gateway.GatewayName.Equals("zarinpal", StringComparison.OrdinalIgnoreCase))
                        builder.SetZarinPalData(new ZarinPalInvoice(description: $"Payment {history.PaymentID}, attempt {history.ID}")).UseZarinPal();
                    else
                        builder.SetGateway(gateway.GatewayName);
                });
                var url = invoice.IsSucceed ? invoice.GatewayTransporter.Descriptor.Url : null;
                var saved = await _billingRep.SaveGatewayAttemptAsync(history.ID, invoice.IsSucceed, url, invoice.Message);
                if (!saved.Status) throw new InvalidOperationException(saved.ErrorMessage);
                if (!invoice.IsSucceed) throw new InvalidOperationException(invoice.Message);
                result.Result = new RequestPaymentResultBody { PaymentHistoryID = history.ID, Amount = history.Amount, PayGatewayUrl = url };
                return Ok(result);
            }
            catch (Exception ex)
            {
                if (attemptId > 0) await _billingRep.SaveGatewayAttemptAsync(attemptId, false, null, ex.Message);
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
                return BadRequest(result);
            }
        }

        [AllowAnonymous]
        [HttpGet("VerifyPayment")]
        public async Task<IActionResult> VerifyPayment(long PayId = 0, string? paymentToken = "", string? Authority = "", string? Status = "", bool browserCallback = false)
        {
            try
            {
                if (PayId <= 0) throw new InvalidOperationException("شناسه پرداخت معتبر نیست");
                var row = await _PaymentHistoryRep.GetPaymentHistoryByIdAsync(PayId);
                if (!row.Status || row.Result == null) throw new InvalidOperationException("سابقه پرداخت یافت نشد");
                var history = row.Result;
                if (history.PaymentStatus)
                    return await BuildPaymentVerificationResponseAsync(history.ID, true, history.TransactionCode, null, browserCallback);
                var invoice = await _onlinePayment.FetchAsync();
                if (invoice.TrackingNumber != history.ID || (decimal)invoice.Amount != history.Amount ||
                    !string.Equals(invoice.GatewayName, history.GatewayName, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("اطلاعات تاییدشده درگاه با درخواست پرداخت تطابق ندارد");

                string? transactionCode;
                string? message;
                if (invoice.IsAlreadyVerified)
                {
                    transactionCode = invoice.TransactionCode;
                    message = invoice.Message;
                }
                else
                {
                    if (invoice.Status != PaymentFetchResultStatus.ReadyForVerifying)
                    {
                        var failed = await _billingRep.SaveGatewayAttemptAsync(history.ID, false, null, invoice.Message);
                        if (!failed.Status) throw new InvalidOperationException(failed.ErrorMessage);
                        return await BuildPaymentVerificationResponseAsync(history.ID, false, null,
                            invoice.Message ?? "پرداخت قابل تایید نیست", browserCallback);
                    }
                    var verified = await _onlinePayment.VerifyAsync(invoice);
                    transactionCode = verified.TransactionCode;
                    message = verified.Message;
                    if (verified.Status != PaymentVerifyResultStatus.Succeed && verified.Status != PaymentVerifyResultStatus.AlreadyVerified)
                    {
                        var failed = await _billingRep.SaveGatewayAttemptAsync(history.ID, false, null, message);
                        if (!failed.Status) throw new InvalidOperationException(failed.ErrorMessage);
                        return await BuildPaymentVerificationResponseAsync(history.ID, false, null,
                            message ?? "پرداخت ناموفق بود", browserCallback);
                    }
                }
                var result = await _billingRep.ConfirmGatewayPaymentAsync(history.ID, transactionCode ?? "",
                    history.ID.ToString(), !string.IsNullOrWhiteSpace(Authority) ? Authority : paymentToken, message);

                result = await _bookingRep.UpdatePaymentBookingsStatus(history);

                return await BuildPaymentVerificationResponseAsync(history.ID, result.Status, transactionCode,
                    result.Status ? null : result.ErrorMessage, browserCallback);
            }
            catch (Exception ex)
            {
                if (PayId > 0)
                    return await BuildPaymentVerificationResponseAsync(PayId, false, null,
                        $"{ex.Message} - {ex.InnerException?.Message}", browserCallback);
                return BadRequest(new PaymentVerificationResultBody
                {
                    Status = false,
                    Success = false,
                    PaymentVerified = false,
                    ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}"
                });
            }
        }

       

      

        #region PaymentTools

      
        private static List<long> NormalizeBookingIds(AddEditPaymentRequestBody requestBody)
        {
            var bookingIds = requestBody.BookingIDs?
                .Where(x => x > 0)
                .ToList() ?? new List<long>();

            return bookingIds.Distinct().ToList();
        }


        private async Task<ActionResult<ListResultObject<PaymentVM>>> GetAllPaymentsInternal(GetPaymentListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var userId = User.GetCurrentUserId();
            var roleId = User.GetCurrentRoleId();
            long customerId = requestBody.CustomerId ?? 0;
            if (roleId != 4)
            {
                var dbcustomer = await _customerRep.ExistCustomerAsync(userId.ToString(), "personid");
                customerId = dbcustomer.Status ? dbcustomer.ID : 0;
            }

            var result = await _PaymentRep.GetAllPaymentsAsync(
                requestBody.BookingId,
                customerId,
                requestBody.PaymentIncludes,
                requestBody.PageIndex,
                requestBody.PageSize,
                requestBody.SearchText,
                requestBody.SortQuery,
                requestBody.PaymentStatus ?? "",
                requestBody.PaymentLevel,
                requestBody.FromDate,
                requestBody.ToDate,
                roleId == (long)BaseRole.Admin ? 0 : userId);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<PaymentVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }


        private async Task<ActionResult<RowResultObject<PaymentVM>>> GetPaymentByIdInternal(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _PaymentRep.GetPaymentByIdAsync(requestBody.ID);
            if (result.Status)
            {
                var resultVM = _mapper.Map<RowResultObject<PaymentVM>>(result);
                if (User.GetCurrentRoleId() != 4)
                {
                    if (!await _billingRep.CanAccessPaymentAsync(requestBody.ID, User.GetCurrentUserId(), false))
                    {
                        return Forbid();
                    }
                }
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        private async Task<ActionResult<BitResultObject>> DeletePaymentInternal(GetRowRequestBody requestBody)
        {
            if (!await _billingRep.CanAccessPaymentAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long)BaseRole.Admin)) return Forbid();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _PaymentRep.RemovePaymentAsync(requestBody.ID);
            if (result.Status)
            {

                #region AddLog

                Log log = new Log()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    LogTime = DateTime.Now.ToShamsi(),
                    ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),

                };
                await _logRep.AddLogAsync(log);

                #endregion

                return Ok(result);
            }
            return BadRequest(result);
        }

        private async Task<IActionResult> BuildPaymentVerificationResponseAsync(
           long paymentHistoryId,
           bool paymentVerified,
           string? transactionCode,
           string? errorMessage,
           bool browserCallback)
        {
            var context = await _billingRep.GetPaymentVerificationContextAsync(paymentHistoryId);
            if (!context.Status || context.Result == null)
            {
                var fallback = new PaymentVerificationResultBody
                {
                    Status = false,
                    Success = false,
                    PaymentVerified = false,
                    PaymentHistoryId = paymentHistoryId,
                    ErrorMessage = context.ErrorMessage ?? errorMessage ?? "اطلاعات پرداخت یافت نشد"
                };
                if (browserCallback)
                {
                    try { return Redirect(await BuildPaymentCallbackUrl(fallback)); }
                    catch { return Ok(fallback); }
                }
                return BadRequest(fallback);
            }

            var data = context.Result;
            var response = new PaymentVerificationResultBody
            {
                Status = paymentVerified,
                Success = paymentVerified,
                PaymentVerified = paymentVerified,
                PaymentHistoryId = data.PaymentHistoryID,
                PaymentId = data.PaymentID,
                Purpose = data.Purpose,
                ProfileType = data.ProfileType,
                BookingIds = data.BookingIDs,
                TransactionCode = transactionCode,
                ResultPath = GetPaymentResultPath(data.Purpose, data.ProfileType),
                ErrorMessage = paymentVerified ? null : errorMessage
            };

            if (browserCallback)
            {
                try { return Redirect(await BuildPaymentCallbackUrl(response)); }
                catch { return Ok(response); }
            }

            return Ok(response);
        }

        private static string? GetPaymentResultPath(string purpose, string? profileType)
        {
            if (!purpose.Equals(PaymentPurposes.Subscription, StringComparison.OrdinalIgnoreCase))
                return null;
            return profileType?.Equals("Workshop", StringComparison.OrdinalIgnoreCase) == true
                ? "/workshop/billing/payments/result"
                : profileType?.Equals("Worker", StringComparison.OrdinalIgnoreCase) == true
                    ? "/worker/billing/payments/result"
                    : null;
        }

        private async Task<string> BuildPaymentCallbackUrl(PaymentVerificationResultBody response)
        {
            var frontendCallbackUrlRow = await _settingRep.GetSettingRowAsync(0, "PaymentCallbackUrl");
            var frontendCallbackUrl = frontendCallbackUrlRow.Status && frontendCallbackUrlRow.Result?.IsActive == true
                ? frontendCallbackUrlRow.Result.Value?.Trim()
                : null;
            if (string.IsNullOrWhiteSpace(frontendCallbackUrl))
                throw new InvalidOperationException("مسیر بازگشت به برنامه تنظیم نشده یا غیرفعال است");

            if (!Uri.TryCreate(frontendCallbackUrl, UriKind.Absolute, out var callbackUri) ||
                (callbackUri.Scheme != Uri.UriSchemeHttps && callbackUri.Scheme != Uri.UriSchemeHttp))
                throw new InvalidOperationException("مسیر بازگشت به برنامه نامعتبر است");
            var query = new List<string>
            {
                $"status={Uri.EscapeDataString(response.Status ? "true" : "false")}",
                $"success={Uri.EscapeDataString(response.Status ? "true" : "false")}",
                $"paymentVerified={Uri.EscapeDataString(response.PaymentVerified ? "true" : "false")}",
                $"paymentHistoryId={response.PaymentHistoryId}",
                $"paymentId={response.PaymentId}",
                $"purpose={Uri.EscapeDataString(response.Purpose ?? "")}",
                $"profileType={Uri.EscapeDataString(response.ProfileType ?? "")}",
                $"bookingIds={Uri.EscapeDataString(string.Join(",", response.BookingIds))}",
                $"resultPath={Uri.EscapeDataString(response.ResultPath ?? "")}"
            };
            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
                query.Add($"errorMessage={Uri.EscapeDataString(response.ErrorMessage)}");
            var separator = string.IsNullOrEmpty(callbackUri.Query) ? "?" : "&";
            return $"{frontendCallbackUrl}{separator}{string.Join("&", query)}";
        }


        #endregion
    }
}
