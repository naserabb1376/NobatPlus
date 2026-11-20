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
[Route("BillingAccount")]
[Authorize]
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

    [HttpPost("ExistBillingAccount_Base")]
    public async Task<ActionResult<BitResultObject>> ExistBillingAccount_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.ExistBillingAccountAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpPost("GetAllBillingAccounts_Base")]
    public async Task<ActionResult<ListResultObject<BillingAccountVM>>> GetAllBillingAccounts_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllBillingAccountsAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(_mapper.Map<ListResultObject<BillingAccountVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetBillingAccountById_Base")]
    public async Task<ActionResult<RowResultObject<BillingAccountVM>>> GetBillingAccountById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetBillingAccountByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(_mapper.Map<RowResultObject<BillingAccountVM>>(result)) : BadRequest(result);
    }
}

