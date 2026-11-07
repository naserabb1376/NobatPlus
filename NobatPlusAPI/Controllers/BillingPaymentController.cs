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
[Route("BillingPayment")]
[Authorize]
[RequireRole(2, 3, 4)]
[Produces("application/json")]
public class BillingPaymentController : ControllerBase
{
    private readonly IBillingPaymentRep _repository;
    private readonly IMapper _mapper;
    public BillingPaymentController(IBillingPaymentRep repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpPost("GetAllBillingPayments_Base")]
    public async Task<ActionResult<ListResultObject<BillingPaymentVM>>> GetAllBillingPayments_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<ListResultObject<BillingPaymentVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetBillingPaymentById_Base")]
    public async Task<ActionResult<RowResultObject<BillingPaymentVM>>> GetBillingPaymentById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<RowResultObject<BillingPaymentVM>>(result)) : BadRequest(result);
    }
}

