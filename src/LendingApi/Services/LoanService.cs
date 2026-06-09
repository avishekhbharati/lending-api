using LendingApi.Common;
using LendingApi.Data;
using LendingApi.Models;
using LendingApi.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LendingApi.Services;

public class LoanService : ILoanService
{
    private readonly LendingDbContext _db;
    private readonly LoanRulesOptions _rules;

    public LoanService(LendingDbContext db, IOptions<LoanRulesOptions> rules)
    {
        _db = db;
        _rules = rules.Value;
    }

    public async Task<PagedResult<LoanApplication>> GetAllAsync(int page, int pageSize)
    {
        var query = _db.LoanApplications.AsQueryable();
        var totalCount = await query.CountAsync();  

        var items = await query
        .OrderBy(l => l.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
        return new PagedResult<LoanApplication>(items, page, pageSize, totalCount);
    }

    public async Task<LoanApplication?> GetByIdAsync(Guid id)
    {
        return await _db.LoanApplications.FindAsync(id);
    }

    public async Task<LoanApplication> CreateAsync(CreateLoanRequest request)
    {
        var loan = new LoanApplication
        {
            Id = Guid.NewGuid(),
            ApplicantName = request.ApplicantName,
            Amount = request.Amount,
            TermMonths = request.TermMonths,
            Status = LoanStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.LoanApplications.Add(loan);
        await _db.SaveChangesAsync();
        return loan;
    }

    public async Task<Result<LoanApplication>> SubmitAsync(Guid id)
    {
        var loan = await _db.LoanApplications.FindAsync(id);

        if (loan is null)
            return Result<LoanApplication>.NotFound($"Loan {id} not found");

        if (loan.Status != LoanStatus.Draft)
            return Result<LoanApplication>.Conflict(
                $"Cannot submit loan in status {loan.Status}; must be Draft");

        if (loan.Amount < _rules.MinAmount || loan.Amount > _rules.MaxAmount)
            return Result<LoanApplication>.Validation(
                $"Amount must be between {_rules.MinAmount:C} and {_rules.MaxAmount:C}");

        if (loan.TermMonths < _rules.MinTermMonths || loan.TermMonths > _rules.MaxTermMonths)
            return Result<LoanApplication>.Validation(
                $"Term must be between {_rules.MinTermMonths} and {_rules.MaxTermMonths} months");

        loan.Status = LoanStatus.Submitted;
        await _db.SaveChangesAsync();

        return Result<LoanApplication>.Success(loan);
    }

    public async Task<Result<LoanApplication>> ApproveAsync(Guid id)
    {
        var loan = await _db.LoanApplications.FindAsync(id);

        if (loan is null)
            return Result<LoanApplication>.NotFound($"Loan {id} not found");

        if (loan.Status != LoanStatus.Submitted)
            return Result<LoanApplication>.Conflict(
                $"Cannot approve loan in status {loan.Status}; must be Submitted");

        loan.Status = LoanStatus.Approved;
        await _db.SaveChangesAsync();

        return Result<LoanApplication>.Success(loan);
    }

    public async Task<Result<LoanApplication>> RejectAsync(Guid id)
    {
        var loan = await _db.LoanApplications.FindAsync(id);

        if (loan is null)
            return Result<LoanApplication>.NotFound($"Loan {id} not found");

        if (loan.Status != LoanStatus.Submitted)
            return Result<LoanApplication>.Conflict(
                $"Cannot reject loan in status {loan.Status}; must be Submitted");

        loan.Status = LoanStatus.Rejected;
        await _db.SaveChangesAsync();

        return Result<LoanApplication>.Success(loan);
    }

}