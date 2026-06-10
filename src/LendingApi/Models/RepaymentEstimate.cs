namespace LendingApi.Models;

public record RepaymentEstimate(
    decimal MonthlyPayment,
    decimal TotalRepayment,
    decimal TotalInterest);