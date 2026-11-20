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
[Route("SubscriptionPlan")]
[Authorize]
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
        var result = await _repository.GetAllSubscriptionPlansAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(_mapper.Map<ListResultObject<SubscriptionPlanVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetSubscriptionPlanById_Base")]
    public async Task<ActionResult<RowResultObject<SubscriptionPlanVM>>> GetSubscriptionPlanById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetSubscriptionPlanByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(_mapper.Map<RowResultObject<SubscriptionPlanVM>>(result)) : BadRequest(result);
    }

    [HttpPost("ExistSubscriptionPlan_Base")]
    public async Task<ActionResult<BitResultObject>> ExistSubscriptionPlan_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.ExistSubscriptionPlanAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long)BaseRole.Admin);
        return result.Status ? Ok(result) : BadRequest(result);
    }
}

