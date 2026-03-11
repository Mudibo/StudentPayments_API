using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentPayments_API.Migrations
{
    /// <inheritdoc />
    public partial class IdempotencyKey_OneToOne_PaymentTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_IdempotencyKeyId",
                table: "PaymentTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_IdempotencyKeyId",
                table: "PaymentTransactions",
                column: "IdempotencyKeyId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_IdempotencyKeyId",
                table: "PaymentTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_IdempotencyKeyId",
                table: "PaymentTransactions",
                column: "IdempotencyKeyId");
        }
    }
}
