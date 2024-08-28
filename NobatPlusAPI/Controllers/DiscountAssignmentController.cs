using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.City;
using NobatPlusAPI.Models.DiscountAssignment;
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

        [HttpPost("AddDiscountAssignments_Base")]
        public async Task<ActionResult<BitResultObject>> AddDiscountAssignments_Base(List<AddEditDiscountAssignmentRequestBody> requestBodies)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBodies);
            }

            var discountAssignments = requestBodies.Select(requestBody => new DiscountAssignment()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                Description = requestBody.Description,
                StylistId = requestBody.StylistId,
                AdminId = requestBody.AdminId,
                DiscountId = requestBody.DiscountId,
                UpdateDate = DateTime.Now.ToShamsi(),
            }).ToList();

            var result = await _DiscountAssignmentRep.AddDiscountAssignmentsAsync(discountAssignments);
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


        [HttpPut("EditDiscountAssignments_Base")]
        public async Task<ActionResult<BitResultObject>> EditDiscountAssignments_Base(List<AddEditDiscountAssignmentRequestBody> requestBodies)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBodies);
            }

            var discountAssignments = new List<DiscountAssignment>();

            foreach (var requestBody in requestBodies)
            {
                var theRow = await _DiscountAssignmentRep.GetDiscountAssignmentByIdAsync(requestBody.ID);
                if (!theRow.Status)
                {
                    return BadRequest(theRow);
                }

                discountAssignments.Add(new DiscountAssignment()
                {
                    CreateDate = theRow.Result.CreateDate,
                    UpdateDate = DateTime.Now.ToShamsi(),
                    ID = requestBody.ID,
                    Description = requestBody.Description,
                    StylistId = requestBody.StylistId,
                    AdminId = requestBody.AdminId,
                    DiscountId = requestBody.DiscountId,
                });
            }

            var result = await _DiscountAssignmentRep.EditDiscountAssignmentsAsync(discountAssignments);
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


        [HttpDelete("DeleteDiscountAssignments_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteDiscountAssignments_Base(List<GetRowRequestBody> requestBodies)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBodies);
            }

            var discountAssignmentIds = requestBodies.Select(rb => rb.ID).ToList();
            var result = await _DiscountAssignmentRep.RemoveDiscountAssignmentsAsync(discountAssignmentIds);

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
