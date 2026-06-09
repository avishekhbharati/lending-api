namespace LendingApi.Options;

public class LoanRulesOptions
{
    public const string SectionName = "LoanRules";

    public decimal MinAmount { get; set; } = 1000;
    public decimal MaxAmount { get; set; } = 1_000_000;
    public int MinTermMonths { get; set; } = 1;
    public int MaxTermMonths { get; set; } = 360;
}