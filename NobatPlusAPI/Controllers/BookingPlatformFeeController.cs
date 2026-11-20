using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.Billing;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Tools;
using NobatPlusAPI.ViewModels.Billing;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using static NobatPlusDATA.Tools.DbTools;
namespace NobatPlusAPI.Controllers;

[ApiController]
[Route("BookingPlatformFee")]
[Authorize]
[Produces("application/json")]
public class BookingPlatformFeeController : ControllerBase
{
    private readonly IBookingPlatformFeeRep _repository;
    private readonly IMapper _mapper;
    public BookingPlatformFeeController(IBookingPlatformFeeRep repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpPost("ExistBookingPlatformFee_Base")]
    public async Task<ActionResult<BitResultObject>> ExistBookingPlatformFee_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.ExistBookingPlatformFeeAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpPost("GetAllBookingPlatformFees_Base")]
    public async Task<ActionResult<ListResultObject<BookingPlatformFeeVM>>> GetAllBookingPlatformFees_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllBookingPlatformFeesAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(_mapper.Map<ListResultObject<BookingPlatformFeeVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetBookingPlatformFeeById_Base")]
    public async Task<ActionResult<RowResultObject<BookingPlatformFeeVM>>> GetBookingPlatformFeeById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetBookingPlatformFeeByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(_mapper.Map<RowResultObject<BookingPlatformFeeVM>>(result)) : BadRequest(result);
    }
}

