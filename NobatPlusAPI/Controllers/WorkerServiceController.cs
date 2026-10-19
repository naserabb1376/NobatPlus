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
using NobatPlusAPI.Models.WorkerService;
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
    [Route("WorkerService")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class WorkerServiceController : ControllerBase
    {
        IWorkerServiceRep _WorkerServiceRep;
        IServiceManagementRep _serviceManagementRep;
        ICustomerRep _customerRep;
        ILogRep _logRep;
        private readonly IMapper _mapper;


        public WorkerServiceController(IWorkerServiceRep WorkerServiceRep,IServiceManagementRep serviceManagementRep,ICustomerRep customerRep,ILogRep logRep, IMapper mapper)
        {
           _WorkerServiceRep = WorkerServiceRep;
            _customerRep = customerRep;
            _serviceManagementRep = serviceManagementRep;
            _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllWorkerServices_Base")]
        [AllowAnonymous]
        public async Task<ActionResult<ListResultObject<WorkerServiceVM>>> GetAllWorkerServices_Base(GetWorkerServiceListRequestBody requestBody)
        {
            var result = new ListResultObject<WorkerServiceWithDiscountDto>();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var userId = User.GetCurrentUserId();
            var dbcustomer = await _customerRep.ExistCustomerAsync(userId.ToString(), "personid");
            var customerId = requestBody.CustomerID ?? dbcustomer.ID;
            var discountId = requestBody.DiscountID ?? 0;

            result = await _WorkerServiceRep.GetAllWorkerServicesAsync(requestBody.WorkerID,requestBody.ServiceID,customerId,requestBody.BookingID,discountId,requestBody.OptionValueIDs,requestBody.OnlyLeafServices,requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<WorkerServiceVM>>(result);
                return Ok(resultVM);
            }

            return BadRequest(result);
        }


        [HttpPost("GetWorkerServiceById_Base")]
        public async Task<ActionResult<RowResultObject<WorkerServiceVM>>> GetWorkerServiceById_Base(GetWorkerServiceRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var userId = User.GetCurrentUserId();
            var dbcustomer = await _customerRep.ExistCustomerAsync(userId.ToString(), "personid");
            var customerId = requestBody.CustomerID ?? dbcustomer.ID;
            var discountId = requestBody.DiscountID ?? 0;


            var result = await _WorkerServiceRep.GetWorkerServiceByIdAsync(requestBody.WorkerID,requestBody.ServiceID,customerId,discountId,requestBody.OptionValueIDs);
            if (result.Status)
            {
                var resultVM = _mapper.Map<RowResultObject<WorkerServiceVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistWorkerService_Base")]
        public async Task<ActionResult<BitResultObject>> ExistWorkerService_Base(GetWorkerServiceRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _WorkerServiceRep.ExistWorkerServiceAsync(requestBody.WorkerID, requestBody.ServiceID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddWorkerServices_Base")]
        public async Task<ActionResult<BitResultObject>> AddWorkerServices_Base(List<AddEditWorkerServiceRequestBody> requestBodyList)
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
                var validrecord = await _WorkerServiceRep.ExistWorkerServiceAsync(item.WorkerID,item.ServiceID);

                if (validrecord.Status && !item.HasDynamicPricing)
                {
                    result.Status = !validrecord.Status;
                    result.ErrorMessage = $"برای خدمت {theService.Result.ServiceName} با قیمت ثابت یا متغیر قبلا اطلاعات ثبت کرده اید امکان ثبت برای یک خدمت هم با قیمت ثابت هم متغیر وجود ندارد";
                    return BadRequest(result);
                }
            }
         


            var workerServices = requestBodyList.Select(requestBody => new WorkerService()
            {
                WorkerID = requestBody.WorkerID,
                ServiceManagementID = requestBody.ServiceID,
                DepositPercent = requestBody.DepositPercent,
                ServiceDuration = requestBody.Duration,
                ServicePrice = requestBody.ServicePrice,
                HasDynamicPricing = requestBody.HasDynamicPricing,
                Description = requestBody.Description,
                PriceVariants = BuildPriceVariants(requestBody)
            }).ToList();

            result = await _WorkerServiceRep.AddWorkerServicesAsync(workerServices);

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


        [HttpPut("EditWorkerServices_Base")]
        public async Task<ActionResult<BitResultObject>> EditWorkerServices_Base(List<AddEditWorkerServiceRequestBody> requestBodyList)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBodyList);
            }

            var workerServices = requestBodyList.Select(requestBody => new WorkerService()
            {
                WorkerID = requestBody.WorkerID,
                ServiceManagementID = requestBody.ServiceID,
                ServiceDuration = requestBody.Duration,
                DepositPercent = requestBody.DepositPercent,
                ServicePrice = requestBody.ServicePrice,
                HasDynamicPricing = requestBody.HasDynamicPricing,
                Description = requestBody.Description,
                PriceVariants = BuildPriceVariants(requestBody)
            }).ToList();

            var result = await _WorkerServiceRep.EditWorkerServicesAsync(workerServices);

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


        [HttpDelete("DeleteWorkerServices_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteWorkerServices_Base(List<DeleteWorkerServiceRowRequestBody> requestBodyList)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBodyList);
            }

            var workerServiceIds = requestBodyList
                .Select(requestBody => (requestBody.WorkerID, requestBody.ServiceID, requestBody.WorkerServicePriceVariantID))
                .ToList();

            var result = await _WorkerServiceRep.RemoveWorkerServicesAsync(workerServiceIds);

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

        private static List<WorkerServicePriceVariant> BuildPriceVariants(AddEditWorkerServiceRequestBody requestBody)
        {
            return requestBody.PriceVariants?
                .Where(x => x.OptionValueIDs != null && x.OptionValueIDs.Any())
                .Select(x => new WorkerServicePriceVariant
                {
                    ID = x.ID,
                    WorkerID = requestBody.WorkerID,
                    ServiceManagementID = requestBody.ServiceID,
                    Price = x.Price,
                    DepositPercent = x.DepositPercent,
                    Duration = x.Duration,
                    IsActive = x.IsActive,
                    BookingTagID = x.BookingTagID,
                    OptionValueCombinationKey = WorkerServicePriceVariant.BuildOptionValueCombinationKey(x.OptionValueIDs),
                    OptionValues = x.OptionValueIDs
                        .Where(optionValueId => optionValueId > 0)
                        .Distinct()
                        .Select(optionValueId => new WorkerServicePriceVariantOptionValue
                        {
                            ServiceOptionValueID = optionValueId
                        })
                        .ToList()
                })
                .ToList() ?? new List<WorkerServicePriceVariant>();
        }

    }
}
