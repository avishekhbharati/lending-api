namespace LendingApi.Models;
public class LoanApplication
{
    public Guid Id { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int TermMonths { get; set; }
    public LoanStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}