using LendingApi.Data;
using LendingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LendingApi.Services;

public class LoanService : ILoanService
{
    private readonly LendingDbContext _db;

    public LoanService(LendingDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<LoanApplication>> GetAllAsync()
    {
        return await _db.LoanApplications.ToListAsync();
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
            CreatedAt = DateTime.UtcNow
        };

        _db.LoanApplications.Add(loan);
        await _db.SaveChangesAsync();
        return loan;
    }
}