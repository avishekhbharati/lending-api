using FluentAssertions;
using LendingApi.Common;
using LendingApi.Data;
using LendingApi.Models;
using LendingApi.Options;
using LendingApi.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LendingApi.Tests;

public class LoanServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LendingDbContext _db;
    private readonly LoanService _service;

    public LoanServiceTests()
    {
        // 1. Open an in-memory SQLite connection and KEEP IT OPEN.
        //    The database lives only as long as this connection is open.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // 2. Point a DbContext at that open connection.
        var options = new DbContextOptionsBuilder<LendingDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new LendingDbContext(options);

        // 3. Create the schema (no migrations — just build tables from the model).
        _db.Database.EnsureCreated();

        // 4. Build the service with real DbContext, real rules, no-op logger.
        var rules = Microsoft.Extensions.Options.Options.Create(new LoanRulesOptions());
        var logger = NullLogger<LoanService>.Instance;
        _service = new LoanService(_db, rules, logger);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();   // closing the connection destroys the in-memory DB
    }

    // Tests go here
    [Fact]
    public async Task CreateAsync_PersistsLoanAsDraft()
    {
        // Arrange
        var request = new CreateLoanRequest("Test Applicant", 20000m, 24);

        // Act
        var loan = await _service.CreateAsync(request);

        // Assert
        loan.Id.Should().NotBeEmpty();
        loan.Status.Should().Be(LoanStatus.Draft);
        loan.ApplicantName.Should().Be("Test Applicant");

        // Confirm it actually persisted to the database
        var fromDb = await _db.LoanApplications.FindAsync(loan.Id);
        fromDb.Should().NotBeNull();
        fromDb!.Amount.Should().Be(20000m);
    }

    [Fact]
    public async Task SubmitAsync_FromDraft_TransitionsToSubmitted()
    {
        // Arrange
        var loan = await SeedLoanAsync(LoanStatus.Draft);

        // Act
        var result = await _service.SubmitAsync(loan.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(LoanStatus.Submitted);
    }

    [Fact]
    public async Task ApproveAsync_FromSubmitted_TransitionsToApproved()
    {
        var loan = await SeedLoanAsync(LoanStatus.Submitted);

        var result = await _service.ApproveAsync(loan.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(LoanStatus.Approved);
    }

    [Fact]
    public async Task RejectAsync_FromSubmitted_TransitionsToRejected()
    {
        var loan = await SeedLoanAsync(LoanStatus.Submitted);

        var result = await _service.RejectAsync(loan.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(LoanStatus.Rejected);
    }

    [Fact]
    public async Task SubmitAsync_WhenLoanNotFound_ReturnsNotFound()
    {
        var result = await _service.SubmitAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ResultError.NotFound);
    }

    [Theory]
    [InlineData(LoanStatus.Draft)]
    [InlineData(LoanStatus.Approved)]
    [InlineData(LoanStatus.Rejected)]
    public async Task ApproveAsync_FromNonSubmittedState_ReturnsConflict(LoanStatus startingStatus)
    {
        var loan = await SeedLoanAsync(startingStatus);

        var result = await _service.ApproveAsync(loan.Id);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ResultError.Conflict);
    }

    [Theory]
    [InlineData(LoanStatus.Draft)]
    [InlineData(LoanStatus.Approved)]
    [InlineData(LoanStatus.Rejected)]
    public async Task RejectAsync_FromNonSubmittedState_ReturnsConflict(LoanStatus startingStatus)
    {
        var loan = await SeedLoanAsync(startingStatus);

        var result = await _service.RejectAsync(loan.Id);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ResultError.Conflict);
    }

    [Theory]
    [InlineData(LoanStatus.Submitted)]
    [InlineData(LoanStatus.Approved)]
    [InlineData(LoanStatus.Rejected)]
    public async Task SubmitAsync_FromNonDraftState_ReturnsConflict(LoanStatus startingStatus)
    {
        var loan = await SeedLoanAsync(startingStatus);

        var result = await _service.SubmitAsync(loan.Id);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ResultError.Conflict);
    }

    //helper
    private async Task<LoanApplication> SeedLoanAsync(LoanStatus status, decimal amount = 20000m, int termMonths = 24)
    {
        var loan = new LoanApplication
        {
            Id = Guid.NewGuid(),
            ApplicantName = "Seed Applicant",
            Amount = amount,
            TermMonths = termMonths,
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.LoanApplications.Add(loan);
        await _db.SaveChangesAsync();
        return loan;
    }
}