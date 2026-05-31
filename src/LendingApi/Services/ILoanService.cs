using LendingApi.Models;

namespace LendingApi.Services;

public interface ILoanService
{
    Task<IEnumerable<LoanApplication>> GetAllAsync();
    Task<LoanApplication?> GetByIdAsync(Guid id);
    Task<LoanApplication> CreateAsync(CreateLoanRequest request);
}