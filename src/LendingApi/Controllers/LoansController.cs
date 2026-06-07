using LendingApi.Models;
using LendingApi.Services;
using Microsoft.AspNetCore.Mvc;

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
}