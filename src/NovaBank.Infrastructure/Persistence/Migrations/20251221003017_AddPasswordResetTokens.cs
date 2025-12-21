using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaBank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bank_password_reset_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequestedIp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RequestedUserAgent = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank_password_reset_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bank_password_reset_tokens_bank_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "bank_customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bank_password_reset_tokens_CustomerId",
                table: "bank_password_reset_tokens",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_bank_password_reset_tokens_ExpiresAt",
                table: "bank_password_reset_tokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_bank_password_reset_tokens_IsUsed",
                table: "bank_password_reset_tokens",
                column: "IsUsed");

            migrationBuilder.CreateIndex(
                name: "IX_bank_password_reset_tokens_CustomerId_IsUsed_ExpiresAt",
                table: "bank_password_reset_tokens",
                columns: new[] { "CustomerId", "IsUsed", "ExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bank_password_reset_tokens");
        }
    }
}
