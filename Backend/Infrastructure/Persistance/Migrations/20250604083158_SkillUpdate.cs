using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SkillUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeSkills",
                table: "EmployeeSkills");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Skills",
                type: "char(24)",
                unicode: false,
                fixedLength: true,
                maxLength: 24,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "SkillId",
                table: "EmployeeSkills",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "EmployeeSkills",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "EmployeeSkills",
                type: "char(24)",
                unicode: false,
                fixedLength: true,
                maxLength: 24,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "EmployeeSkills",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FlowCaseCVId",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeSkills",
                table: "EmployeeSkills",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSkills_EmployeeId",
                table: "EmployeeSkills",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeSkills",
                table: "EmployeeSkills");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeSkills_EmployeeId",
                table: "EmployeeSkills");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "EmployeeSkills");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "EmployeeSkills");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "EmployeeSkills");

            migrationBuilder.DropColumn(
                name: "FlowCaseCVId",
                table: "Employees");

            migrationBuilder.AlterColumn<Guid>(
                name: "SkillId",
                table: "EmployeeSkills",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeSkills",
                table: "EmployeeSkills",
                columns: new[] { "EmployeeId", "SkillId" });
        }
    }
}
