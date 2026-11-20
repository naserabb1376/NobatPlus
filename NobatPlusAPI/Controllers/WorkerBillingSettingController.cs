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
[Route("WorkerBillingSetting")]
[Authorize]
[RequireRole(2, 3, 4)]
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

    [HttpPost("GetAllWorkerBillingSettings_Base")]
    public async Task<ActionResult<ListResultObject<WorkerBillingSettingVM>>> GetAllWorkerBillingSettings_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<ListResultObject<WorkerBillingSettingVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetWorkerBillingSettingById_Base")]
    public async Task<ActionResult<RowResultObject<WorkerBillingSettingVM>>> GetWorkerBillingSettingById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<RowResultObject<WorkerBillingSettingVM>>(result)) : BadRequest(result);
    }
}

