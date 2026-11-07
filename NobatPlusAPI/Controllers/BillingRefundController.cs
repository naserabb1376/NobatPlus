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
[Route("BillingRefund")]
[Authorize]
[RequireRole(2, 3, 4)]
[Produces("application/json")]
public class BillingRefundController : ControllerBase
{
    private readonly IBillingRefundRep _repository;
    private readonly IMapper _mapper;
    public BillingRefundController(IBillingRefundRep repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpPost("GetAllBillingRefunds_Base")]
    public async Task<ActionResult<ListResultObject<BillingRefundVM>>> GetAllBillingRefunds_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<ListResultObject<BillingRefundVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetBillingRefundById_Base")]
    public async Task<ActionResult<RowResultObject<BillingRefundVM>>> GetBillingRefundById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<RowResultObject<BillingRefundVM>>(result)) : BadRequest(result);
    }
}
