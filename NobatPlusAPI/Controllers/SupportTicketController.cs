using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Models.SupportTicket;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;

namespace NobatPlusAPI.Controllers
{
    [Route("SupportTicket")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class SupportTicketController : ControllerBase
    {
        private readonly ISupportTicketRep _supportTicketRep;

        public SupportTicketController(ISupportTicketRep supportTicketRep)
        {
            _supportTicketRep = supportTicketRep;
        }

        [HttpPost("GetAllSupportTickets_Base")]
        public async Task<ActionResult<ListResultObject<SupportTicketVM>>> GetAllSupportTickets_Base(GetSupportTicketListRequestBody requestBody)
        {
            if (!ModelState.IsValid) return BadRequest(requestBody);

            var isAdmin = User.GetCurrentRoleId() == (long)DbTools.BaseRole.Admin;
            var personId = isAdmin ? requestBody.PersonID : User.GetCurrentUserId();

            var result = await _supportTicketRep.GetAllSupportTicketsAsync(
                personId,
                requestBody.Status,
                requestBody.Priority,
                requestBody.Category,
                requestBody.FromDate,
                requestBody.ToDate,
                requestBody.PageIndex,
                requestBody.PageSize,
                requestBody.SearchText,
                requestBody.SortQuery);

            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPost("GetSupportTicketById_Base")]
        public async Task<ActionResult<RowResultObject<SupportTicketVM>>> GetSupportTicketById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid) return BadRequest(requestBody);

            var result = await _supportTicketRep.GetSupportTicketByIdAsync(requestBody.ID);
            if (!result.Status) return BadRequest(result);

            if (User.GetCurrentRoleId() != (long)DbTools.BaseRole.Admin && result.Result.PersonID != User.GetCurrentUserId())
                return Forbid();

            return Ok(result);
        }

        [HttpPost("AddSupportTicket_Base")]
        public async Task<ActionResult<BitResultObject>> AddSupportTicket_Base(AddSupportTicketRequestBody requestBody)
        {
            if (!ModelState.IsValid) return BadRequest(requestBody);

            var isAdmin = User.GetCurrentRoleId() == (long)DbTools.BaseRole.Admin;
            var personId = isAdmin && requestBody.PersonID > 0 ? requestBody.PersonID : User.GetCurrentUserId();

            var ticket = new SupportTicket
            {
                PersonID = personId,
                Title = requestBody.Title.Trim(),
                Category = requestBody.Category.Trim(),
                Priority = requestBody.Priority.Trim(),
                Status = "open",
                Description = requestBody.Description
            };

            var result = await _supportTicketRep.AddSupportTicketAsync(ticket, requestBody.Message.Trim());
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPost("AddSupportTicketMessage_Base")]
        public async Task<ActionResult<BitResultObject>> AddSupportTicketMessage_Base(AddSupportTicketMessageRequestBody requestBody)
        {
            if (!ModelState.IsValid) return BadRequest(requestBody);

            var ticket = await _supportTicketRep.GetSupportTicketByIdAsync(requestBody.TicketID);
            if (!ticket.Status) return BadRequest(ticket);

            var isAdmin = User.GetCurrentRoleId() == (long)DbTools.BaseRole.Admin;
            if (!isAdmin && ticket.Result.PersonID != User.GetCurrentUserId())
                return Forbid();

            var result = await _supportTicketRep.AddSupportTicketMessageAsync(
                requestBody.TicketID,
                User.GetCurrentUserId(),
                isAdmin,
                requestBody.Message.Trim(),
                isAdmin ? requestBody.NextStatus : "");

            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPost("UpdateSupportTicketStatus_Base")]
        public async Task<ActionResult<BitResultObject>> UpdateSupportTicketStatus_Base(UpdateSupportTicketStatusRequestBody requestBody)
        {
            if (!ModelState.IsValid) return BadRequest(requestBody);
            if (User.GetCurrentRoleId() != (long)DbTools.BaseRole.Admin) return Forbid();

            var result = await _supportTicketRep.UpdateSupportTicketStatusAsync(
                requestBody.TicketID,
                requestBody.Status,
                User.GetCurrentUserId());

            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("DeleteSupportTicket_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteSupportTicket_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid) return BadRequest(requestBody);
            if (User.GetCurrentRoleId() != (long)DbTools.BaseRole.Admin) return Forbid();

            var result = await _supportTicketRep.RemoveSupportTicketAsync(requestBody.ID);
            return result.Status ? Ok(result) : BadRequest(result);
        }
    }
}
