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
[Route("BillingInvoiceItem")]
[Authorize]
[RequireRole(2, 3, 4)]
[Produces("application/json")]
public class BillingInvoiceItemController : ControllerBase
{
    private readonly IBillingInvoiceItemRep _repository;
    private readonly IMapper _mapper;
    public BillingInvoiceItemController(IBillingInvoiceItemRep repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpPost("GetAllBillingInvoiceItems_Base")]
    public async Task<ActionResult<ListResultObject<BillingInvoiceItemVM>>> GetAllBillingInvoiceItems_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<ListResultObject<BillingInvoiceItemVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetBillingInvoiceItemById_Base")]
    public async Task<ActionResult<RowResultObject<BillingInvoiceItemVM>>> GetBillingInvoiceItemById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<RowResultObject<BillingInvoiceItemVM>>(result)) : BadRequest(result);
    }
}

