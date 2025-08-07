using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusAPI.Models.Stylist;
using NobatPlusAPI.Models.Public;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
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

        public StylistController(IStylistRep StylistRep,IAddressRep addressRep, ILogRep logRep)
        {
            _StylistRep = StylistRep;
            _AddressRep = addressRep;
            _logRep = logRep;
        }

        [HttpPost("GetAllStylists_Base")]
        [AllowAnonymous]
        public async Task<ActionResult<ListResultObject<Stylist>>> GetAllStylists_Base(GetStylistListRequestBody requestBody)
        {
            ListResultObject<Stylist> result = new ListResultObject<Stylist>();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            if (requestBody.DiscountID > 0)
            {
               result = await _StylistRep.GetStylistsOfDiscountAsync(requestBody.DiscountID,requestBody.CityID, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText,requestBody.SortQuery);
            }
            if (requestBody.ServiceID > 0)
            {
                result = await _StylistRep.GetStylistsOfServiceAsync(requestBody.ServiceID,requestBody.CityID, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText,requestBody.SortQuery);
            }
            if (requestBody.JobTypeID > 0)
            {
                result = await _StylistRep.GetStylistsOfJobTypeAsync(requestBody.JobTypeID,requestBody.CityID, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText,requestBody.SortQuery);
            }
            else
                result = await _StylistRep.GetAllStylistsAsync(requestBody.ParentID,requestBody.CityID,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("GetStylistById_Base")]
        public async Task<ActionResult<RowResultObject<Stylist>>> GetStylistById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _StylistRep.GetStylistByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistStylist_Base")]
        public async Task<ActionResult<BitResultObject>> ExistStylist_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _StylistRep.ExistStylistAsync(requestBody.ID);
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
