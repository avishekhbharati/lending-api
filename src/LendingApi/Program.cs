using LendingApi.Models;
using LendingApi.Data;
using Microsoft.EntityFrameworkCore;
using LendingApi.Services;
using LendingApi.Options;

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();