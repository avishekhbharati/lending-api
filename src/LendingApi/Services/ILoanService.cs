using LendingApi.Models;

namespace LendingApi.Services;

public interface ILoanService
{
    Task<PagedResult<LoanApplication>> GetAllAsync(int page, int pageSize);
    Task<LoanApplication?> GetByIdAsync(Guid id);
    Task<LoanApplication> CreateAsync(CreateLoanRequest request);
}