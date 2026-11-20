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
[Route("BillingInvoiceItem")]
[Authorize]
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

    [HttpPost("ExistBillingInvoiceItem_Base")]
    public async Task<ActionResult<BitResultObject>> ExistBillingInvoiceItem_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.ExistBillingInvoiceItemAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpPost("GetAllBillingInvoiceItems_Base")]
    public async Task<ActionResult<ListResultObject<BillingInvoiceItemVM>>> GetAllBillingInvoiceItems_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllBillingInvoiceItemsAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(_mapper.Map<ListResultObject<BillingInvoiceItemVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetBillingInvoiceItemById_Base")]
    public async Task<ActionResult<RowResultObject<BillingInvoiceItemVM>>> GetBillingInvoiceItemById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetBillingInvoiceItemByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(_mapper.Map<RowResultObject<BillingInvoiceItemVM>>(result)) : BadRequest(result);
    }
}

