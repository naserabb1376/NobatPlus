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
[Route("BillingPayment")]
[Authorize]
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

    [HttpPost("ExistBillingPayment_Base")]
    public async Task<ActionResult<BitResultObject>> ExistBillingPayment_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.ExistBillingPaymentAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpPost("GetAllBillingPayments_Base")]
    public async Task<ActionResult<ListResultObject<BillingPaymentVM>>> GetAllBillingPayments_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllBillingPaymentsAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(_mapper.Map<ListResultObject<BillingPaymentVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetBillingPaymentById_Base")]
    public async Task<ActionResult<RowResultObject<BillingPaymentVM>>> GetBillingPaymentById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetBillingPaymentByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(_mapper.Map<RowResultObject<BillingPaymentVM>>(result)) : BadRequest(result);
    }
}

