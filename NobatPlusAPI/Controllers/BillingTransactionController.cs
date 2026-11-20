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
[Route("BillingTransaction")]
[Authorize]
[RequireRole(2, 3, 4)]
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

    [HttpPost("GetAllBillingTransactions_Base")]
    public async Task<ActionResult<ListResultObject<BillingTransactionVM>>> GetAllBillingTransactions_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<ListResultObject<BillingTransactionVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetBillingTransactionById_Base")]
    public async Task<ActionResult<RowResultObject<BillingTransactionVM>>> GetBillingTransactionById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<RowResultObject<BillingTransactionVM>>(result)) : BadRequest(result);
    }
}

