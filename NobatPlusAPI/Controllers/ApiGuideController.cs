using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusAPI.Models.ApiGuide;
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
    [Route("ApiGuide")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class ApiGuideController : ControllerBase
    {
        IApiGuideRep _ApiGuideRep;
        ILogRep _logRep;

        public ApiGuideController(IApiGuideRep ApiGuideRep,ILogRep logRep)
        {
           _ApiGuideRep = ApiGuideRep;
           _logRep = logRep;
        }

        [HttpPost("GetAllApiGuides_Base")]
        public async Task<ActionResult<ListResultObject<ApiGuide>>> GetAllApiGuides_Base(GetApiGuideListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _ApiGuideRep.GetAllApiGuidesAsync(requestBody.GuideType,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("GetApiGuideById_Base")]
        public async Task<ActionResult<RowResultObject<ApiGuide>>> GetApiGuideById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _ApiGuideRep.GetApiGuideByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }



        [HttpPost("GetApiGuideByApiName_Base")]
        public async Task<ActionResult<RowResultObject<ApiGuide>>> GetApiGuideByApiName_Base(GetApiGuideByApiNameRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _ApiGuideRep.GetGuideForApiAsync(requestBody.ApiName,requestBody.GuideType);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }


        [HttpPost("ExistApiGuide_Base")]
        public async Task<ActionResult<BitResultObject>> ExistApiGuide_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _ApiGuideRep.ExistApiGuideAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddApiGuide_Base")]
        public async Task<ActionResult<BitResultObject>> AddApiGuide_Base(AddEditApiGuideRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            ApiGuide ApiGuide = new ApiGuide()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                ApiName = requestBody.ApiName,
                FieldEnglishName = requestBody.FieldEnglishName,
                FieldFarsiName = requestBody.FieldFarsiName,
                FieldDataType = requestBody.FieldDataType,
                GuideType = requestBody.GuideType,
                ModelName = requestBody.ModelName,
                FieldRecomendedInputType = requestBody.FieldRecomendedInputType,
                Description = requestBody.Description,
            };
            var result = await _ApiGuideRep.AddApiGuideAsync(ApiGuide);
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

        [HttpPut("EditApiGuide_Base")]
        public async Task<ActionResult<BitResultObject>> EditApiGuide_Base(AddEditApiGuideRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var theRow = await _ApiGuideRep.GetApiGuideByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            ApiGuide ApiGuide = new ApiGuide()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ID = requestBody.ID,
                ApiName = requestBody.ApiName,
                FieldEnglishName = requestBody.FieldEnglishName,
                FieldFarsiName = requestBody.FieldFarsiName,
                FieldDataType = requestBody.FieldDataType,
                GuideType = requestBody.GuideType,
                ModelName = requestBody.ModelName,
                FieldRecomendedInputType = requestBody.FieldRecomendedInputType,
                Description = requestBody.Description,
            };
            result = await _ApiGuideRep.EditApiGuideAsync(ApiGuide);
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

        [HttpDelete("DeleteApiGuide_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteApiGuide_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _ApiGuideRep.RemoveApiGuideAsync(requestBody.ID);
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
