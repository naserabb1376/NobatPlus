using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.RequestObjects;
using NobatPlusAPI.RequestObjects.City;
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
    [Route("DiscountAssignment")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class DiscountAssignmentController : ControllerBase
    {
        IDiscountAssignmentRep _DiscountAssignmentRep;
        ILogRep _logRep;

        public DiscountAssignmentController(IDiscountAssignmentRep DiscountAssignmentRep,ILogRep logRep)
        {
           _DiscountAssignmentRep = DiscountAssignmentRep;
           _logRep = logRep;
        }

        [HttpPost("GetAllDiscountAssignments_Base")]
        public async Task<ActionResult<ListResultObject<DiscountAssignment>>> GetAllDiscountAssignments_Base(GetDiscountAssignmentListRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _DiscountAssignmentRep.GetAllDiscountAssignmentsAsync(requestBody.DiscountId,requestBody.AdminId,requestBody.StylistId,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("GetDiscountAssignmentById_Base")]
        public async Task<ActionResult<ListResultObject<DiscountAssignment>>> GetDiscountAssignmentById_Base(GetRowRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _DiscountAssignmentRep.GetDiscountAssignmentByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistDiscountAssignment_Base")]
        public async Task<ActionResult<BitResultObject>> ExistDiscountAssignment_Base(GetRowRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _DiscountAssignmentRep.ExistDiscountAssignmentAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddDiscountAssignment_Base")]
        public async Task<ActionResult<BitResultObject>> AddDiscountAssignment_Base(AddEditDiscountAssignmentRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            DiscountAssignment DiscountAssignment = new DiscountAssignment()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                Description = requestBody.Description,
                StylistId = requestBody.StylistId,
                AdminId = requestBody.AdminId,
                DiscountId = requestBody.DiscountId,
                UpdateDate = DateTime.Now.ToShamsi(),
            };
            var result = await _DiscountAssignmentRep.AddDiscountAssignmentAsync(DiscountAssignment);
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

        [HttpPut("EditDiscountAssignment_Base")]
        public async Task<ActionResult<BitResultObject>> EditDiscountAssignment_Base(AddEditDiscountAssignmentRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var theRow = await _DiscountAssignmentRep.GetDiscountAssignmentByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            DiscountAssignment DiscountAssignment = new DiscountAssignment()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ID = requestBody.ID,
                Description = requestBody.Description,
                StylistId = requestBody.StylistId,
                AdminId = requestBody.AdminId,
                DiscountId = requestBody.DiscountId,
            };
            result = await _DiscountAssignmentRep.EditDiscountAssignmentAsync(DiscountAssignment);
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

        [HttpDelete("DeleteDiscountAssignment_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteDiscountAssignment_Base(GetRowRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _DiscountAssignmentRep.RemoveDiscountAssignmentAsync(requestBody.ID);
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
