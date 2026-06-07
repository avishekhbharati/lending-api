using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LendingApi.Migrations
{
    /// <inheritdoc />
    public partial class SchemaCleanup : Migration
    {
        /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ApplicantName: simple shrink, EF generated this correctly
        migrationBuilder.AlterColumn<string>(
            name: "ApplicantName",
            table: "LoanApplications",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        // Status: int → string, with explicit data conversion for ALL rows
        // (EF Core's generated AlterColumn would corrupt non-seed rows.)

        // Step 1: add a temporary string column
        migrationBuilder.AddColumn<string>(
            name: "StatusNew",
            table: "LoanApplications",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "Draft");

        // Step 2: copy data across with explicit int → string mapping
        migrationBuilder.Sql(@"
            UPDATE LoanApplications
            SET StatusNew = CASE Status
                WHEN 0 THEN 'Draft'
                WHEN 1 THEN 'Submitted'
                WHEN 2 THEN 'Approved'
                WHEN 3 THEN 'Rejected'
            END;
        ");

        // Step 3: drop the old int column
        migrationBuilder.DropColumn(
            name: "Status",
            table: "LoanApplications");

        // Step 4: rename StatusNew to take its place
        migrationBuilder.RenameColumn(
            name: "StatusNew",
            table: "LoanApplications",
            newName: "Status");
    }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "LoanApplications",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "ApplicantName",
                table: "LoanApplications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.UpdateData(
                table: "LoanApplications",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "Status",
                value: 0);

            migrationBuilder.UpdateData(
                table: "LoanApplications",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "Status",
                value: 1);

            migrationBuilder.UpdateData(
                table: "LoanApplications",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "Status",
                value: 2);
        }
    }
}
