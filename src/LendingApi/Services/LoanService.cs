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
}