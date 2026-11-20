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
[Route("BillingTransaction")]
[Authorize]
[Produces("application/json")]
public class BillingTransactionController : ControllerBase
{
    private readonly IBillingTransactionRep _repository;
    private readonly IMapper _mapper;
    public BillingTransactionController(IBillingTransactionRep repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpPost("ExistBillingTransaction_Base")]
    public async Task<ActionResult<BitResultObject>> ExistBillingTransaction_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.ExistBillingTransactionAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpPost("GetAllBillingTransactions_Base")]
    public async Task<ActionResult<ListResultObject<BillingTransactionVM>>> GetAllBillingTransactions_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllBillingTransactionsAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(_mapper.Map<ListResultObject<BillingTransactionVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetBillingTransactionById_Base")]
    public async Task<ActionResult<RowResultObject<BillingTransactionVM>>> GetBillingTransactionById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetBillingTransactionByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(_mapper.Map<RowResultObject<BillingTransactionVM>>(result)) : BadRequest(result);
    }
}

