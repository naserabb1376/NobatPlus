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
[Route("BillingSettingHistory")]
[Authorize]
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

    [HttpPost("ExistBillingSettingHistory_Base")]
    public async Task<ActionResult<BitResultObject>> ExistBillingSettingHistory_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.ExistBillingSettingHistoryAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpPost("GetAllBillingSettingHistories_Base")]
    public async Task<ActionResult<ListResultObject<BillingSettingHistoryVM>>> GetAllBillingSettingHistories_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllBillingSettingHistoriesAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(_mapper.Map<ListResultObject<BillingSettingHistoryVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetBillingSettingHistoryById_Base")]
    public async Task<ActionResult<RowResultObject<BillingSettingHistoryVM>>> GetBillingSettingHistoryById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetBillingSettingHistoryByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(_mapper.Map<RowResultObject<BillingSettingHistoryVM>>(result)) : BadRequest(result);
    }
}

