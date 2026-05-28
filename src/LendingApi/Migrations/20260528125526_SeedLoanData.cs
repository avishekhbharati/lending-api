using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LendingApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedLoanData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "LoanApplications",
                columns: new[] { "Id", "Amount", "ApplicantName", "CreatedAt", "Status", "TermMonths" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), 25000m, "Avi Bharati", new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 0, 36 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 50000m, "Jane Doe", new DateTime(2026, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), 1, 60 },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 10000m, "Sandip Pandey", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 12 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LoanApplications",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "LoanApplications",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "LoanApplications",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));
        }
    }
}
