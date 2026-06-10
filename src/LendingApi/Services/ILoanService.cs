using LendingApi.Common;
using LendingApi.Models;

namespace LendingApi.Services;

public interface ILoanService
{
    Task<PagedResult<LoanApplication>> GetAllAsync(int page, int pageSize);
    Task<LoanApplication?> GetByIdAsync(Guid id);
    Task<LoanApplication> CreateAsync(CreateLoanRequest request);
    Task<Result<LoanApplication>> SubmitAsync(Guid id);
    Task<Result<LoanApplication>> ApproveAsync(Guid id);
    Task<Result<LoanApplication>> RejectAsync(Guid id);
    Task<Result<RepaymentEstimate>> CalculateRepaymentAsync(Guid id);
}