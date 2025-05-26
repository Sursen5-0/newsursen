using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    HireDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LeaveDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Birthdate = table.Column<DateOnly>(type: "date", nullable: false),
                    BusinessUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkPhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PersonalPhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    HubSpotId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeveraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntraId = table.Column<Guid>(type: "uniqueidentifier", fixedLength: true, maxLength: 255, nullable: false),
                    FlowCaseId = table.Column<string>(type: "char(24)", unicode: false, fixedLength: true, maxLength: 24, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Employees_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Absences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Absences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Absences_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsFinalized = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Probability = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ClosingDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ResponsibleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deals_Employees_ResponsibleId",
                        column: x => x.ResponsibleId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmployeeContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ToDate = table.Column<DateOnly>(type: "date", nullable: true),
                    FromDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpectedHours = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BillableHours = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    SeveraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeContracts_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectEmployees",
                columns: table => new
                {
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectEmployees", x => new { x.EmployeeId, x.ProjectId });
                    table.ForeignKey(
                        name: "FK_ProjectEmployees_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectEmployees_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeSkills",
                columns: table => new
                {
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    YearsExperience = table.Column<decimal>(type: "decimal(3,1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeSkills", x => new { x.EmployeeId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_EmployeeSkills_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LineItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HourlyPrice = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DealId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponsibleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LineItems_Deals_DealId",
                        column: x => x.DealId,
                        principalTable: "Deals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LineItems_Employees_ResponsibleId",
                        column: x => x.ResponsibleId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Allocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Allocations_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Allocations_LineItems_LineItemId",
                        column: x => x.LineItemId,
                        principalTable: "LineItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Allocations_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Opportunities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opportunities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Opportunities_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Opportunities_LineItems_LineItemId",
                        column: x => x.LineItemId,
                        principalTable: "LineItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Absences_EmployeeId",
                table: "Absences",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_EmployeeId",
                table: "Allocations",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_LineItemId",
                table: "Allocations",
                column: "LineItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_ProjectId",
                table: "Allocations",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_ResponsibleId",
                table: "Deals",
                column: "ResponsibleId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeContracts_EmployeeId",
                table: "EmployeeContracts",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ManagerId",
                table: "Employees",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSkills_SkillId",
                table: "EmployeeSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_LineItems_DealId",
                table: "LineItems",
                column: "DealId");

            migrationBuilder.CreateIndex(
                name: "IX_LineItems_ResponsibleId",
                table: "LineItems",
                column: "ResponsibleId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_EmployeeId",
                table: "Opportunities",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_LineItemId",
                table: "Opportunities",
                column: "LineItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectEmployees_ProjectId",
                table: "ProjectEmployees",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Absences");

            migrationBuilder.DropTable(
                name: "Allocations");

            migrationBuilder.DropTable(
                name: "EmployeeContracts");

            migrationBuilder.DropTable(
                name: "EmployeeSkills");

            migrationBuilder.DropTable(
                name: "Opportunities");

            migrationBuilder.DropTable(
                name: "ProjectEmployees");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "LineItems");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Deals");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
