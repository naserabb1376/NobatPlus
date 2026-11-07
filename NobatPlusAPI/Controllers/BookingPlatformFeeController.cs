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
namespace NobatPlusAPI.Controllers;

[ApiController]
[Route("BookingPlatformFee")]
[Authorize]
[RequireRole(2, 3, 4)]
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

    [HttpPost("GetAllBookingPlatformFees_Base")]
    public async Task<ActionResult<ListResultObject<BookingPlatformFeeVM>>> GetAllBookingPlatformFees_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<ListResultObject<BookingPlatformFeeVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetBookingPlatformFeeById_Base")]
    public async Task<ActionResult<RowResultObject<BookingPlatformFeeVM>>> GetBookingPlatformFeeById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<RowResultObject<BookingPlatformFeeVM>>(result)) : BadRequest(result);
    }
}

