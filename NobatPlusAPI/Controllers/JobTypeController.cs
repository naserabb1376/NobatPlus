using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.RequestObjects;
using NobatPlusAPI.RequestObjects.Authenticate;
using NobatPlusAPI.RequestObjects.JobType;
using NobatPlusAPI.RequestObjects.Public;
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
    [Route("jobtype")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class JobTypeController : ControllerBase
    {
        IJobTypeRep _jobTypeRep;
        ILogRep _logRep;

        public JobTypeController(IJobTypeRep jobTypeRep,ILogRep logRep)
        {
           _jobTypeRep = jobTypeRep;
           _logRep = logRep;
        }

        [HttpPost("GetAllJobTypes_Base")]
        public async Task<ActionResult<ListResultObject<JobType>>> GetAllJobTypes_Base(GetJobTypeListRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _jobTypeRep.GetAllJobTypesAsync(requestBody.SexTypeChecked,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("GetJobTypeById_Base")]
        public async Task<ActionResult<ListResultObject<JobType>>> GetJobTypeById_Base(GetRowRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _jobTypeRep.GetJobTypeByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistJobType_Base")]
        public async Task<ActionResult<BitResultObject>> ExistJobType_Base(GetRowRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _jobTypeRep.ExistJobTypeAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddJobType_Base")]
        public async Task<ActionResult<BitResultObject>> AddJobType_Base(AddEditJobTypeRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            JobType jobType = new JobType()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                JobTitle = requestBody.JobTitle,
                SexTypeChecked = requestBody.SexTypeChecked,
                Description = requestBody.Description,
            };
            var result = await _jobTypeRep.AddJobTypeAsync(jobType);
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
                result = await _logRep.AddLogAsync(log);

                #endregion


                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPut("EditJobType_Base")]
        public async Task<ActionResult<BitResultObject>> EditJobType_Base(AddEditJobTypeRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var theRow = await _jobTypeRep.GetJobTypeByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            JobType jobType = new JobType()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ID = requestBody.ID,
                JobTitle = requestBody.JobTitle,
                SexTypeChecked = requestBody.SexTypeChecked,
                Description = requestBody.Description,
            };
            result = await _jobTypeRep.EditJobTypeAsync(jobType);
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

        [HttpDelete("DeleteJobType_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteJobType_Base(GetRowRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _jobTypeRep.RemoveJobTypeAsync(requestBody.ID);
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
