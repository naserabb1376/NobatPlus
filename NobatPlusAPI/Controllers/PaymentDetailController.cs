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
        ILogRep _logRep;
        private readonly IMapper _mapper;


        public PaymentDetailController(IPaymentDetailRep PaymentDetailRep,ILogRep logRep, IMapper mapper)
        {
           _PaymentDetailRep = PaymentDetailRep;
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
                var resultVM = _mapper.Map<RowResultObject<PaymentDetailVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistPaymentDetail_Base")]
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
    }
}
