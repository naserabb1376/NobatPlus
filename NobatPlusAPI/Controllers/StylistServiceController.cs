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
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Models.StylistService;
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
    [Route("StylistService")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class StylistServiceController : ControllerBase
    {
        IStylistServiceRep _StylistServiceRep;
        IServiceManagementRep _serviceManagementRep;
        ICustomerRep _customerRep;
        ILogRep _logRep;
        private readonly IMapper _mapper;


        public StylistServiceController(IStylistServiceRep StylistServiceRep,IServiceManagementRep serviceManagementRep,ICustomerRep customerRep,ILogRep logRep, IMapper mapper)
        {
           _StylistServiceRep = StylistServiceRep;
            _customerRep = customerRep;
            _serviceManagementRep = serviceManagementRep;
            _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllStylistServices_Base")]
        [AllowAnonymous]
        public async Task<ActionResult<ListResultObject<StylistServiceVM>>> GetAllStylistServices_Base(GetStylistServiceListRequestBody requestBody)
        {
            var result = new ListResultObject<StylistServiceWithDiscountDto>();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var userId = User.GetCurrentUserId();
            var dbcustomer = await _customerRep.ExistCustomerAsync(userId.ToString(), "personid");
            var customerId = requestBody.CustomerID ?? dbcustomer.ID;
            var discountId = requestBody.DiscountID ?? 0;

            result = await _StylistServiceRep.GetAllStylistServicesAsync(requestBody.StylistID,requestBody.ServiceID,customerId,requestBody.BookingID,discountId,requestBody.OptionValueIDs,requestBody.OnlyLeafServices,requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<StylistServiceVM>>(result);
                return Ok(resultVM);
            }

            return BadRequest(result);
        }


        [HttpPost("GetStylistServiceById_Base")]
        public async Task<ActionResult<RowResultObject<StylistServiceVM>>> GetStylistServiceById_Base(GetStylistServiceRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var userId = User.GetCurrentUserId();
            var dbcustomer = await _customerRep.ExistCustomerAsync(userId.ToString(), "personid");
            var customerId = requestBody.CustomerID ?? dbcustomer.ID;
            var discountId = requestBody.DiscountID ?? 0;


            var result = await _StylistServiceRep.GetStylistServiceByIdAsync(requestBody.StylistID,requestBody.ServiceID,customerId,discountId,requestBody.OptionValueIDs);
            if (result.Status)
            {
                var resultVM = _mapper.Map<RowResultObject<StylistServiceVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistStylistService_Base")]
        public async Task<ActionResult<BitResultObject>> ExistStylistService_Base(GetStylistServiceRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _StylistServiceRep.ExistStylistServiceAsync(requestBody.StylistID, requestBody.ServiceID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddStylistServices_Base")]
        public async Task<ActionResult<BitResultObject>> AddStylistServices_Base(List<AddEditStylistServiceRequestBody> requestBodyList)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBodyList);
            }
            foreach (var item in requestBodyList)
            {
                var theService = await _serviceManagementRep.GetServiceManagementByIdAsync(item.ServiceID);
                if (theService.Result == null)
                {
                    result.Status = false;
                    result.ErrorMessage = $"خدمات وارد شده نامعتبر است";
                    return BadRequest(result);

                }
                var validrecord = await _StylistServiceRep.ExistStylistServiceAsync(item.StylistID,item.ServiceID);

                if (validrecord.Status && !item.HasDynamicPricing)
                {
                    result.Status = !validrecord.Status;
                    result.ErrorMessage = $"برای خدمت {theService.Result.ServiceName} با قیمت ثابت یا متغیر قبلا اطلاعات ثبت کرده اید امکان ثبت برای یک خدمت هم با قیمت ثابت هم متغیر وجود ندارد";
                    return BadRequest(result);
                }
            }
         


            var stylistServices = requestBodyList.Select(requestBody => new StylistService()
            {
                StylistID = requestBody.StylistID,
                ServiceManagementID = requestBody.ServiceID,
                DepositPercent = requestBody.DepositPercent,
                ServiceDuration = requestBody.Duration,
                ServicePrice = requestBody.ServicePrice,
                HasDynamicPricing = requestBody.HasDynamicPricing,
                PriceVariants = BuildPriceVariants(requestBody)
            }).ToList();

            result = await _StylistServiceRep.AddStylistServicesAsync(stylistServices);

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


        [HttpPut("EditStylistServices_Base")]
        public async Task<ActionResult<BitResultObject>> EditStylistServices_Base(List<AddEditStylistServiceRequestBody> requestBodyList)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBodyList);
            }

            var stylistServices = requestBodyList.Select(requestBody => new StylistService()
            {
                StylistID = requestBody.StylistID,
                ServiceManagementID = requestBody.ServiceID,
                ServiceDuration = requestBody.Duration,
                DepositPercent = requestBody.DepositPercent,
                ServicePrice = requestBody.ServicePrice,
                HasDynamicPricing = requestBody.HasDynamicPricing,
                PriceVariants = BuildPriceVariants(requestBody)
            }).ToList();

            var result = await _StylistServiceRep.EditStylistServicesAsync(stylistServices);

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


        [HttpDelete("DeleteStylistServices_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteStylistServices_Base(List<DeleteStylistServiceRowRequestBody> requestBodyList)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBodyList);
            }

            var stylistServiceIds = requestBodyList
                .Select(requestBody => (requestBody.StylistID, requestBody.ServiceID, requestBody.StylistServicePriceVariantID))
                .ToList();

            var result = await _StylistServiceRep.RemoveStylistServicesAsync(stylistServiceIds);

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

        private static List<StylistServicePriceVariant> BuildPriceVariants(AddEditStylistServiceRequestBody requestBody)
        {
            return requestBody.PriceVariants?
                .Where(x => x.OptionValueIDs != null && x.OptionValueIDs.Any())
                .Select(x => new StylistServicePriceVariant
                {
                    ID = x.ID,
                    StylistID = requestBody.StylistID,
                    ServiceManagementID = requestBody.ServiceID,
                    Price = x.Price,
                    DepositPercent = x.DepositPercent,
                    Duration = x.Duration,
                    IsActive = x.IsActive,
                    OptionValueCombinationKey = StylistServicePriceVariant.BuildOptionValueCombinationKey(x.OptionValueIDs),
                    OptionValues = x.OptionValueIDs
                        .Where(optionValueId => optionValueId > 0)
                        .Distinct()
                        .Select(optionValueId => new StylistServicePriceVariantOptionValue
                        {
                            ServiceOptionValueID = optionValueId
                        })
                        .ToList()
                })
                .ToList() ?? new List<StylistServicePriceVariant>();
        }

    }
}
