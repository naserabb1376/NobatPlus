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
        IStylistServiceRep _stylistServiceRep;
        ILogRep _logRep;
        private readonly IOnlinePayment _onlinePayment;
        private readonly IMapper _mapper;

        public PaymentController(IPaymentRep PaymentRep,IPaymentHistoryRep paymentHistoryRep,ICustomerRep customerRep,IStylistServiceRep stylistServiceRep,ILogRep logRep, IOnlinePayment onlinePayment, IMapper mapper)
        {
           _PaymentRep = PaymentRep;
            _PaymentHistoryRep = paymentHistoryRep;
            _customerRep = customerRep;
            _stylistServiceRep = stylistServiceRep;
           _logRep = logRep;
            _onlinePayment = onlinePayment;
            _mapper = mapper;
        }

        [HttpPost("GetAllPayments")]
        public async Task<ActionResult<ListResultObject<PaymentVM>>> GetAllPayments(GetPaymentListRequestBody requestBody)
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
                if (!dbcustomer.Status)
                {
                    return Forbid();
                }
                customerId = dbcustomer.ID;
            }

            var result = await _PaymentRep.GetAllPaymentsAsync(requestBody.BookingId,customerId,requestBody.PaymentIncludes,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<PaymentVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("GetPaymentById")]
        public async Task<ActionResult<RowResultObject<PaymentVM>>> GetPaymentById(GetRowRequestBody requestBody)
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
                    var userId = User.GetCurrentUserId();
                    var dbcustomer = await _customerRep.ExistCustomerAsync(userId.ToString(), "personid");
                    if (!dbcustomer.Status || resultVM.Result?.CustomerID != dbcustomer.ID)
                    {
                        return Forbid();
                    }
                }
                return Ok(resultVM);
            }
            return BadRequest(result);
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

            var userId = User.GetCurrentUserId();
            var dbcustomer = await _customerRep.ExistCustomerAsync(userId.ToString(), "personid");
            if (!dbcustomer.Status)
            {
                return Forbid();
            }
            var customerId = dbcustomer.ID;
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
                DepositAmount = calcPayment.Result.DepositAmount,
                TotalServiceAmount = calcPayment.Result.TotalServiceAmount,
                PlarformAmount = calcPayment.Result.PlatformAmount,
                StylistAmount = calcPayment.Result.StylistAmount,
                AllPaymentAmount = calcPayment.Result.AllPaymentAmount,
                PayedAmount = calcPayment.Result.PayedAmount,
                RemainAmount = calcPayment.Result.RemainAmount,
                DiscountedServiceAmount = calcPayment.Result.DiscountedServiceAmount,
                VatAmount = calcPayment.Result.VatAmount,
                PaymentStatus = requestBody.PaymentStatus,
                PaymentDate = requestBody.PaymentDate ?? DateTime.Now.ToShamsi(),
                PaymentLevel = requestBody.PaymentLevel,
                PaymentFinished = requestBody.PaymentFinished,
                DiscountID = discountId,
                Description = requestBody.Description,
                PaymentBookings = bookingIds.Select(bookingId => new PaymentBooking
                {
                    BookingID = bookingId
                }).ToList(),
                PaymentDetails = calcPayment.Result.stylistServiceWithDiscountDtos.Select(d=> new PaymentDetail()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    Description = requestBody.Description,
                    BookingID = d.BookingID,
                    StylistID = d.StylistID,
                    ServiceManagementID = d.ServiceManagementID,
                    StylistServicePriceVariantID = d.StylistServicePriceVariantID,
                    AppliedOptionSummary = d.AppliedOptionSummary,
                    StylistServiceAmount = d.ServicePrice,
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

            foreach (var bookingId in bookingIds)
            {
                PaymentHistory paymentHistory = new PaymentHistory()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    BookingID = bookingId,
                    PaymentID = result.ID,
                    PaymentMethod = requestBody.PaymentLevel,
                    PaymentDate = requestBody.PaymentDate ?? DateTime.Now.ToShamsi(),
                    Amount = calcPayment.Result.PayedAmount,
                    PaymentStatus = requestBody.PaymentFinished,
                    GatewayName = requestBody.PaymentLevel == 1 ? "Cash" : null,
                    Description = requestBody.Description,
                };
                result = await _PaymentHistoryRep.AddPaymentHistoryAsync(paymentHistory);

                if (!result.Status)
                {
                    return BadRequest(result);
                }
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

            var userId = User.GetCurrentUserId();
            var dbcustomer = await _customerRep.ExistCustomerAsync(userId.ToString(), "personid");
            var customerId = dbcustomer.ID;
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
                DepositAmount = calcPayment.Result.DepositAmount,
                TotalServiceAmount = calcPayment.Result.TotalServiceAmount,
                PlarformAmount = calcPayment.Result.PlatformAmount,
                StylistAmount = calcPayment.Result.StylistAmount,
                AllPaymentAmount = calcPayment.Result.AllPaymentAmount,
                PayedAmount = calcPayment.Result.PayedAmount,
                RemainAmount = calcPayment.Result.RemainAmount,
                DiscountedServiceAmount = calcPayment.Result.DiscountedServiceAmount,
                VatAmount = calcPayment.Result.VatAmount,
                PaymentStatus = requestBody.PaymentStatus,
                PaymentDate = requestBody.PaymentDate ?? DateTime.Now.ToShamsi(),
                PaymentLevel = requestBody.PaymentLevel,
                PaymentFinished = requestBody.PaymentFinished,
                DiscountID = discountId,
                Description = requestBody.Description,
                PaymentBookings = bookingIds.Select(bookingId => new PaymentBooking
                {
                    PaymentID = requestBody.ID,
                    BookingID = bookingId
                }).ToList(),
                PaymentDetails = calcPayment.Result.stylistServiceWithDiscountDtos.Select(d => new PaymentDetail()
                {
                    CreateDate = theRow.Result.CreateDate,
                    UpdateDate = DateTime.Now.ToShamsi(),
                    Description = requestBody.Description,
                    PaymentID = requestBody.ID,
                    BookingID = d.BookingID,
                    StylistID = d.StylistID,
                    ServiceManagementID = d.ServiceManagementID,
                    StylistServicePriceVariantID = d.StylistServicePriceVariantID,
                    AppliedOptionSummary = d.AppliedOptionSummary,
                    StylistServiceAmount = d.ServicePrice,
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

        [HttpPost("RequestPayment")]
        public async Task<ActionResult<RowResultObject<RequestPaymentResultBody>>> RequestPayment(RequestPaymentRequestBody requestBody)
        {
            var result = new RowResultObject<RequestPaymentResultBody>
            {
                Result = new RequestPaymentResultBody()
            };

            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var paymentRow = await _PaymentRep.GetPaymentByIdAsync(requestBody.PaymentID);
            if (!paymentRow.Status || paymentRow.Result == null)
            {
                result.Status = false;
                result.Result = null;
                result.ErrorMessage = "پرداخت مورد نظر یافت نشد";
                return BadRequest(result);
            }

            var payment = paymentRow.Result;
            if (!await CanCurrentUserAccessPaymentAsync(payment))
            {
                return Forbid();
            }

            if (payment.PaymentFinished)
            {
                result.Status = false;
                result.Result = null;
                result.ErrorMessage = "این پرداخت قبلا تکمیل شده است";
                return BadRequest(result);
            }

            var amount = GetOnlinePayableAmount(payment);
            if (amount <= 0)
            {
                result.Status = false;
                result.Result = null;
                result.ErrorMessage = "مبلغ قابل پرداخت معتبر نیست";
                return BadRequest(result);
            }

            var bookingId = payment.PaymentBookings?.FirstOrDefault()?.BookingID ?? 0;
            if (bookingId <= 0)
            {
                result.Status = false;
                result.Result = null;
                result.ErrorMessage = "رزرو مرتبط با پرداخت مشخص نیست";
                return BadRequest(result);
            }
            var activePayGateway = ToolBox.GetActivePayGateway();

            var paymentHistory = new PaymentHistory
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                BookingID = bookingId,
                PaymentID = payment.ID,
                PaymentDate = DateTime.Now.ToShamsi(),
                PaymentMethod = payment.PaymentLevel,
                Amount = amount,
                PaymentStatus = false,
                GatewayName = activePayGateway.GatewayName,
                Description = $"Online payment request for payment #{payment.ID}"
            };

            var addResult = await _PaymentHistoryRep.AddPaymentHistoryAsync(paymentHistory);
            if (!addResult.Status)
            {
                result.Status = false;
                result.Result = null;
                result.ErrorMessage = addResult.ErrorMessage;
                return BadRequest(result);
            }

            var callbackUrl = Url.Action(nameof(VerifyPayment), "Payment", new { PayId = addResult.ID }, Request.Scheme);
            var zarInvoice = new ZarinPalInvoice(description: $"{addResult.ID} - Payment {payment.ID}");
            var invoice = await _onlinePayment.RequestAsync(invoice =>
            {
                if (activePayGateway.GatewayName.ToLower() == "zarinpal")
                {
                    invoice.SetCallbackUrl(callbackUrl)
                     .SetAmount(amount)
                     .SetZarinPalData(zarInvoice)
                     .UseZarinPal();
                }
                else
                {
                    invoice.SetCallbackUrl(callbackUrl)
                     .SetAmount(amount)
                     .SetGateway(activePayGateway.GatewayName);
                }

                invoice.SetTrackingNumber(addResult.ID);
            });

            if (invoice.IsSucceed)
            {
                result.Result.PayGatewayUrl = invoice.GatewayTransporter.Descriptor.Url;
                result.Result.PaymentHistoryID = addResult.ID;
                result.Result.Amount = amount;
                result.ErrorMessage = "";

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

            var failedHistory = new PaymentHistory
            {
                ID = addResult.ID,
                CreateDate = paymentHistory.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                BookingID = bookingId,
                PaymentID = payment.ID,
                PaymentDate = paymentHistory.PaymentDate,
                PaymentMethod = payment.PaymentLevel,
                Amount = amount,
                PaymentStatus = false,
                GatewayName = activePayGateway.GatewayName,
                GatewayMessage = invoice.Message,
                Description = paymentHistory.Description
            };
            await _PaymentHistoryRep.EditPaymentHistoryAsync(failedHistory);



            result.Status = false;
            result.Result = null;
            result.ErrorMessage = invoice.Message;
            return BadRequest(result);
        }

        [AllowAnonymous]
        [HttpGet("VerifyPayment")]
        public async Task<ActionResult<BitResultObject>> VerifyPayment(long PayId = 0, string? paymentToken = "", string? Authority = "", string? Status = "")
        {
            var result = new BitResultObject();

            if (PayId <= 0)
            {
                result.Status = false;
                result.ErrorMessage = "شناسه پرداخت معتبر نیست";
                return BadRequest(result);
            }

            var paymentHistoryRow = await _PaymentHistoryRep.GetPaymentHistoryByIdAsync(PayId);
            if (!paymentHistoryRow.Status || paymentHistoryRow.Result == null)
            {
                result.Status = false;
                result.ErrorMessage = "سابقه پرداخت یافت نشد";
                return BadRequest(result);
            }

            var paymentHistory = paymentHistoryRow.Result;
            var invoice = await _onlinePayment.FetchAsync();

            if (invoice.Status != PaymentFetchResultStatus.ReadyForVerifying)
            {
                await SaveGatewayResultAsync(paymentHistory, false, null, PayId.ToString(), paymentToken, Authority, invoice.Message);
                result.Status = false;
                result.ID = paymentHistory.ID;
                result.ErrorMessage = string.IsNullOrWhiteSpace(invoice.Message) ? "پرداخت قابل تایید نیست" : invoice.Message;
                return Ok(result);
            }

            var verifyResult = await _onlinePayment.VerifyAsync(invoice);
            var transactionCode = verifyResult.TransactionCode?.ToString();
            var trackingNumber = verifyResult.TrackingNumber > 0 ? verifyResult.TrackingNumber.ToString() : PayId.ToString();
            var token = !string.IsNullOrWhiteSpace(Authority) ? Authority : paymentToken;

            if (verifyResult.Status == PaymentVerifyResultStatus.Succeed)
            {
                await SaveGatewayResultAsync(paymentHistory, true, transactionCode, trackingNumber, token, Authority, verifyResult.Message);

                var paymentRow = await _PaymentRep.GetPaymentByIdAsync(paymentHistory.PaymentID);
                if (paymentRow.Status && paymentRow.Result != null)
                {
                    var payment = paymentRow.Result;
                    var paidAmount = payment.PayedAmount > 0 ? payment.PayedAmount : paymentHistory.Amount;
                    var updatedPayment = new Payment
                    {
                        ID = payment.ID,
                        CreateDate = payment.CreateDate,
                        UpdateDate = DateTime.Now.ToShamsi(),
                        DiscountID = payment.DiscountID,
                        AllPaymentAmount = payment.AllPaymentAmount,
                        DepositAmount = payment.DepositAmount,
                        PayedAmount = payment.PayedAmount + paidAmount,
                        RemainAmount = Math.Max(0, payment.AllPaymentAmount - paidAmount),
                        TotalServiceAmount = payment.TotalServiceAmount,
                        DiscountedServiceAmount = payment.DiscountedServiceAmount,
                        StylistAmount = payment.StylistAmount,
                        PlarformAmount = payment.PlarformAmount,
                        VatAmount = payment.VatAmount,
                        PaymentDate = DateTime.Now.ToShamsi(),
                        PaymentStatus = "paid",
                        PaymentLevel = payment.PaymentLevel,
                        Description = payment.Description
                    };

                    updatedPayment.PaymentFinished = updatedPayment.AllPaymentAmount == 0;

                    await _PaymentRep.EditPaymentAsync(updatedPayment);
                }


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

                result.Status = true;
                result.ID = paymentHistory.ID;
                result.ErrorMessage = "پرداخت موفقیت آمیز بود";
                return Ok(result);
            }

            await SaveGatewayResultAsync(paymentHistory, false, transactionCode, trackingNumber, token, Authority, verifyResult.Message);


          


            result.Status = false;
            result.ID = paymentHistory.ID;
            result.ErrorMessage = string.IsNullOrWhiteSpace(verifyResult.Message) ? "پرداخت ناموفق بود" : verifyResult.Message;
            return Ok(result);
        }

        #region PaymentTools
        private async Task<bool> CanCurrentUserAccessPaymentAsync(Payment payment)
        {
            if (User.GetCurrentRoleId() == (long) BaseRole.Admin)
            {
                return true;
            }

            var userId = User.GetCurrentUserId();
            var dbcustomer = await _customerRep.ExistCustomerAsync(userId.ToString(), "personid");
            if (!dbcustomer.Status)
            {
                return false;
            }

            return payment.PaymentBookings?.Any(x => x.Booking?.CustomerID == dbcustomer.ID) ?? false;
        }

        private static decimal GetOnlinePayableAmount(Payment payment)
        {
            return payment.PayedAmount > 0 ? payment.RemainAmount : payment.DepositAmount;
        }

        private async Task SaveGatewayResultAsync(PaymentHistory source, bool paymentStatus, string? transactionCode, string? trackingNumber, string? paymentToken, string? authority, string? message)
        {
            var paymentHistory = new PaymentHistory
            {
                ID = source.ID,
                CreateDate = source.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                BookingID = source.BookingID,
                PaymentID = source.PaymentID,
                PaymentDate = DateTime.Now.ToShamsi(),
                PaymentMethod = source.PaymentMethod,
                Amount = source.Amount,
                PaymentStatus = paymentStatus,
                TransactionCode = transactionCode,
                TrackingNumber = trackingNumber,
                GatewayName = source.GatewayName ?? "ZarinPal",
                GatewayMessage = message,
                PaymentToken = !string.IsNullOrWhiteSpace(paymentToken) ? paymentToken : authority,
                Description = source.Description
            };

            await _PaymentHistoryRep.EditPaymentHistoryAsync(paymentHistory);
        }

      
        private static List<long> NormalizeBookingIds(AddEditPaymentRequestBody requestBody)
        {
            var bookingIds = requestBody.BookingIDs?
                .Where(x => x > 0)
                .ToList() ?? new List<long>();

            return bookingIds.Distinct().ToList();
        }

        #endregion
    }
}
