namespace LendingApi.Models;

public record CreateLoanRequest(string ApplicantName, decimal Amount, int TermMonths);