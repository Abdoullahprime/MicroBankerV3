using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microbanker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase6_GL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "InterestAccruedDR",
                table: "MemberAccounts",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "InterestAccruedCR",
                table: "MemberAccounts",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateTable(
                name: "EODLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ProcessType = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RunBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EODLogs", x => x.Id);
                    table.CheckConstraint("CK_EODLogs_Status", "[Status] IN ('RUNNING','COMPLETED','FAILED')");
                    table.CheckConstraint("CK_EODLogs_Type", "[ProcessType] IN ('SOD','EOD','EOM','EOY')");
                    table.ForeignKey(
                        name: "FK_EODLogs_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GLEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JournalRef = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    GLAccountNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DrCr = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ValueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModule = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SourceRef = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FiscalPeriod = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GLEntries", x => x.Id);
                    table.CheckConstraint("CK_GLEntries_Amount", "[Amount] > 0");
                    table.CheckConstraint("CK_GLEntries_DrCr", "[DrCr] IN ('DR','CR')");
                    table.CheckConstraint("CK_GLEntries_Source", "[SourceModule] IN ('TELLER','LOAN','SYSTEM')");
                    table.CheckConstraint("CK_GLEntries_Status", "[Status] IN ('POSTED','REVERSED')");
                    table.ForeignKey(
                        name: "FK_GLEntries_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcessingDates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LastSODAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastEODAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastEOMAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastEOYAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EODStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClosedThroughPeriod = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    ConcurrencyStamp = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessingDates", x => x.Id);
                    table.CheckConstraint("CK_ProcessingDates_Status", "[EODStatus] IN ('OPEN','EOD_RUNNING','CLOSED')");
                    table.ForeignKey(
                        name: "FK_ProcessingDates_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EODLogs_OrganizationId_ProcessType_ProcessDate",
                table: "EODLogs",
                columns: new[] { "OrganizationId", "ProcessType", "ProcessDate" });

            migrationBuilder.CreateIndex(
                name: "IX_GLEntries_OrganizationId_FiscalPeriod",
                table: "GLEntries",
                columns: new[] { "OrganizationId", "FiscalPeriod" });

            migrationBuilder.CreateIndex(
                name: "IX_GLEntries_OrganizationId_GLAccountNo_ValueDate",
                table: "GLEntries",
                columns: new[] { "OrganizationId", "GLAccountNo", "ValueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_GLEntries_OrganizationId_JournalRef",
                table: "GLEntries",
                columns: new[] { "OrganizationId", "JournalRef" });

            migrationBuilder.CreateIndex(
                name: "IX_GLEntries_OrganizationId_SourceRef",
                table: "GLEntries",
                columns: new[] { "OrganizationId", "SourceRef" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingDates_OrganizationId",
                table: "ProcessingDates",
                column: "OrganizationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EODLogs");

            migrationBuilder.DropTable(
                name: "GLEntries");

            migrationBuilder.DropTable(
                name: "ProcessingDates");

            migrationBuilder.AlterColumn<decimal>(
                name: "InterestAccruedDR",
                table: "MemberAccounts",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "InterestAccruedCR",
                table: "MemberAccounts",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");
        }
    }
}
