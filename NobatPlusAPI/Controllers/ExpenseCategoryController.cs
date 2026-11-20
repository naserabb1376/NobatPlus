using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.ExpenseCategory;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;

namespace NobatPlusAPI.Controllers;

[ApiController, Route("ExpenseCategory"), Authorize, Produces("application/json")]
public class ExpenseCategoryController : ControllerBase
{
    private readonly IExpenseCategoryRep _repository;
    private readonly IMapper _mapper;
    public ExpenseCategoryController(IExpenseCategoryRep repository, IMapper mapper) { _repository = repository; _mapper = mapper; }

    [HttpPost("GetAllExpenseCategories_Base")]
    public async Task<ActionResult<ListResultObject<ExpenseCategoryVM>>> GetAllExpenseCategories_Base(GetExpenseCategoryListRequestBody requestBody)
    { if (!ModelState.IsValid) return BadRequest(requestBody); var result = await _repository.GetAllExpenseCategoriesAsync(ProfileId(), User.GetCurrentUserId(), IsAdmin(), requestBody.IncludeInactive, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery); return result.Status ? Ok(_mapper.Map<ListResultObject<ExpenseCategoryVM>>(result)) : BadRequest(result); }

    [HttpPost("GetExpenseCategoryById_Base")]
    public async Task<ActionResult<RowResultObject<ExpenseCategoryVM>>> GetExpenseCategoryById_Base(GetRowRequestBody requestBody)
    { if (!ModelState.IsValid) return BadRequest(requestBody); var result = await _repository.GetExpenseCategoryByIdAsync(requestBody.ID, User.GetCurrentUserId(), IsAdmin()); return result.Status ? Ok(_mapper.Map<RowResultObject<ExpenseCategoryVM>>(result)) : BadRequest(result); }

    [HttpPost("ExistExpenseCategory_Base")]
    public async Task<ActionResult<BitResultObject>> ExistExpenseCategory_Base(GetRowRequestBody requestBody)
    { if (!ModelState.IsValid) return BadRequest(requestBody); var result = await _repository.ExistExpenseCategoryAsync(requestBody.ID, User.GetCurrentUserId(), IsAdmin()); return result.Status ? Ok(result) : BadRequest(result); }

    [HttpPost("AddExpenseCategory_Base")]
    public async Task<ActionResult<BitResultObject>> AddExpenseCategory_Base(AddEditExpenseCategoryRequestBody requestBody)
    { if (!ModelState.IsValid) return BadRequest(requestBody); var row = new ExpenseCategory { Name = requestBody.Name, OwnerWorkerID = requestBody.OwnerWorkerID, IsActive = requestBody.IsActive, Description = requestBody.Description, CreateDate = DateTime.Now.ToShamsi(), UpdateDate = DateTime.Now.ToShamsi() }; var result = await _repository.AddExpenseCategoryAsync(row, User.GetCurrentUserId(), IsAdmin()); return result.Status ? Ok(result) : BadRequest(result); }

    [HttpPut("EditExpenseCategory_Base")]
    public async Task<ActionResult<BitResultObject>> EditExpenseCategory_Base(AddEditExpenseCategoryRequestBody requestBody)
    { if (!ModelState.IsValid) return BadRequest(requestBody); var row = new ExpenseCategory { ID = requestBody.ID, Name = requestBody.Name, OwnerWorkerID = requestBody.OwnerWorkerID, IsActive = requestBody.IsActive, Description = requestBody.Description, UpdateDate = DateTime.Now.ToShamsi() }; var result = await _repository.EditExpenseCategoryAsync(row, User.GetCurrentUserId(), IsAdmin()); return result.Status ? Ok(result) : BadRequest(result); }

    [HttpDelete("DeleteExpenseCategory_Base")]
    public async Task<ActionResult<BitResultObject>> DeleteExpenseCategory_Base(GetRowRequestBody requestBody)
    { if (!ModelState.IsValid) return BadRequest(requestBody); var result = await _repository.RemoveExpenseCategoryAsync(requestBody.ID, User.GetCurrentUserId(), IsAdmin()); return result.Status ? Ok(result) : BadRequest(result); }

    private bool IsAdmin() => User.GetCurrentRoleId() == (long)DbTools.BaseRole.Admin;
    private long ProfileId() => IsAdmin() ? 0 : User.GetCurrentProfileId() > 0 ? User.GetCurrentProfileId() : User.GetCurrentWorkerId();
}
