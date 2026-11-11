using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microbanker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase5_Transactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionRef = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TransactionCodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DrCr = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ValueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TellerRef = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BatchRef = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsReversal = table.Column<bool>(type: "bit", nullable: false),
                    ReversalRef = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    GLAccountNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReversedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReversedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReversalReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.CheckConstraint("CK_Transactions_AccountType", "[AccountType] IN ('SAVINGS','CURRENT','FIXED_DEPOSIT','LOAN')");
                    table.CheckConstraint("CK_Transactions_Amount", "[Amount] > 0");
                    table.CheckConstraint("CK_Transactions_DrCr", "[DrCr] IN ('DR','CR')");
                    table.CheckConstraint("CK_Transactions_Status", "[Status] IN ('PENDING','POSTED','REVERSED')");
                    table.ForeignKey(
                        name: "FK_Transactions_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_TransactionCodes_TransactionCodeId",
                        column: x => x.TransactionCodeId,
                        principalTable: "TransactionCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WithdrawalRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionCodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CompletedTransactionRef = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WithdrawalRequests", x => x.Id);
                    table.CheckConstraint("CK_WithdrawalRequests_Status", "[Status] IN ('PENDING','APPROVED','REJECTED','COMPLETED')");
                    table.ForeignKey(
                        name: "FK_WithdrawalRequests_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WithdrawalRequests_MemberAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "MemberAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WithdrawalRequests_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WithdrawalRequests_TransactionCodes_TransactionCodeId",
                        column: x => x.TransactionCodeId,
                        principalTable: "TransactionCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_MemberAccounts_NoOverdraft",
                table: "MemberAccounts",
                sql: "[BookBalance] >= 0 AND [AvailableBalance] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountId_PostingDate",
                table: "Transactions",
                columns: new[] { "AccountId", "PostingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BranchId",
                table: "Transactions",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_OrganizationId_BatchRef",
                table: "Transactions",
                columns: new[] { "OrganizationId", "BatchRef" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_OrganizationId_TransactionRef",
                table: "Transactions",
                columns: new[] { "OrganizationId", "TransactionRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_OrganizationId_ValueDate",
                table: "Transactions",
                columns: new[] { "OrganizationId", "ValueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionCodeId",
                table: "Transactions",
                column: "TransactionCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_AccountId",
                table: "WithdrawalRequests",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_BranchId",
                table: "WithdrawalRequests",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_OrganizationId_Status",
                table: "WithdrawalRequests",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_TransactionCodeId",
                table: "WithdrawalRequests",
                column: "TransactionCodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "WithdrawalRequests");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MemberAccounts_NoOverdraft",
                table: "MemberAccounts");
        }
    }
}
