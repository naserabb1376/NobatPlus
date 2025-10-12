using AutoMapper;
using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Models.Stylist;
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
    [Route("Stylist")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class StylistController : ControllerBase
    {
        IStylistRep _StylistRep;
        IAddressRep _AddressRep;
        ILogRep _logRep;
        private readonly IMapper _mapper;


        public StylistController(IStylistRep StylistRep,IAddressRep addressRep, ILogRep logRep, IMapper mapper)
        {
            _StylistRep = StylistRep;
            _AddressRep = addressRep;
            _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllStylists_Base")]
        [AllowAnonymous]
        public async Task<ActionResult<ListResultObject<StylistVM>>> GetAllStylists_Base(GetStylistListRequestBody requestBody)
        {
            ListResultObject<StylistDTO> result = new ListResultObject<StylistDTO>();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
                result = await _StylistRep.GetAllStylistsAsync(requestBody.ParentID,requestBody.ServiceID,requestBody.JobTypeID,requestBody.DiscountID,requestBody.CityID,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<StylistVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("GetStylistById_Base")]
        [AllowAnonymous]
        public async Task<ActionResult<RowResultObject<StylistVM>>> GetStylistById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _StylistRep.GetStylistByIdAsync(requestBody.ID);
            if (result.Status)
            {
                var resultVM = _mapper.Map<RowResultObject<StylistVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistStylist_Base")]
        public async Task<ActionResult<BitResultObject>> ExistStylist_Base(ExistStylistRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _StylistRep.ExistStylistAsync(requestBody.FieldValue,requestBody.FieldName);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddStylist_Base")]
        public async Task<ActionResult<BitResultObject>> AddStylist_Base(AddEditStylistRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            Stylist Stylist = new Stylist()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                JobTypeID = requestBody.JobTypeID,
                PersonID = requestBody.PersonID,
                StylistParentID = requestBody.StylistParentID,
                Specialty = requestBody.Specialty ?? "",
                YearsOfExperience = requestBody.YearsOfExperience,
                Description = requestBody.Description ?? "",
                GenderAccepted = requestBody.GenderAccepted ?? "",
                StylistBio = requestBody.StylistBio ?? "",
                StylistName = requestBody.StylistName ?? "",
                WorkShopInteractMode = requestBody.WorkShopInteractMode ?? "",
                AccountStatus = requestBody.AccountStatus ?? "",
                PayMethod = requestBody.PayMethod ?? "",
                WorkShopDepositAmount = requestBody.WorkShopDepositAmount,
                WorkShopRentAmount = requestBody.WorkShopRentAmount,
                IsWorkShop = requestBody.IsWorkshop,
                RestTime = requestBody.RestTime,
            };
            var result = await _StylistRep.AddStylistAsync(Stylist);
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

        [HttpPut("EditStylist_Base")]
        public async Task<ActionResult<BitResultObject>> EditStylist_Base(AddEditStylistRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var theRow = await _StylistRep.GetStylistByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            Stylist Stylist = new Stylist()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ID = requestBody.ID,
                JobTypeID = requestBody.JobTypeID,
                PersonID = requestBody.PersonID,
                StylistParentID = requestBody.StylistParentID,
                Specialty = requestBody.Specialty ?? "",
                YearsOfExperience = requestBody.YearsOfExperience,
                Description = requestBody.Description ?? "",
                GenderAccepted = requestBody.GenderAccepted ?? "",
                StylistBio = requestBody.StylistBio ?? "",
                StylistName = requestBody.StylistName ?? "",
                WorkShopInteractMode = requestBody.WorkShopInteractMode ?? "",
                AccountStatus = requestBody.AccountStatus ?? "",
                PayMethod = requestBody.PayMethod ?? "",
                WorkShopDepositAmount = requestBody.WorkShopDepositAmount,
                WorkShopRentAmount = requestBody.WorkShopRentAmount,
                IsWorkShop = requestBody.IsWorkshop,
                RestTime = requestBody.RestTime,
            };
            result = await _StylistRep.EditStylistAsync(Stylist);
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

        [HttpDelete("DeleteStylist_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteStylist_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _StylistRep.RemoveStylistAsync(requestBody.ID);
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
