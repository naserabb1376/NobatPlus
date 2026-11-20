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
[Route("BillingAllocation")]
[Authorize]
[RequireRole(2, 3, 4)]
[Produces("application/json")]
public class BillingAllocationController : ControllerBase
{
    private readonly IBillingAllocationRep _repository;
    private readonly IMapper _mapper;
    public BillingAllocationController(IBillingAllocationRep repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpPost("GetAllBillingAllocations_Base")]
    public async Task<ActionResult<ListResultObject<BillingAllocationVM>>> GetAllBillingAllocations_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<ListResultObject<BillingAllocationVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetBillingAllocationById_Base")]
    public async Task<ActionResult<RowResultObject<BillingAllocationVM>>> GetBillingAllocationById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<RowResultObject<BillingAllocationVM>>(result)) : BadRequest(result);
    }
}

