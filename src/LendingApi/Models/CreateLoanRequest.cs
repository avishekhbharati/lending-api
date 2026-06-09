using System.ComponentModel.DataAnnotations;

namespace LendingApi.Models;

public record CreateLoanRequest(
    [Required(ErrorMessage = "Applicant name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Applicant name must be between 1 and 200 characters")]
    string ApplicantName,

    decimal Amount,

    int TermMonths
) : IValidatableObject {
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        var results = new List<ValidationResult>();

        if (Amount < 1000)
            results.Add(new ValidationResult("Amount must be at least $1,000", new[] { nameof(Amount) }));
        
        if (Amount > 1_000_000)
            results.Add(new ValidationResult("Amount must not exceed $1,000,000", new[] { nameof(Amount) }));

        if (TermMonths < 1)
            results.Add(new ValidationResult("Term must be at least 1 month", new[] { nameof(TermMonths) }));

        if (TermMonths > 360)
            results.Add(new ValidationResult("Term must not exceed 360 months", new[] { nameof(TermMonths) }));

        return results;
    }
}