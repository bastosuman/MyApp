using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        private const string TableAccounts = "Accounts";
        private const string SqlServerIdentity = "SqlServer:Identity";
        private const string TypeNvarcharMax = "nvarchar(max)";
        private const string TypeDateTime2 = "datetime2";
        private const string TypeDecimal182 = "decimal(18,2)";
        private const string TypeDecimal52 = "decimal(5,2)";
        
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: TableAccounts,
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation(SqlServerIdentity, "1, 1"),
                    AccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: TypeNvarcharMax, nullable: false),
                    DateCreated = table.Column<DateTime>(type: TypeDateTime2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation(SqlServerIdentity, "1, 1"),
                    Name = table.Column<string>(type: TypeNvarcharMax, nullable: false),
                    ProductType = table.Column<string>(type: TypeNvarcharMax, nullable: false),
                    MinAmount = table.Column<decimal>(type: TypeDecimal182, nullable: false),
                    MaxAmount = table.Column<decimal>(type: TypeDecimal182, nullable: false),
                    InterestRate = table.Column<decimal>(type: TypeDecimal52, nullable: false),
                    MinTermMonths = table.Column<int>(type: "int", nullable: false),
                    MaxTermMonths = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation(SqlServerIdentity, "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    ApplicationType = table.Column<string>(type: TypeNvarcharMax, nullable: false),
                    RequestedAmount = table.Column<decimal>(type: TypeDecimal182, nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: TypeDecimal182, nullable: true),
                    InterestRate = table.Column<decimal>(type: TypeDecimal52, nullable: false),
                    TermMonths = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: TypeNvarcharMax, nullable: false),
                    ApplicationDate = table.Column<DateTime>(type: TypeDateTime2, nullable: false),
                    DecisionDate = table.Column<DateTime>(type: TypeDateTime2, nullable: true),
                    Notes = table.Column<string>(type: TypeNvarcharMax, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: TableAccounts,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation(SqlServerIdentity, "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<string>(type: TypeNvarcharMax, nullable: false),
                    Amount = table.Column<decimal>(type: TypeDecimal182, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: TypeDateTime2, nullable: false),
                    Description = table.Column<string>(type: TypeNvarcharMax, nullable: false),
                    Status = table.Column<string>(type: TypeNvarcharMax, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: TableAccounts,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccountNumber",
                table: TableAccounts,
                column: "AccountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Email",
                table: TableAccounts,
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_AccountId",
                table: "Applications",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountId",
                table: "Transactions",
                column: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: TableAccounts);
        }
    }
}
