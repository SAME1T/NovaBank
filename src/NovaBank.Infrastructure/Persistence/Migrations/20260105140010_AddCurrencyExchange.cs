using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaBank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyExchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "currency_positions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    average_cost_rate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    total_cost_try = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currency_positions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "currency_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_type = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    exchange_rate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    rate_type = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    rate_source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    rate_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    try_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    commission_try = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    net_try_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    from_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    position_before_amount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    position_after_amount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    avg_cost_before = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    avg_cost_after = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    realized_pnl_try = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    realized_pnl_percent = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    reference_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currency_transactions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_currency_positions_customer",
                table: "currency_positions",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_currency_positions_customer_currency",
                table: "currency_positions",
                columns: new[] { "customer_id", "currency" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_currency_transactions_customer",
                table: "currency_transactions",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_currency_transactions_date",
                table: "currency_transactions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_currency_transactions_reference",
                table: "currency_transactions",
                column: "reference_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_currency_transactions_type",
                table: "currency_transactions",
                column: "transaction_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "currency_positions");

            migrationBuilder.DropTable(
                name: "currency_transactions");
        }
    }
}
