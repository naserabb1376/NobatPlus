using AutoMapper;
using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Models.Worker;
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
    [Route("worker")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class WorkerController : ControllerBase
    {
        IWorkerRep _WorkerRep;
        IAddressRep _AddressRep;
        ILogRep _logRep;
        private readonly IMapper _mapper;


        public WorkerController(IWorkerRep WorkerRep,IAddressRep addressRep, ILogRep logRep, IMapper mapper)
        {
            _WorkerRep = WorkerRep;
            _AddressRep = addressRep;
            _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllWorkers_Base")]
        [AllowAnonymous]
        public async Task<ActionResult<ListResultObject<WorkerVM>>> GetAllWorkers_Base(GetWorkerListRequestBody requestBody)
        {
            ListResultObject<WorkerDTO> result = new ListResultObject<WorkerDTO>();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
                result = await _WorkerRep.GetAllWorkersAsync(requestBody.ParentID, requestBody.ServiceIDs, requestBody.JobTypeID, requestBody.DiscountID, requestBody.FromPrice, requestBody.ToPrice, requestBody.CityID, requestBody.Gender, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery, requestBody.FindLocationRequestBody, requestBody.AccountStatus ?? "");
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<WorkerVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("GetWorkerById_Base")]
        [AllowAnonymous]
        public async Task<ActionResult<RowResultObject<WorkerVM>>> GetWorkerById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _WorkerRep.GetWorkerByIdAsync(requestBody.ID);
            if (result.Status)
            {
                var resultVM = _mapper.Map<RowResultObject<WorkerVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistWorker_Base")]
        public async Task<ActionResult<BitResultObject>> ExistWorker_Base(ExistWorkerRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _WorkerRep.ExistWorkerAsync(requestBody.FieldValue,requestBody.FieldName);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddWorker_Base")]
        public async Task<ActionResult<BitResultObject>> AddWorker_Base(AddEditWorkerRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
          
            Worker Worker = new Worker()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                JobTypeID = requestBody.JobTypeID,
                PersonID = requestBody.PersonID,
                WorkerParentID = requestBody.WorkerParentID,
                Specialty = requestBody.Specialty ?? "",
                YearsOfExperience = requestBody.YearsOfExperience,
                Description = requestBody.Description ?? "",
                GenderAccepted = requestBody.GenderAccepted ?? "",
                WorkerBio = requestBody.WorkerBio ?? "",
                WorkerName = requestBody.WorkerName ?? "",
                WorkShopInteractMode = requestBody.WorkShopInteractMode ?? "",
                AccountStatus = requestBody.AccountStatus ?? "",
                PayMethod = requestBody.PayMethod ?? "",
                WorkShopDepositAmount = requestBody.WorkShopDepositAmount,
                WorkShopRentAmount = requestBody.WorkShopRentAmount,
                IsWorkShop = requestBody.IsWorkshop,
                RestTime = requestBody.RestTime,
                SlotIntervalMinutes = requestBody.SlotIntervalMinutes,
                BookingCreationMode = requestBody.BookingCreationMode ?? "automatic",
                SlotDisplayMode = requestBody.SlotDisplayMode ?? "all",
                IsActive = requestBody.IsActive,
            };
            var result = await _WorkerRep.AddWorkerAsync(Worker);
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

        [HttpPut("EditWorker_Base")]
        public async Task<ActionResult<BitResultObject>> EditWorker_Base(AddEditWorkerRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var theRow = await _WorkerRep.GetWorkerByIdAsync(requestBody.ID);
          
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            Worker Worker = new Worker()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ID = requestBody.ID,
                JobTypeID = requestBody.JobTypeID,
                PersonID = requestBody.PersonID,
                WorkerParentID = requestBody.WorkerParentID,
                Specialty = requestBody.Specialty ?? "",
                YearsOfExperience = requestBody.YearsOfExperience,
                Description = requestBody.Description ?? "",
                GenderAccepted = requestBody.GenderAccepted ?? "",
                WorkerBio = requestBody.WorkerBio ?? "",
                WorkerName = requestBody.WorkerName ?? "",
                WorkShopInteractMode = requestBody.WorkShopInteractMode ?? "",
                AccountStatus = requestBody.AccountStatus ?? "",
                PayMethod = requestBody.PayMethod ?? "",
                WorkShopDepositAmount = requestBody.WorkShopDepositAmount,
                WorkShopRentAmount = requestBody.WorkShopRentAmount,
                IsWorkShop = requestBody.IsWorkshop,
                RestTime = requestBody.RestTime,
                SlotIntervalMinutes = requestBody.SlotIntervalMinutes,
                BookingCreationMode = requestBody.BookingCreationMode ?? theRow.Result.BookingCreationMode,
                SlotDisplayMode = requestBody.SlotDisplayMode ?? theRow.Result.SlotDisplayMode,
                IsActive = requestBody.IsActive,
            };
            result = await _WorkerRep.EditWorkerAsync(Worker);
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

        [HttpDelete("DeleteWorker_Base")]
        [RequireRole(3, 4)]
        public async Task<ActionResult<BitResultObject>> DeleteWorker_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
         
            var result = await _WorkerRep.RemoveWorkerAsync(requestBody.ID);
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
