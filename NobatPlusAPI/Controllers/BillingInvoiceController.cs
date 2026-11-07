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
[Route("BillingInvoice")]
[Authorize]
[RequireRole(2, 3, 4)]
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

    [HttpPost("GetAllBillingInvoices_Base")]
    public async Task<ActionResult<ListResultObject<BillingInvoiceVM>>> GetAllBillingInvoices_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<ListResultObject<BillingInvoiceVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetBillingInvoiceById_Base")]
    public async Task<ActionResult<RowResultObject<BillingInvoiceVM>>> GetBillingInvoiceById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<RowResultObject<BillingInvoiceVM>>(result)) : BadRequest(result);
    }
}

