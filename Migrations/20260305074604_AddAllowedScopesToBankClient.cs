using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StudentPayments_API.Models.Enums;

#nullable disable

namespace StudentPayments_API.Migrations
{
    /// <inheritdoc />
    public partial class AddAllowedScopesToBankClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.AddColumn<string>(
                name: "AllowedScopes",
                table: "BankClients",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedScopes",
                table: "BankClients");

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    payment_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bank_client_id = table.Column<int>(type: "integer", nullable: true),
                    admission_number = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    payment_channel = table.Column<PaymentChannelEnum>(type: "payment_channel_enum", nullable: false),
                    payment_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    payment_type = table.Column<PaymentTypeEnum>(type: "payment_type_enum", nullable: false),
                    reference_number = table.Column<string>(type: "text", nullable: false),
                    student_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.payment_id);
                    table.ForeignKey(
                        name: "FK_payments_BankClients_bank_client_id",
                        column: x => x.bank_client_id,
                        principalTable: "BankClients",
                        principalColumn: "BankClientId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_payments_bank_client_id",
                table: "payments",
                column: "bank_client_id");
        }
    }
}
