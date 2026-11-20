using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.Expense;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;

namespace NobatPlusAPI.Controllers;

[ApiController, Route("Expense"), Authorize, Produces("application/json")]
public class ExpenseController : ControllerBase
{
    private readonly IExpenseRep _repository;
    private readonly IMapper _mapper;
    public ExpenseController(IExpenseRep repository, IMapper mapper) { _repository = repository; _mapper = mapper; }

    [HttpPost("GetAllExpenses_Base")]
    public async Task<ActionResult<ListResultObject<ExpenseVM>>> GetAllExpenses_Base(GetExpenseListRequestBody requestBody)
    { if (!ModelState.IsValid) return BadRequest(requestBody); var result = await _repository.GetAllExpensesAsync(ProfileId(), User.GetCurrentUserId(), IsAdmin(), requestBody.ExpenseCategoryID, requestBody.FromDate, requestBody.ToDate, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery); return result.Status ? Ok(_mapper.Map<ListResultObject<ExpenseVM>>(result)) : BadRequest(result); }

    [HttpPost("GetExpenseById_Base")]
    public async Task<ActionResult<RowResultObject<ExpenseVM>>> GetExpenseById_Base(GetRowRequestBody requestBody)
    { if (!ModelState.IsValid) return BadRequest(requestBody); var result = await _repository.GetExpenseByIdAsync(requestBody.ID, User.GetCurrentUserId(), IsAdmin()); return result.Status ? Ok(_mapper.Map<RowResultObject<ExpenseVM>>(result)) : BadRequest(result); }

    [HttpPost("ExistExpense_Base")]
    public async Task<ActionResult<BitResultObject>> ExistExpense_Base(GetRowRequestBody requestBody)
    { if (!ModelState.IsValid) return BadRequest(requestBody); var result = await _repository.ExistExpenseAsync(requestBody.ID, User.GetCurrentUserId(), IsAdmin()); return result.Status ? Ok(result) : BadRequest(result); }

    [HttpPost("AddExpense_Base")]
    public async Task<ActionResult<BitResultObject>> AddExpense_Base(AddEditExpenseRequestBody requestBody)
    { if (!ModelState.IsValid) return BadRequest(requestBody); var row = new Expense { OwnerWorkerID = requestBody.OwnerWorkerID, ExpenseCategoryID = requestBody.ExpenseCategoryID, Title = requestBody.Title, AmountIRR = requestBody.AmountIRR, ExpenseDate = requestBody.ExpenseDate.Date, Description = requestBody.Description, CreateDate = DateTime.Now.ToShamsi(), UpdateDate = DateTime.Now.ToShamsi() }; var result = await _repository.AddExpenseAsync(row, User.GetCurrentUserId(), IsAdmin()); return result.Status ? Ok(result) : BadRequest(result); }

    [HttpPut("EditExpense_Base")]
    public async Task<ActionResult<BitResultObject>> EditExpense_Base(AddEditExpenseRequestBody requestBody)
    { if (!ModelState.IsValid) return BadRequest(requestBody); var row = new Expense { ID = requestBody.ID, OwnerWorkerID = requestBody.OwnerWorkerID, ExpenseCategoryID = requestBody.ExpenseCategoryID, Title = requestBody.Title, AmountIRR = requestBody.AmountIRR, ExpenseDate = requestBody.ExpenseDate.Date, Description = requestBody.Description, UpdateDate = DateTime.Now.ToShamsi() }; var result = await _repository.EditExpenseAsync(row, User.GetCurrentUserId(), IsAdmin()); return result.Status ? Ok(result) : BadRequest(result); }

    [HttpDelete("DeleteExpense_Base")]
    public async Task<ActionResult<BitResultObject>> DeleteExpense_Base(GetRowRequestBody requestBody)
    { if (!ModelState.IsValid) return BadRequest(requestBody); var result = await _repository.RemoveExpenseAsync(requestBody.ID, User.GetCurrentUserId(), IsAdmin()); return result.Status ? Ok(result) : BadRequest(result); }

    private bool IsAdmin() => User.GetCurrentRoleId() == (long)DbTools.BaseRole.Admin;
    private long ProfileId() => IsAdmin() ? 0 : User.GetCurrentProfileId() > 0 ? User.GetCurrentProfileId() : User.GetCurrentWorkerId();
}
