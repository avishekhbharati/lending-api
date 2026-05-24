using LendingApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


var loans = new List<LoanApplication>
{
    new()
    {
        Id = Guid.NewGuid(),
        ApplicantName = "Avi Bharati",
        Amount = 25000m,
        TermMonths = 36,
        Status = LoanStatus.Draft,
        CreatedAt = DateTime.UtcNow
    },
    new()
    {
        Id = Guid.NewGuid(),
        ApplicantName = "Jane Doe",
        Amount = 50000m,
        TermMonths = 60,
        Status = LoanStatus.Approved,
        CreatedAt = DateTime.UtcNow.AddDays(-7)
    }
};

app.MapGet("/loans", () => loans)
   .WithName("GetLoans");
   
app.Run();