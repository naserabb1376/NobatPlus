using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Models.Setting;
using NobatPlusAPI.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static NobatPlusAPI.Tools.ToolBox;
using Domains;

namespace NobatPlusAPI.Controllers
{
    [Route("Setting")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class SettingController : ControllerBase
    {
        ISettingRep _SettingRep;
        ILogRep _logRep;
        private readonly IMapper _mapper;


        public SettingController(ISettingRep SettingRep,ILogRep logRep,IMapper mapper)
        {
           _SettingRep = SettingRep;
           _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllSettings_Base")]
        public async Task<ActionResult<ListResultObject<SettingVM>>> GetAllSettings_Base(GetSettingListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _SettingRep.GetAllSettingsAsync(requestBody.ParentId,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<SettingVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("GetSettingById_Base")]
        public async Task<ActionResult<RowResultObject<SettingVM>>> GetSettingById_Base(GetSettingRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _SettingRep.GetSettingRowAsync(requestBody.ID,requestBody.Key);
            if (result.Status)
            {
                var resultVM = _mapper.Map<RowResultObject<SettingVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistSetting_Base")]
        public async Task<ActionResult<BitResultObject>> ExistSetting_Base(GetSettingRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _SettingRep.ExistSettingRowAsync(requestBody.ID,requestBody.Key);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddSetting_Base")]
        public async Task<ActionResult<BitResultObject>> AddSetting_Base(AddEditSettingRequestBody requestBody)
        {
            BitResultObject result = new BitResultObject();

            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var existSetting = await _SettingRep.ExistSettingRowAsync(0,requestBody.Key);

            if (existSetting.Status)
            {
                result.Status = existSetting.Status;
                result.ErrorMessage = "این کلید تنظیمات در سیستم وجود دارد";
                return BadRequest(result);
            }

            Setting Setting = new Setting()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                Key = requestBody.Key,
                Value = requestBody.Value,
                ParentId = requestBody.ParentId,
                //OtherLangs = requestBody.OtherLangs ?? "",

            };
             result = await _SettingRep.AddSettingAsync(Setting);
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

        [HttpPut("EditSetting_Base")]
        public async Task<ActionResult<BitResultObject>> EditSetting_Base(AddEditSettingRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var theRow = await _SettingRep.GetSettingRowAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            var existSetting = await _SettingRep.ExistSettingRowAsync(0, requestBody.Key);

            if (existSetting.Status && existSetting.ID != theRow.Result.ID)
            {
                result.Status = existSetting.Status;
                result.ErrorMessage = "این کلید تنظیمات در سیستم وجود دارد";
                return BadRequest(result);
            }


            Setting Setting = new Setting()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ID = requestBody.ID,
                Key = requestBody.Key,
                Value = requestBody.Value,
                ParentId = requestBody.ParentId,
                //OtherLangs = requestBody.OtherLangs ?? "",

            };
            result = await _SettingRep.EditSettingAsync(Setting);
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

        [HttpDelete("DeleteSetting_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteSetting_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _SettingRep.RemoveSettingAsync(requestBody.ID);
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
