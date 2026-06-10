using LendingApi.Models;
using LendingApi.Services;
using Microsoft.AspNetCore.Mvc;
using LendingApi.Common;

namespace LendingApi.Controllers;

[ApiController]
[Route("loans")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<LoanApplication>>> GetLoans(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _loanService.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LoanApplication>> GetLoanById(Guid id)
    {
        var loan = await _loanService.GetByIdAsync(id);
        return loan is null ? NotFound() : Ok(loan);
    }

    [HttpPost]
    public async Task<ActionResult<LoanApplication>> CreateLoan(CreateLoanRequest request)
    {
        var loan = await _loanService.CreateAsync(request);
        return CreatedAtAction(nameof(GetLoanById), new { id = loan.Id }, loan);
    }

    [HttpPost("{id}/submit")]
    public async Task<ActionResult<LoanApplication>> Submit(Guid id)
    {
        var result = await _loanService.SubmitAsync(id);
        return MapToActionResult(result);
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult<LoanApplication>> Approve(Guid id)
    {
        var result = await _loanService.ApproveAsync(id);
        return MapToActionResult(result);
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<LoanApplication>> Reject(Guid id)
    {
        var result = await _loanService.RejectAsync(id);
        return MapToActionResult(result);
    }

    [HttpGet("{id}/repayment")]
    public async Task<ActionResult<RepaymentEstimate>> GetRepayment(Guid id)
    {
        var result = await _loanService.CalculateRepaymentAsync(id);

        if (result.IsSuccess)
            return Ok(result.Value);

        return result.Error switch
        {
            ResultError.NotFound => NotFound(new { error = result.ErrorMessage }),
            ResultError.Conflict => Conflict(new { error = result.ErrorMessage }),
            _ => StatusCode(500, new { error = "Unknown error" })
        };
    }

    private ActionResult<LoanApplication> MapToActionResult(Result<LoanApplication> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return result.Error switch
        {
            ResultError.NotFound => NotFound(new { error = result.ErrorMessage }),
            ResultError.Conflict => Conflict(new { error = result.ErrorMessage }),
            ResultError.Validation => UnprocessableEntity(new { error = result.ErrorMessage }),
            _ => StatusCode(500, new { error = "Unknown error" })
        };
    }
}