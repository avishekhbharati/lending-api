using LendingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LendingApi.Data;

public class LendingDbContext : DbContext
{
    public LendingDbContext(DbContextOptions<LendingDbContext> options)
        : base(options)
    {
    }

    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LoanApplication>().HasData(
            new LoanApplication
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                ApplicantName = "Avi Bharati",
                Amount = 25000m,
                TermMonths = 36,
                Status = LoanStatus.Draft,
                CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            },
            new LoanApplication
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ApplicantName = "Jane Doe",
                Amount = 50000m,
                TermMonths = 60,
                Status = LoanStatus.Submitted,
                CreatedAt = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc)
            },
            new LoanApplication
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                ApplicantName = "Sandip Pandey",
                Amount = 10000m,
                TermMonths = 12,
                Status = LoanStatus.Approved,
                CreatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}