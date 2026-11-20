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
[Route("BillingInvoice")]
[Authorize]
[Produces("application/json")]
public class BillingInvoiceController : ControllerBase
{
    private readonly IBillingInvoiceRep _repository;
    private readonly IMapper _mapper;
    public BillingInvoiceController(IBillingInvoiceRep repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpPost("ExistBillingInvoice_Base")]
    public async Task<ActionResult<BitResultObject>> ExistBillingInvoice_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.ExistBillingInvoiceAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpPost("GetAllBillingInvoices_Base")]
    public async Task<ActionResult<ListResultObject<BillingInvoiceVM>>> GetAllBillingInvoices_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllBillingInvoicesAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(_mapper.Map<ListResultObject<BillingInvoiceVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetBillingInvoiceById_Base")]
    public async Task<ActionResult<RowResultObject<BillingInvoiceVM>>> GetBillingInvoiceById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetBillingInvoiceByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(_mapper.Map<RowResultObject<BillingInvoiceVM>>(result)) : BadRequest(result);
    }
}

