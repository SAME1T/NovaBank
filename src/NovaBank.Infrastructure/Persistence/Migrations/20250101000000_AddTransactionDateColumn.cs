using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaBank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionDateColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TransactionDate",
                table: "bank_transactions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.CreateIndex(
                name: "IX_bank_transactions_TransactionDate",
                table: "bank_transactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_bank_transactions_AccountId_TransactionDate",
                table: "bank_transactions",
                columns: new[] { "AccountId", "TransactionDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_bank_transactions_AccountId_TransactionDate",
                table: "bank_transactions");

            migrationBuilder.DropIndex(
                name: "IX_bank_transactions_TransactionDate",
                table: "bank_transactions");

            migrationBuilder.DropColumn(
                name: "TransactionDate",
                table: "bank_transactions");
        }
    }
}
