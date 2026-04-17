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
        private readonly IMapper _mapper;

        public PaymentController(IPaymentRep PaymentRep,IPaymentHistoryRep paymentHistoryRep,ICustomerRep customerRep,IStylistServiceRep stylistServiceRep,ILogRep logRep, IMapper mapper)
        {
           _PaymentRep = PaymentRep;
            _PaymentHistoryRep = paymentHistoryRep;
            _customerRep = customerRep;
            _stylistServiceRep = stylistServiceRep;
           _logRep = logRep;
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
            var dbcustomer = await _customerRep.ExistCustomerAsync(userId.ToString(), "personid");
            var customerId = requestBody.CustomerId ?? dbcustomer.ID;

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
            var customerId = dbcustomer.ID;
            var discountId = requestBody.DiscountID ?? 0;


            var calcPayment = await _PaymentRep.CalculatePaymentAsync(customerId,requestBody.BookingID,discountId);

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
                BookingID = requestBody.BookingID,
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
                PaymentDetails = calcPayment.Result.stylistServiceWithDiscountDtos.Select(d=> new PaymentDetail()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    Description = requestBody.Description,
                    StylistID = d.StylistID,
                    ServiceManagementID = d.ServiceManagementID,
                    StylistServiceAmount = d.ServicePrice,
                    DiscountAmount = d.PriceAfterDiscount,
                    DiscountPercent = d.DiscountPercent,
                }).ToList()
            };

            result = await _PaymentRep.AddPaymentAsync(Payment);

            PaymentHistory paymentHistory = new PaymentHistory()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                BookingID = requestBody.BookingID,
                PaymentID = result.ID,
                PaymentMethod = requestBody.PaymentLevel,
                PaymentDate = requestBody.PaymentDate ?? DateTime.Now.ToShamsi(),
                Description = requestBody.Description,
                
            };
            result = await _PaymentHistoryRep.AddPaymentHistoryAsync(paymentHistory);

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

            var calcPayment = await _PaymentRep.CalculatePaymentAsync(customerId, requestBody.BookingID, discountId);

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
                BookingID = requestBody.BookingID,
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
                //PaymentDetails = calcPayment.Result.stylistServiceWithDiscountDtos.Select(d => new PaymentDetail()
                //{
                //    CreateDate = DateTime.Now.ToShamsi(),
                //    UpdateDate = DateTime.Now.ToShamsi(),
                //    Description = requestBody.Description,
                //    StylistID = d.StylistID,
                //    ServiceManagementID = d.ServiceManagementID,
                //    StylistServiceAmount = d.ServicePrice,
                //    DiscountAmount = d.PriceAfterDiscount,

                //}).ToList()
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
            RowResultObject<RequestPaymentResultBody> result = new RowResultObject<RequestPaymentResultBody>();
            result.Result = new RequestPaymentResultBody();
            decimal rowAmount = 0, discountedrowAmount = 0;

            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var UserId = User.GetCurrentUserId();

            switch (requestBody.EntityType.ToLower())
            {
                default:
                case "group":
                    {
                        var theRow = await _GroupRep.GetGroupByIdAsync(requestBody.ForeignKeyId);

                        if (theRow.Result == null)
                        {
                            result.Result = null;
                            result.Status = false;
                            result.ErrorMessage = "درخواست نامعتبر است";
                            return BadRequest(result);
                        }

                        rowAmount = theRow.Result.Fee;
                        discountedrowAmount = theRow.Result.DiscountedFee;

                    }
                    break;
                case "event":
                    {
                        var theRow = await _EventRep.GetEventByIdAsync(requestBody.ForeignKeyId);

                        if (theRow.Result == null)
                        {
                            result.Result = null;
                            result.Status = false;
                            result.ErrorMessage = "درخواست نامعتبر است";
                            return BadRequest(result);
                        }

                        rowAmount = theRow.Result.Fee ?? 0;
                        discountedrowAmount = theRow.Result.DiscountedFee ?? 0;


                    }
                    break;
            }

            if (discountedrowAmount == rowAmount && requestBody.DiscountId > 0)
            {
                var discount = await _discountRep.GetDiscountByIdAsync(requestBody.DiscountId.Value);
                if (discount.Result.IsActive && discount.Result.EntityName.ToLower() == requestBody.EntityType.ToLower() && discount.Result.ForeignKeyId == requestBody.ForeignKeyId && DateTime.Now.ToShamsi() >= discount.Result.ExpireDate)
                {
                    discountedrowAmount = rowAmount - (rowAmount * discount.Result.DiscountPercent / 100m);
                }
            }

            rowAmount = discountedrowAmount;

            if (rowAmount > 0)
            {
                PaymentHistory PaymentHistory = new PaymentHistory()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    ForeignKeyId = requestBody.ForeignKeyId,
                    EntityType = requestBody.EntityType,
                    Amount = rowAmount,
                    UserId = UserId,
                    PaymentDate = DateTime.Now.ToShamsi(),
                    PaymentStatus = false,

                    //  Description = requestBody.Description,
                };
                var Addresult = await _PaymentHistoryRep.AddPaymentHistoryAsync(PaymentHistory);
                if (Addresult.Status)
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


                    #region PaymentGateway

                    var zarInvoice = new ZarinPalInvoice(description: $"{Addresult.ID} - {PaymentHistory.PaymentDate}");
                    // var callbackUrl = Url.Action("VerifyPayment", "PaymentHistory", new { payId = Addresult.ID }, Request.Scheme);
                    var callbackUrl = $"https://aitechac.com/payment/verify?PayId={Addresult.ID}&UserId={UserId}&EntityType={requestBody.EntityType}&ForeignKeyId={requestBody.ForeignKeyId}";

                    var invoice = await _onlinePayment.RequestAsync(invoice =>
                    {
                        invoice.SetCallbackUrl(callbackUrl)
                               .SetAmount(rowAmount)
                                .SetZarinPalData(zarInvoice)
                                .UseZarinPal();

                        invoice.UseAutoIncrementTrackingNumber();

                    });

                    if (invoice.IsSucceed)
                    {
                        result.Result.PayGatewayUrl = invoice.GatewayTransporter.Descriptor.Url;
                        result.ErrorMessage = "";
                    }



                    else
                    {
                        result.ErrorMessage = invoice.Message;
                        result.Result.PayGatewayUrl = "";
                    }

                    #endregion



                    return Ok(result);
                }
            }

            else
            {
                var userRow = await _UserRep.GetUserByIdAsync(UserId);


                PreRegistration PreRegistration = new PreRegistration()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    Email = userRow.Result.Email,
                    FirstName = userRow.Result.FirstName,
                    LastName = userRow.Result.LastName,
                    PhoneNumber = userRow.Result.Username,

                    EducationalClass = null,
                    SchoolName = null,
                    FavoriteField = null,
                    RecognitionLevel = null,
                    ProgrammingSkillLevel = null,

                    ForeignKeyId = requestBody.ForeignKeyId,
                    EntityType = requestBody.EntityType ?? "",
                    RegistrationDate = DateTime.Now.ToShamsi(),
                    OtherLangs = null,
                    // Description = requestBody.Description,
                };
                var addPreRegistrationResult = await _PreRegistrationRep.AddPreRegistrationAsync(PreRegistration);

                if (addPreRegistrationResult.Status)
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

                }
                else
                {
                    return BadRequest(addPreRegistrationResult);
                }
            }

            return BadRequest(result);
        }

        [HttpGet("VerifyPayment")]
        public async Task<ActionResult<BitResultObject>> VerifyPayment(long PayId = 0, string? EntityType = "", long? ForeignKeyId = 0, string? paymentToken = "", string? Authority = "", string? Status = "")
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var paymentHistory = await _PaymentHistoryRep.GetPaymentHistoryByIdAsync(PayId);

                var UserId = User.GetCurrentUserId();

                var userRow = await _UserRep.GetUserByIdAsync(UserId);

                var invoice = await _onlinePayment.FetchAsync();

                // Check if the invoice is new or it's already processed before.
                if (invoice.Status != PaymentFetchResultStatus.ReadyForVerifying)
                {
                    // You can also see if the invoice is already verified before.
                    paymentHistory.Result.PaymentStatus = false;
                }

                var verifyResult = await _onlinePayment.VerifyAsync(invoice);

                var originalResult = verifyResult.GetZarinPalOriginalVerificationResult();

                // Note: Save the verifyResult.TransactionCode in your database.

                if (verifyResult.Status == PaymentVerifyResultStatus.Succeed)
                {
                    paymentHistory.Result.PaymentStatus = true;

                    PreRegistration PreRegistration = new PreRegistration()
                    {
                        CreateDate = DateTime.Now.ToShamsi(),
                        UpdateDate = DateTime.Now.ToShamsi(),
                        Email = userRow.Result.Email,
                        FirstName = userRow.Result.FirstName,
                        LastName = userRow.Result.LastName,
                        PhoneNumber = userRow.Result.Username,

                        EducationalClass = null,
                        SchoolName = null,
                        FavoriteField = null,
                        RecognitionLevel = null,
                        ProgrammingSkillLevel = null,

                        ForeignKeyId = ForeignKeyId ?? 0,
                        EntityType = EntityType ?? "",
                        RegistrationDate = DateTime.Now.ToShamsi(),
                        OtherLangs = null,
                        // Description = requestBody.Description,
                    };
                    var addPreRegistrationResult = await _PreRegistrationRep.AddPreRegistrationAsync(PreRegistration);

                    if (addPreRegistrationResult.Status)
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

                    }
                }

                else
                {
                    paymentHistory.Result.PaymentStatus = false;
                }

                var saveResult = await _PaymentHistoryRep.EditPaymentHistoryAsync(paymentHistory.Result);

                if (saveResult.Status)
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

                }

                if (paymentHistory.Result.PaymentStatus)
                {
                    result.Status = true;
                    result.ErrorMessage = $"پرداخت موفقیت آمیز بود";
                    result.ID = paymentHistory.Result.ID;
                    return Ok(result);
                }

                else
                {
                    result.Status = false;
                    result.ErrorMessage = $"پرداخت ناموفق بود";
                    result.ID = paymentHistory.Result.ID;
                    return Ok(result);
                }

            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
                return BadRequest(result);
            }

        }
    }
}
