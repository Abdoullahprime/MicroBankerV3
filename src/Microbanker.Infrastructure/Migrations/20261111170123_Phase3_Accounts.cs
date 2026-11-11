using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microbanker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase3_Accounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemberAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountNo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AccountType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BookBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvailableBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BlockedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnauthorizedDR = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnauthorizedCR = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyDRTurnover = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyCRTurnover = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestAccruedDR = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestAccruedCR = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LastDRDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastCRDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastDRActivityDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastCRActivityDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AtmAccountNo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    AtmDailyLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubSectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GLAssetControl = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    GLLiabilityControl = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    GLIncomeAccount = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    GLExpenseAccount = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    GLChargeAccount = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    GLFeeAccount = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EditRequestPending = table.Column<bool>(type: "bit", nullable: false),
                    EditRequestReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OpenDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConcurrencyStamp = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberAccounts", x => x.Id);
                    table.CheckConstraint("CK_MemberAccounts_AccountType", "[AccountType] IN ('SAVINGS','CURRENT','FIXED_DEPOSIT')");
                    table.CheckConstraint("CK_MemberAccounts_Blocked", "[BlockedAmount] >= 0");
                    table.CheckConstraint("CK_MemberAccounts_Status", "[Status] IN ('PENDING','ACTIVE','DORMANT','FROZEN','CLOSED')");
                    table.ForeignKey(
                        name: "FK_MemberAccounts_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberAccounts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberAccounts_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberAccounts_ProductDefinitions_ProductDefinitionId",
                        column: x => x.ProductDefinitionId,
                        principalTable: "ProductDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberAccounts_Sectors_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Sectors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MemberAccounts_SubSectors_SubSectorId",
                        column: x => x.SubSectorId,
                        principalTable: "SubSectors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AccountEditRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReviewedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountEditRequests", x => x.Id);
                    table.CheckConstraint("CK_AccountEditRequests_Status", "[Status] IN ('PENDING','APPROVED','REJECTED','COMPLETED')");
                    table.ForeignKey(
                        name: "FK_AccountEditRequests_MemberAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "MemberAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountEditRequests_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountEditRequests_AccountId_Status",
                table: "AccountEditRequests",
                columns: new[] { "AccountId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountEditRequests_OrganizationId",
                table: "AccountEditRequests",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberAccounts_BranchId",
                table: "MemberAccounts",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberAccounts_CustomerId",
                table: "MemberAccounts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberAccounts_OrganizationId_AccountNo",
                table: "MemberAccounts",
                columns: new[] { "OrganizationId", "AccountNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberAccounts_OrganizationId_Status",
                table: "MemberAccounts",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MemberAccounts_ProductDefinitionId",
                table: "MemberAccounts",
                column: "ProductDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberAccounts_SectorId",
                table: "MemberAccounts",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberAccounts_SubSectorId",
                table: "MemberAccounts",
                column: "SubSectorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountEditRequests");

            migrationBuilder.DropTable(
                name: "MemberAccounts");
        }
    }
}
