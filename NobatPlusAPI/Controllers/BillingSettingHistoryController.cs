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
[Route("BillingSettingHistory")]
[Authorize]
[RequireRole(2, 3, 4)]
[Produces("application/json")]
public class BillingSettingHistoryController : ControllerBase
{
    private readonly IBillingSettingHistoryRep _repository;
    private readonly IMapper _mapper;
    public BillingSettingHistoryController(IBillingSettingHistoryRep repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpPost("GetAllBillingSettingHistories_Base")]
    public async Task<ActionResult<ListResultObject<BillingSettingHistoryVM>>> GetAllBillingSettingHistories_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<ListResultObject<BillingSettingHistoryVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetBillingSettingHistoryById_Base")]
    public async Task<ActionResult<RowResultObject<BillingSettingHistoryVM>>> GetBillingSettingHistoryById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<RowResultObject<BillingSettingHistoryVM>>(result)) : BadRequest(result);
    }
}

