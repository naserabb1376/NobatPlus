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
[Route("SubscriptionPlan")]
[Authorize]
[RequireRole(2, 3, 4)]
[Produces("application/json")]
public class SubscriptionPlanController : ControllerBase
{
    private readonly ISubscriptionPlanRep _repository;
    private readonly IMapper _mapper;
    public SubscriptionPlanController(ISubscriptionPlanRep repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpPost("GetAllSubscriptionPlans_Base")]
    public async Task<ActionResult<ListResultObject<SubscriptionPlanVM>>> GetAllSubscriptionPlans_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<ListResultObject<SubscriptionPlanVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetSubscriptionPlanById_Base")]
    public async Task<ActionResult<RowResultObject<SubscriptionPlanVM>>> GetSubscriptionPlanById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<RowResultObject<SubscriptionPlanVM>>(result)) : BadRequest(result);
    }
}

