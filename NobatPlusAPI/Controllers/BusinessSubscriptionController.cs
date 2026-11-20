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
[Route("BusinessSubscription")]
[Authorize]
[RequireRole(2, 3, 4)]
[Produces("application/json")]
public class BusinessSubscriptionController : ControllerBase
{
    private readonly IBusinessSubscriptionRep _repository;
    private readonly IMapper _mapper;
    public BusinessSubscriptionController(IBusinessSubscriptionRep repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpPost("GetAllBusinessSubscriptions_Base")]
    public async Task<ActionResult<ListResultObject<BusinessSubscriptionVM>>> GetAllBusinessSubscriptions_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<ListResultObject<BusinessSubscriptionVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetBusinessSubscriptionById_Base")]
    public async Task<ActionResult<RowResultObject<BusinessSubscriptionVM>>> GetBusinessSubscriptionById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<RowResultObject<BusinessSubscriptionVM>>(result)) : BadRequest(result);
    }
}

