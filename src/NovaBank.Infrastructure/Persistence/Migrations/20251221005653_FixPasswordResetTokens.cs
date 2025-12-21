using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaBank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixPasswordResetTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReversalOfTransferId",
                table: "bank_transfers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReversedAt",
                table: "bank_transfers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReversedByTransferId",
                table: "bank_transfers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "bank_password_reset_tokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.Sql("""
CREATE INDEX IF NOT EXISTS "IX_bank_transfers_ReversalOfTransferId"
ON bank_transfers ("ReversalOfTransferId");
""");

            migrationBuilder.Sql("""
CREATE UNIQUE INDEX IF NOT EXISTS "IX_bank_transfers_ReversedByTransferId"
ON bank_transfers ("ReversedByTransferId");
""");

            migrationBuilder.Sql("""
CREATE INDEX IF NOT EXISTS "IX_bank_password_reset_tokens_CustomerId_IsUsed_ExpiresAt"
ON bank_password_reset_tokens ("CustomerId", "IsUsed", "ExpiresAt");
""");

            migrationBuilder.Sql("""
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint 
        WHERE conname = 'FK_bank_password_reset_tokens_bank_customers_CustomerId'
    ) THEN
        ALTER TABLE bank_password_reset_tokens
        ADD CONSTRAINT "FK_bank_password_reset_tokens_bank_customers_CustomerId"
        FOREIGN KEY ("CustomerId") REFERENCES bank_customers ("Id") ON DELETE CASCADE;
    END IF;
END $$;
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""ALTER TABLE bank_password_reset_tokens DROP CONSTRAINT IF EXISTS "FK_bank_password_reset_tokens_bank_customers_CustomerId";""");

            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_bank_transfers_ReversalOfTransferId";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_bank_transfers_ReversedByTransferId";""");

            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_bank_password_reset_tokens_CustomerId_IsUsed_ExpiresAt";""");

            migrationBuilder.DropColumn(
                name: "ReversalOfTransferId",
                table: "bank_transfers");

            migrationBuilder.DropColumn(
                name: "ReversedAt",
                table: "bank_transfers");

            migrationBuilder.DropColumn(
                name: "ReversedByTransferId",
                table: "bank_transfers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "bank_password_reset_tokens",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");
        }
    }
}
