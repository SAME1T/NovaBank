using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaBank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "bank_customers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "bank_customers");
        }
    }
}
