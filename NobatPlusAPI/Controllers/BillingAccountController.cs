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
[Route("BillingAccount")]
[Authorize]
[RequireRole(2, 3, 4)]
[Produces("application/json")]
public class BillingAccountController : ControllerBase
{
    private readonly IBillingAccountRep _repository;
    private readonly IMapper _mapper;
    public BillingAccountController(IBillingAccountRep repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpPost("GetAllBillingAccounts_Base")]
    public async Task<ActionResult<ListResultObject<BillingAccountVM>>> GetAllBillingAccounts_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<ListResultObject<BillingAccountVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetBillingAccountById_Base")]
    public async Task<ActionResult<RowResultObject<BillingAccountVM>>> GetBillingAccountById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<RowResultObject<BillingAccountVM>>(result)) : BadRequest(result);
    }
}

