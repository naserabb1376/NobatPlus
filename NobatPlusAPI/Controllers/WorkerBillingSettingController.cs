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
[Route("WorkerBillingSetting")]
[Authorize]
[Produces("application/json")]
public class WorkerBillingSettingController : ControllerBase
{
    private readonly IWorkerBillingSettingRep _repository;
    private readonly IMapper _mapper;
    public WorkerBillingSettingController(IWorkerBillingSettingRep repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpPost("ExistWorkerBillingSetting_Base")]
    public async Task<ActionResult<BitResultObject>> ExistWorkerBillingSetting_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.ExistWorkerBillingSettingAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpPost("GetAllWorkerBillingSettings_Base")]
    public async Task<ActionResult<ListResultObject<WorkerBillingSettingVM>>> GetAllWorkerBillingSettings_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllWorkerBillingSettingsAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(_mapper.Map<ListResultObject<WorkerBillingSettingVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetWorkerBillingSettingById_Base")]
    public async Task<ActionResult<RowResultObject<WorkerBillingSettingVM>>> GetWorkerBillingSettingById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetWorkerBillingSettingByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin);
        return result.Status ? Ok(_mapper.Map<RowResultObject<WorkerBillingSettingVM>>(result)) : BadRequest(result);
    }
}

