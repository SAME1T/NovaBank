using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaBank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditCardSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BillingCycleDay",
                table: "bank_cards",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentDebt",
                table: "bank_cards",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "bank_cards",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "bank_cards",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MinPaymentAmount",
                table: "bank_cards",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MinPaymentDueDate",
                table: "bank_cards",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "bank_credit_card_applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedLimit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ApprovedLimit = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MonthlyIncome = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedByAdminId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank_credit_card_applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bank_credit_card_applications_bank_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "bank_customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bank_credit_card_applications_bank_customers_ProcessedByAdm~",
                        column: x => x.ProcessedByAdminId,
                        principalTable: "bank_customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bank_cards_CustomerId",
                table: "bank_cards",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_bank_credit_card_applications_CustomerId",
                table: "bank_credit_card_applications",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_bank_credit_card_applications_ProcessedByAdminId",
                table: "bank_credit_card_applications",
                column: "ProcessedByAdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_bank_cards_bank_customers_CustomerId",
                table: "bank_cards",
                column: "CustomerId",
                principalTable: "bank_customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bank_cards_bank_customers_CustomerId",
                table: "bank_cards");

            migrationBuilder.DropTable(
                name: "bank_credit_card_applications");

            migrationBuilder.DropIndex(
                name: "IX_bank_cards_CustomerId",
                table: "bank_cards");

            migrationBuilder.DropColumn(
                name: "BillingCycleDay",
                table: "bank_cards");

            migrationBuilder.DropColumn(
                name: "CurrentDebt",
                table: "bank_cards");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "bank_cards");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "bank_cards");

            migrationBuilder.DropColumn(
                name: "MinPaymentAmount",
                table: "bank_cards");

            migrationBuilder.DropColumn(
                name: "MinPaymentDueDate",
                table: "bank_cards");
        }
    }
}
