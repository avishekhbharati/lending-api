using LendingApi.Models;
using LendingApi.Data;
using Microsoft.EntityFrameworkCore;
using LendingApi.Services;
using LendingApi.Options;
using LendingApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<LendingDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("LendingDb")));

builder.Services.AddScoped<ILoanService, LoanService>();

builder.Services.Configure<LoanRulesOptions>(
    builder.Configuration.GetSection(LoanRulesOptions.SectionName));

builder.Services.AddControllers()
  .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

var app = builder.Build();

// Apply migrations on startup, with retry — SQL Server in a container
// may not be ready for DDL even after its healthcheck passes.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LendingDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    const int maxAttempts = 10;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            logger.LogInformation("Applying migrations (attempt {Attempt}/{Max})...", attempt, maxAttempts);
            db.Database.Migrate();
            logger.LogInformation("Migrations applied successfully.");
            break;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning(ex, "Migration attempt {Attempt} failed; retrying in 5s...", attempt);
            Thread.Sleep(5000);
        }
    }
}

// ... rest of pipeline (timing middleware, etc.)

// Timing middleware FIRST so it wraps everything else
app.UseMiddleware<RequestTimingMiddleware>();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();