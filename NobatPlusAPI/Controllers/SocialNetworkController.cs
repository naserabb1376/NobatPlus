using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusAPI.Models.SocialNetwork;
using NobatPlusAPI.Models.Public;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;

namespace NobatPlusAPI.Controllers
{
    [Route("SocialNetwork")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class SocialNetworkController : ControllerBase
    {
        ISocialNetworkRep _SocialNetworkRep;
        ILogRep _logRep;

        public SocialNetworkController(ISocialNetworkRep SocialNetworkRep,ILogRep logRep)
        {
           _SocialNetworkRep = SocialNetworkRep;
            _logRep = logRep;
        }

        [HttpPost("GetAllSocialNetworks_Base")]
        public async Task<ActionResult<ListResultObject<SocialNetwork>>> GetAllSocialNetworks_Base(GetSocialNetworkListRequestBody requestBody)
        {
            var result = new ListResultObject<SocialNetwork>();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            result = await _SocialNetworkRep.GetAllSocialNetworksAsync(requestBody.StylistId, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery);
            if (result.Status)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }


        [HttpPost("GetSocialNetworkById_Base")]
        public async Task<ActionResult<RowResultObject<SocialNetwork>>> GetSocialNetworkById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _SocialNetworkRep.GetSocialNetworkByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistSocialNetwork_Base")]
        public async Task<ActionResult<BitResultObject>> ExistSocialNetwork_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _SocialNetworkRep.ExistSocialNetworkAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddSocialNetwork_Base")]
        public async Task<ActionResult<BitResultObject>> AddSocialNetwork_Base(AddEditSocialNetworkRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var existSocialNetwork = await _SocialNetworkRep.ExistSocialNetworkAccountAsync(requestBody.SocialNetworkName,requestBody.StylistID,requestBody.PhoneNumber,requestBody.AccountLink);
            if (!existSocialNetwork.Status)
            {
                SocialNetwork SocialNetwork = new SocialNetwork()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    AccountLink = requestBody.AccountLink,
                    PhoneNumber = requestBody.PhoneNumber,
                    SocialNetworkIcon = requestBody.SocialNetworkIcon ?? "",
                    SocialNetworkName = requestBody.SocialNetworkName,
                    StylistID = requestBody.StylistID,
                    Description = "",
                };
                var result = await _SocialNetworkRep.AddSocialNetworkAsync(SocialNetwork);
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
            else
            {
                return BadRequest(existSocialNetwork);
            }

        }

        [HttpPut("EditSocialNetwork_Base")]
        public async Task<ActionResult<BitResultObject>> EditSocialNetwork_Base(AddEditSocialNetworkRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var existSocialNetwork = await _SocialNetworkRep.ExistSocialNetworkAccountAsync(requestBody.SocialNetworkName, requestBody.StylistID, requestBody.PhoneNumber, requestBody.AccountLink);
            if (!existSocialNetwork.Status)
            {

                var theRow = await _SocialNetworkRep.GetSocialNetworkByIdAsync(requestBody.ID);

                if (!theRow.Status)
                {
                    result.Status = theRow.Status;
                    result.ErrorMessage = theRow.ErrorMessage;
                }

                SocialNetwork SocialNetwork = new SocialNetwork()
                {
                    ID = requestBody.ID,
                    CreateDate = theRow.Result.CreateDate,
                    UpdateDate = DateTime.Now.ToShamsi(),
                    AccountLink = requestBody.AccountLink,
                    PhoneNumber = requestBody.PhoneNumber,
                    SocialNetworkIcon = requestBody.SocialNetworkIcon ?? "",
                    SocialNetworkName = requestBody.SocialNetworkName,
                    StylistID = requestBody.StylistID,
                    Description = "",
                };
                result = await _SocialNetworkRep.EditSocialNetworkAsync(SocialNetwork);
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
            else
            {
                return BadRequest(existSocialNetwork);
            }
        }

        [HttpDelete("DeleteSocialNetwork_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteSocialNetwork_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _SocialNetworkRep.RemoveSocialNetworkAsync(requestBody.ID);
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
