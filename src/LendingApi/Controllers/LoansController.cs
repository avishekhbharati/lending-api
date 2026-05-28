using LendingApi.Data;
using LendingApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LendingApi.Controllers;

[ApiController]
[Route("loans")]
public class LoansController : ControllerBase
{
    private readonly LendingDbContext _db;

    public LoansController(LendingDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoanApplication>>> GetLoans()
    {
        var loans = await _db.LoanApplications.ToListAsync();
        return Ok(loans);
    }

    [HttpPost]
    public async Task<ActionResult<LoanApplication>> CreateLoan(CreateLoanRequest request)
    {
        var loan = new LoanApplication
        {
            Id = Guid.NewGuid(),
            ApplicantName = request.ApplicantName,
            Amount = request.Amount,
            TermMonths = request.TermMonths,
            Status = LoanStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        _db.LoanApplications.Add(loan);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLoanById), new {id =loan.Id}, loan);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LoanApplication>> GetLoanById(Guid id)
    {
        var loan = await _db.LoanApplications.FindAsync(id);
        if (loan is null)
        {
            return NotFound();
        }
        return Ok(loan);
    }
}