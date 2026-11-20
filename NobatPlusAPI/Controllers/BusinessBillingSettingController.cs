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
[Route("BusinessBillingSetting")]
[Authorize]
[RequireRole(2, 3, 4)]
[Produces("application/json")]
public class BusinessBillingSettingController : ControllerBase
{
    private readonly IBusinessBillingSettingRep _repository;
    private readonly IMapper _mapper;
    public BusinessBillingSettingController(IBusinessBillingSettingRep repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpPost("GetAllBusinessBillingSettings_Base")]
    public async Task<ActionResult<ListResultObject<BusinessBillingSettingVM>>> GetAllBusinessBillingSettings_Base(GetBillingListRequestBody requestBody)
    {
        var result = await _repository.GetAllAsync(requestBody, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<ListResultObject<BusinessBillingSettingVM>>(result)) : BadRequest(result);
    }

    [HttpPost("GetBusinessBillingSettingById_Base")]
    public async Task<ActionResult<RowResultObject<BusinessBillingSettingVM>>> GetBusinessBillingSettingById_Base(GetRowRequestBody requestBody)
    {
        var result = await _repository.GetByIdAsync(requestBody.ID, User.GetCurrentUserId(), User.GetCurrentRoleId() == 4);
        return result.Status ? Ok(_mapper.Map<RowResultObject<BusinessBillingSettingVM>>(result)) : BadRequest(result);
    }
}

