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
using NobatPlusAPI.Models.PaymentDetail;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NobatPlusAPI.Controllers
{
    [Route("PaymentDetail")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class PaymentDetailController : ControllerBase
    {
        IPaymentDetailRep _PaymentDetailRep;
        IPaymentRep _PaymentRep;
        ICustomerRep _CustomerRep;
        IStylistRep _StylistRep;
        ILogRep _logRep;
        private readonly IMapper _mapper;


        public PaymentDetailController(IPaymentDetailRep PaymentDetailRep,IPaymentRep paymentRep,ICustomerRep customerRep,IStylistRep stylistRep,ILogRep logRep, IMapper mapper)
        {
           _PaymentDetailRep = PaymentDetailRep;
           _PaymentRep = paymentRep;
           _CustomerRep = customerRep;
           _StylistRep = stylistRep;
           _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllPaymentHistories_Base")]
        public async Task<ActionResult<ListResultObject<PaymentDetailVM>>> GetAllPaymentDetails_Base(GetPaymentDetailListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            if (!await NormalizePaymentDetailScopeAsync(requestBody))
            {
                return Forbid();
            }
            var result = await _PaymentDetailRep.GetAllPaymentDetailsAsync(requestBody.StylistId,requestBody.ServiceId,requestBody.PaymentId,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<PaymentDetailVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("GetPaymentDetailById_Base")]
        public async Task<ActionResult<RowResultObject<PaymentDetailVM>>> GetPaymentDetailById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _PaymentDetailRep.GetPaymentDetailByIdAsync(requestBody.ID);
            if (result.Status)
            {
                if (!await CanAccessPaymentDetailAsync(result.Result))
                {
                    return Forbid();
                }
                var resultVM = _mapper.Map<RowResultObject<PaymentDetailVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistPaymentDetail_Base")]
        [RequireRole(4)]
        public async Task<ActionResult<BitResultObject>> ExistPaymentDetail_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _PaymentDetailRep.ExistPaymentDetailAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddPaymentDetail_Base")]
        [RequireRole(4)]
        public async Task<ActionResult<BitResultObject>> AddPaymentDetail_Base(AddEditPaymentDetailRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            PaymentDetail PaymentDetail = new PaymentDetail()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                PaymentID = requestBody.PaymentID,
                StylistID = requestBody.StylistID,
                ServiceManagementID = requestBody.ServiceManagemntID,
                DiscountPercent = requestBody.DiscountPercent,
                DiscountAmount = requestBody.DiscountAmount,
                StylistServiceAmount = requestBody.StylistServiceAmount,
                Description = requestBody.Description,
            };
            var result = await _PaymentDetailRep.AddPaymentDetailAsync(PaymentDetail);
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

        [HttpPut("EditPaymentDetail_Base")]
        [RequireRole(4)]
        public async Task<ActionResult<BitResultObject>> EditPaymentDetail_Base(AddEditPaymentDetailRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var theRow = await _PaymentDetailRep.GetPaymentDetailByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            PaymentDetail PaymentDetail = new PaymentDetail()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ID = requestBody.ID,
                PaymentID = requestBody.PaymentID,
                StylistID = requestBody.StylistID,
                ServiceManagementID = requestBody.ServiceManagemntID,
                DiscountPercent = requestBody.DiscountPercent,
                DiscountAmount = requestBody.DiscountAmount,
                StylistServiceAmount = requestBody.StylistServiceAmount,
                Description = requestBody.Description,
            };
            result = await _PaymentDetailRep.EditPaymentDetailAsync(PaymentDetail);
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

        [HttpDelete("DeletePaymentDetail_Base")]
        [RequireRole(4)]
        public async Task<ActionResult<BitResultObject>> DeletePaymentDetail_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _PaymentDetailRep.RemovePaymentDetailAsync(requestBody.ID);
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

        private async Task<bool> NormalizePaymentDetailScopeAsync(GetPaymentDetailListRequestBody requestBody)
        {
            var roleId = User.GetCurrentRoleId();
            var personId = User.GetCurrentUserId();

            if (roleId == 4) return true;

            if (roleId == 1)
            {
                if (requestBody.PaymentId <= 0) return false;
                var customer = await _CustomerRep.ExistCustomerAsync(personId.ToString(), "personid");
                if (!customer.Status) return false;
                var payment = await _PaymentRep.GetPaymentByIdAsync(requestBody.PaymentId);
                return payment.Status && payment.Result != null && payment.Result.Booking.CustomerID == customer.ID;
            }

            if (roleId == 2)
            {
                var stylist = await _StylistRep.ExistStylistAsync(personId.ToString(), "personid");
                if (!stylist.Status) return false;
                requestBody.StylistId = stylist.ID;
                return true;
            }

            if (roleId == 3)
            {
                var salon = await _StylistRep.ExistStylistAsync(personId.ToString(), "personid");
                if (!salon.Status) return false;
                if (requestBody.StylistId <= 0)
                {
                    requestBody.StylistId = salon.ID;
                    return true;
                }
                var stylist = await _StylistRep.GetStylistByIdAsync(requestBody.StylistId);
                return stylist.Status && stylist.Result != null && stylist.Result.StylistParentID == salon.ID;
            }

            return false;
        }

        private async Task<bool> CanAccessPaymentDetailAsync(PaymentDetail detail)
        {
            if (detail == null) return false;

            var roleId = User.GetCurrentRoleId();
            var personId = User.GetCurrentUserId();

            if (roleId == 4) return true;

            if (roleId == 1)
            {
                var customer = await _CustomerRep.ExistCustomerAsync(personId.ToString(), "personid");
                if (!customer.Status) return false;
                var payment = await _PaymentRep.GetPaymentByIdAsync(detail.PaymentID);
                return payment.Status && payment.Result != null && payment.Result.Booking.CustomerID == customer.ID;
            }

            if (roleId == 2)
            {
                var stylist = await _StylistRep.ExistStylistAsync(personId.ToString(), "personid");
                return stylist.Status && detail.StylistID == stylist.ID;
            }

            if (roleId == 3)
            {
                var salon = await _StylistRep.ExistStylistAsync(personId.ToString(), "personid");
                if (!salon.Status) return false;
                if (detail.StylistID == salon.ID) return true;
                var stylist = await _StylistRep.GetStylistByIdAsync(detail.StylistID);
                return stylist.Status && stylist.Result != null && stylist.Result.StylistParentID == salon.ID;
            }

            return false;
        }
    }
}
