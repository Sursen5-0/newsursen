using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillableHours",
                table: "EmployeeContracts");

            migrationBuilder.AlterColumn<Guid>(
                name: "SeveraId",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "HubSpotId",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FlowCaseId",
                table: "Employees",
                type: "char(24)",
                unicode: false,
                fixedLength: true,
                maxLength: 24,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "char(24)",
                oldUnicode: false,
                oldFixedLength: true,
                oldMaxLength: 24);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Employees",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Employees");

            migrationBuilder.AlterColumn<Guid>(
                name: "SeveraId",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HubSpotId",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FlowCaseId",
                table: "Employees",
                type: "char(24)",
                unicode: false,
                fixedLength: true,
                maxLength: 24,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "char(24)",
                oldUnicode: false,
                oldFixedLength: true,
                oldMaxLength: 24,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BillableHours",
                table: "EmployeeContracts",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
