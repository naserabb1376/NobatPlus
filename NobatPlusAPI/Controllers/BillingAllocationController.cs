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
[Route("BillingAllocation")]
[Authorize]
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

    [HttpPost("ExistBillingAllocation_Base")]
    public async Task<ActionResult<BitResultObject>> ExistBillingAllocation_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.ExistBillingAllocationAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpPost("GetAllBillingAllocations_Base")]
    public async Task<ActionResult<ListResultObject<BillingAllocationVM>>> GetAllBillingAllocations_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllBillingAllocationsAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(_mapper.Map<ListResultObject<BillingAllocationVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetBillingAllocationById_Base")]
    public async Task<ActionResult<RowResultObject<BillingAllocationVM>>> GetBillingAllocationById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetBillingAllocationByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(_mapper.Map<RowResultObject<BillingAllocationVM>>(result)) : BadRequest(result);
    }
}

