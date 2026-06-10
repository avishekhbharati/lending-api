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
    private readonly ILogger<LoanService> _logger;


    public LoanService(LendingDbContext db, IOptions<LoanRulesOptions> rules, ILogger<LoanService> logger)
    {
        _db = db;
        _rules = rules.Value;
        _logger = logger;
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
        {
            _logger.LogWarning("Submit failed: loan {LoanId} not found", id);
            return Result<LoanApplication>.NotFound($"Loan {id} not found");
        }

        if (loan.Status != LoanStatus.Draft)
        {
            _logger.LogWarning(
                "Submit failed: loan {LoanId} is in status {ActualStatus}, expected Draft",
                id, loan.Status);
            return Result<LoanApplication>.Conflict(
                $"Cannot submit loan in status {loan.Status}; must be Draft");
        }

        if (loan.Amount < _rules.MinAmount || loan.Amount > _rules.MaxAmount)
        {
            _logger.LogWarning(
                "Submit failed: loan {LoanId} amount {Amount} outside allowed range {MinAmount}-{MaxAmount}",
                id, loan.Amount, _rules.MinAmount, _rules.MaxAmount);
            return Result<LoanApplication>.Validation(
                $"Amount must be between {_rules.MinAmount:C} and {_rules.MaxAmount:C}");
        }

        if (loan.TermMonths < _rules.MinTermMonths || loan.TermMonths > _rules.MaxTermMonths)
        {
            _logger.LogWarning(
                "Submit failed: loan {LoanId} term {TermMonths} outside allowed range {MinTerm}-{MaxTerm}",
                id, loan.TermMonths, _rules.MinTermMonths, _rules.MaxTermMonths);
            return Result<LoanApplication>.Validation(
                $"Term must be between {_rules.MinTermMonths} and {_rules.MaxTermMonths} months");
        }

        loan.Status = LoanStatus.Submitted;
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Loan {LoanId} submitted by {ApplicantName} for {Amount} over {TermMonths} months",
            loan.Id, loan.ApplicantName, loan.Amount, loan.TermMonths);

        return Result<LoanApplication>.Success(loan);
    }

    public async Task<Result<LoanApplication>> ApproveAsync(Guid id)
    {
        var loan = await _db.LoanApplications.FindAsync(id);

        if (loan is null)
        {
            _logger.LogWarning("Approve failed: loan {LoanId} not found", id);
            return Result<LoanApplication>.NotFound($"Loan {id} not found");
        }

        if (loan.Status != LoanStatus.Submitted)
        {
            _logger.LogWarning(
                "Approve failed: loan {LoanId} is in status {ActualStatus}, expected Submitted",
                id, loan.Status);
            return Result<LoanApplication>.Conflict(
                $"Cannot approve loan in status {loan.Status}; must be Submitted");
        }

        loan.Status = LoanStatus.Approved;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Loan {LoanId} approved (amount {Amount})", loan.Id, loan.Amount);

        return Result<LoanApplication>.Success(loan);
    }

    public async Task<Result<LoanApplication>> RejectAsync(Guid id)
    {
        var loan = await _db.LoanApplications.FindAsync(id);

        if (loan is null)
        {
            _logger.LogWarning("Reject failed: loan {LoanId} not found", id);
            return Result<LoanApplication>.NotFound($"Loan {id} not found");
        }

        if (loan.Status != LoanStatus.Submitted)
        {
            _logger.LogWarning(
                "Reject failed: loan {LoanId} is in status {ActualStatus}, expected Submitted",
                id, loan.Status);
            return Result<LoanApplication>.Conflict(
                $"Cannot reject loan in status {loan.Status}; must be Submitted");
        }

        loan.Status = LoanStatus.Rejected;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Loan {LoanId} rejected", loan.Id);

        return Result<LoanApplication>.Success(loan);
    }

    public async Task<Result<RepaymentEstimate>> CalculateRepaymentAsync(Guid id)
    {
        var loan = await _db.LoanApplications.FindAsync(id);

        if (loan is null)
            return Result<RepaymentEstimate>.NotFound($"Loan {id} not found");

        if (loan.Status != LoanStatus.Approved)
            return Result<RepaymentEstimate>.Conflict(
                $"Repayment is only available for approved loans; this loan is {loan.Status}");

        var monthlyRate = _rules.AnnualInterestRate / 12m;
        var n = loan.TermMonths;

        // Standard amortization formula:
        //   M = P * (r * (1+r)^n) / ((1+r)^n - 1)
        // where M = monthly payment, P = principal, r = monthly rate, n = term in months.
        var compounded = (decimal)Math.Pow((double)(1 + monthlyRate), n);
        var monthlyPayment = loan.Amount * (monthlyRate * compounded) / (compounded - 1);

        monthlyPayment = Math.Round(monthlyPayment, 2);
        var totalRepayment = Math.Round(monthlyPayment * n, 2);
        var totalInterest = Math.Round(totalRepayment - loan.Amount, 2);

        var estimate = new RepaymentEstimate(monthlyPayment, totalRepayment, totalInterest);

        return Result<RepaymentEstimate>.Success(estimate);
    }

}