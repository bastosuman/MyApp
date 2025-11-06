using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTransferEntities : Migration
    {
        private const string TableAccountLimits = "AccountLimits";
        private const string TableAccounts = "Accounts";
        private const string TableScheduledTransfers = "ScheduledTransfers";
        private const string TableTransfers = "Transfers";
        private const string TypeDecimal = "decimal(18,2)";
        private const string TypeDateTime2 = "datetime2";
        private const string TypeNvarchar50 = "nvarchar(50)";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: TableAccountLimits,
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    DailyTransferLimit = table.Column<decimal>(type: TypeDecimal, precision: 18, scale: 2, nullable: false),
                    MonthlyTransferLimit = table.Column<decimal>(type: TypeDecimal, precision: 18, scale: 2, nullable: false),
                    PerTransactionMax = table.Column<decimal>(type: TypeDecimal, precision: 18, scale: 2, nullable: false),
                    PerTransactionMin = table.Column<decimal>(type: TypeDecimal, precision: 18, scale: 2, nullable: false),
                    LastDailyReset = table.Column<DateTime>(type: TypeDateTime2, nullable: true),
                    LastMonthlyReset = table.Column<DateTime>(type: TypeDateTime2, nullable: true),
                    DailyTransferUsed = table.Column<decimal>(type: TypeDecimal, precision: 18, scale: 2, nullable: false),
                    MonthlyTransferUsed = table.Column<decimal>(type: TypeDecimal, precision: 18, scale: 2, nullable: false),
                    AccountId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountLimits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountLimits_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: TableAccounts,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountLimits_Accounts_AccountId1",
                        column: x => x.AccountId1,
                        principalTable: TableAccounts,
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: TableScheduledTransfers,
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceAccountId = table.Column<int>(type: "int", nullable: false),
                    DestinationAccountId = table.Column<int>(type: "int", nullable: true),
                    DestinationAccountNumber = table.Column<string>(type: TypeNvarchar50, maxLength: 50, nullable: true),
                    TransferType = table.Column<string>(type: TypeNvarchar50, maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: TypeDecimal, precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: TypeDateTime2, nullable: false),
                    RecurrenceType = table.Column<string>(type: TypeNvarchar50, maxLength: 50, nullable: false),
                    RecurrenceDay = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: TypeNvarchar50, maxLength: 50, nullable: false),
                    NextExecutionDate = table.Column<DateTime>(type: TypeDateTime2, nullable: true),
                    LastExecutionDate = table.Column<DateTime>(type: TypeDateTime2, nullable: true),
                    ExecutionCount = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: TypeDateTime2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledTransfers_Accounts_DestinationAccountId",
                        column: x => x.DestinationAccountId,
                        principalTable: TableAccounts,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledTransfers_Accounts_SourceAccountId",
                        column: x => x.SourceAccountId,
                        principalTable: TableAccounts,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: TableTransfers,
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceAccountId = table.Column<int>(type: "int", nullable: false),
                    DestinationAccountId = table.Column<int>(type: "int", nullable: true),
                    DestinationAccountNumber = table.Column<string>(type: TypeNvarchar50, maxLength: 50, nullable: true),
                    TransferType = table.Column<string>(type: TypeNvarchar50, maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: TypeDecimal, precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: TypeNvarchar50, maxLength: 50, nullable: false),
                    TransferDate = table.Column<DateTime>(type: TypeDateTime2, nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: TypeDateTime2, nullable: true),
                    RecurrencePattern = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SourceTransactionId = table.Column<int>(type: "int", nullable: true),
                    DestinationTransactionId = table.Column<int>(type: "int", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: TypeDateTime2, nullable: false),
                    CompletedDate = table.Column<DateTime>(type: TypeDateTime2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transfers_Accounts_DestinationAccountId",
                        column: x => x.DestinationAccountId,
                        principalTable: TableAccounts,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transfers_Accounts_SourceAccountId",
                        column: x => x.SourceAccountId,
                        principalTable: TableAccounts,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transfers_Transactions_DestinationTransactionId",
                        column: x => x.DestinationTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Transfers_Transactions_SourceTransactionId",
                        column: x => x.SourceTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountLimits_AccountId",
                table: TableAccountLimits,
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountLimits_AccountId1",
                table: "AccountLimits",
                column: "AccountId1",
                unique: true,
                filter: "[AccountId1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTransfers_DestinationAccountId",
                table: TableScheduledTransfers,
                column: "DestinationAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTransfers_SourceAccountId",
                table: TableScheduledTransfers,
                column: "SourceAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_DestinationAccountId",
                table: TableTransfers,
                column: "DestinationAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_DestinationTransactionId",
                table: TableTransfers,
                column: "DestinationTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_SourceAccountId",
                table: TableTransfers,
                column: "SourceAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_SourceTransactionId",
                table: TableTransfers,
                column: "SourceTransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: TableAccountLimits);

            migrationBuilder.DropTable(
                name: TableScheduledTransfers);

            migrationBuilder.DropTable(
                name: TableTransfers);
        }
    }
}
