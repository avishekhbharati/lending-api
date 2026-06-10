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