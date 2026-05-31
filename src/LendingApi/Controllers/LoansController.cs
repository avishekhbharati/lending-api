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
    public async Task<ActionResult<IEnumerable<LoanApplication>>> GetLoans()
    {
        var loans = await _loanService.GetAllAsync();
        return Ok(loans);
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