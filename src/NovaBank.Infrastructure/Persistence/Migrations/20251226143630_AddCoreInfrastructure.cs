using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaBank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCoreInfrastructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "approved_at",
                table: "bank_loans",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "approved_by_id",
                table: "bank_loans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "disbursement_account_id",
                table: "bank_loans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_approved",
                table: "bank_loans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "next_payment_amount",
                table: "bank_loans",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "next_payment_date",
                table: "bank_loans",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "paid_installments",
                table: "bank_loans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "rejection_reason",
                table: "bank_loans",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "remaining_principal",
                table: "bank_loans",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "branch_id",
                table: "bank_customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "failed_login_count",
                table: "bank_customers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "kyc_completed",
                table: "bank_customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "kyc_completed_at",
                table: "bank_customers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_login_at",
                table: "bank_customers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "locked_until",
                table: "bank_customers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "risk_level",
                table: "bank_customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Low");

            migrationBuilder.AddColumn<string>(
                name: "account_type",
                table: "bank_accounts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Checking");

            migrationBuilder.AddColumn<DateTime>(
                name: "approved_at",
                table: "bank_accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "approved_by_id",
                table: "bank_accounts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "branch_id",
                table: "bank_accounts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "interest_rate",
                table: "bank_accounts",
                type: "numeric(8,5)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "is_approved",
                table: "bank_accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "approval_workflows",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    required_role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    approved_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "text", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_approval_workflows", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bill_institutions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bill_institutions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bill_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    institution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subscriber_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    commission = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    reference_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bill_payments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "branches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "commissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    commission_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    fixed_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    percentage_rate = table.Column<decimal>(type: "numeric(8,5)", precision: 8, scale: 5, nullable: false, defaultValue: 0m),
                    min_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    max_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exchange_rates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    base_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    target_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    buy_rate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    sell_rate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    effective_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "TCMB"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exchange_rates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "kyc_verifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    verification_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    document_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    verified_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "text", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kyc_verifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notification_preferences",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_sms = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    transaction_email = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    login_sms = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    login_email = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    marketing_sms = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    marketing_email = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    security_alert_sms = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    security_alert_email = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_preferences", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notification_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transaction_limits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    limit_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    scope_id = table.Column<Guid>(type: "uuid", nullable: true),
                    scope_role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    min_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    max_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    requires_approval_above = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_limits", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bank_customers_branch_id",
                table: "bank_customers",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_bank_customers_Email",
                table: "bank_customers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_bank_accounts_branch_id",
                table: "bank_accounts",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_approval_workflows_entity_type_status",
                table: "approval_workflows",
                columns: new[] { "entity_type", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_approval_workflows_requested_by_id",
                table: "approval_workflows",
                column: "requested_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_approval_workflows_status",
                table: "approval_workflows",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_bill_institutions_category",
                table: "bill_institutions",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "IX_bill_institutions_code",
                table: "bill_institutions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bill_payments_account_id",
                table: "bill_payments",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_bill_payments_institution_id",
                table: "bill_payments",
                column: "institution_id");

            migrationBuilder.CreateIndex(
                name: "IX_bill_payments_reference_code",
                table: "bill_payments",
                column: "reference_code");

            migrationBuilder.CreateIndex(
                name: "IX_branches_code",
                table: "branches",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_commissions_commission_type_is_active",
                table: "commissions",
                columns: new[] { "commission_type", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_exchange_rates_base_currency_target_currency_effective_date",
                table: "exchange_rates",
                columns: new[] { "base_currency", "target_currency", "effective_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_kyc_verifications_customer_id_verification_type",
                table: "kyc_verifications",
                columns: new[] { "customer_id", "verification_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_kyc_verifications_status",
                table: "kyc_verifications",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_notification_preferences_customer_id",
                table: "notification_preferences",
                column: "customer_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_created_at",
                table: "notifications",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_customer_id_status",
                table: "notifications",
                columns: new[] { "customer_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_transaction_limits_limit_type_scope_scope_id_currency",
                table: "transaction_limits",
                columns: new[] { "limit_type", "scope", "scope_id", "currency" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "approval_workflows");

            migrationBuilder.DropTable(
                name: "bill_institutions");

            migrationBuilder.DropTable(
                name: "bill_payments");

            migrationBuilder.DropTable(
                name: "branches");

            migrationBuilder.DropTable(
                name: "commissions");

            migrationBuilder.DropTable(
                name: "exchange_rates");

            migrationBuilder.DropTable(
                name: "kyc_verifications");

            migrationBuilder.DropTable(
                name: "notification_preferences");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "transaction_limits");

            migrationBuilder.DropIndex(
                name: "IX_bank_customers_branch_id",
                table: "bank_customers");

            migrationBuilder.DropIndex(
                name: "IX_bank_customers_Email",
                table: "bank_customers");

            migrationBuilder.DropIndex(
                name: "IX_bank_accounts_branch_id",
                table: "bank_accounts");

            migrationBuilder.DropColumn(
                name: "approved_at",
                table: "bank_loans");

            migrationBuilder.DropColumn(
                name: "approved_by_id",
                table: "bank_loans");

            migrationBuilder.DropColumn(
                name: "disbursement_account_id",
                table: "bank_loans");

            migrationBuilder.DropColumn(
                name: "is_approved",
                table: "bank_loans");

            migrationBuilder.DropColumn(
                name: "next_payment_amount",
                table: "bank_loans");

            migrationBuilder.DropColumn(
                name: "next_payment_date",
                table: "bank_loans");

            migrationBuilder.DropColumn(
                name: "paid_installments",
                table: "bank_loans");

            migrationBuilder.DropColumn(
                name: "rejection_reason",
                table: "bank_loans");

            migrationBuilder.DropColumn(
                name: "remaining_principal",
                table: "bank_loans");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "bank_customers");

            migrationBuilder.DropColumn(
                name: "failed_login_count",
                table: "bank_customers");

            migrationBuilder.DropColumn(
                name: "kyc_completed",
                table: "bank_customers");

            migrationBuilder.DropColumn(
                name: "kyc_completed_at",
                table: "bank_customers");

            migrationBuilder.DropColumn(
                name: "last_login_at",
                table: "bank_customers");

            migrationBuilder.DropColumn(
                name: "locked_until",
                table: "bank_customers");

            migrationBuilder.DropColumn(
                name: "risk_level",
                table: "bank_customers");

            migrationBuilder.DropColumn(
                name: "account_type",
                table: "bank_accounts");

            migrationBuilder.DropColumn(
                name: "approved_at",
                table: "bank_accounts");

            migrationBuilder.DropColumn(
                name: "approved_by_id",
                table: "bank_accounts");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "bank_accounts");

            migrationBuilder.DropColumn(
                name: "interest_rate",
                table: "bank_accounts");

            migrationBuilder.DropColumn(
                name: "is_approved",
                table: "bank_accounts");
        }
    }
}
