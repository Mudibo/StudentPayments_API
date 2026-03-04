using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StudentPayments_API.Models.Enums;

#nullable disable

namespace StudentPayments_API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:currency_enum.currency_enum", "kes,usd,eur,gbp")
                .Annotation("Npgsql:Enum:enrollment_enum.enrollment_status_enum", "active,inactive")
                .Annotation("Npgsql:Enum:idempotency_resource_type_enum.idempotency_resource_type_enum", "payment_transaction")
                .Annotation("Npgsql:Enum:payment_channel_enum.payment_channel_enum", "mobile_banking,internet_banking")
                .Annotation("Npgsql:Enum:payment_transaction_status_enum.payment_transaction_status_enum", "success,failed")
                .Annotation("Npgsql:Enum:payment_type_enum.payment_type_enum", "cash,check,pesalink")
                .Annotation("Npgsql:Enum:program_enum.program_enum", "computer_science,law,international_relations");

            migrationBuilder.CreateTable(
                name: "BankClients",
                columns: table => new
                {
                    BankClientId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<string>(type: "text", nullable: false),
                    ClientSecretHash = table.Column<string>(type: "text", nullable: false),
                    BankName = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankClients", x => x.BankClientId);
                });

            migrationBuilder.CreateTable(
                name: "students",
                columns: table => new
                {
                    StudentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdmissionNumber = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    MobileNumber = table.Column<string>(type: "text", nullable: false),
                    Program = table.Column<ProgramEnum>(type: "program_enum", nullable: false),
                    EnrollmentStatus = table.Column<EnrollmentStatusEnum>(type: "enrollment_status_enum", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false, defaultValue: "Student")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_students", x => x.StudentId);
                });

            migrationBuilder.CreateTable(
                name: "IdempotencyKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BankClientId = table.Column<int>(type: "integer", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    RequestHash = table.Column<string>(type: "text", nullable: false),
                    ResourceType = table.Column<IdempotencyResourceTypeEnum>(type: "idempotency_resource_type_enum", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdempotencyKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdempotencyKeys_BankClients_BankClientId",
                        column: x => x.BankClientId,
                        principalTable: "BankClients",
                        principalColumn: "BankClientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    payment_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reference_number = table.Column<string>(type: "text", nullable: false),
                    payment_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    payment_type = table.Column<PaymentTypeEnum>(type: "payment_type_enum", nullable: false),
                    payment_channel = table.Column<PaymentChannelEnum>(type: "payment_channel_enum", nullable: false),
                    student_id = table.Column<int>(type: "integer", nullable: false),
                    admission_number = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    bank_client_id = table.Column<int>(type: "integer", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "StudentDues",
                columns: table => new
                {
                    DueId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    DuesAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DuesType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentDues", x => x.DueId);
                    table.CheckConstraint("CK_StudentDues_DuesAmount", "\"DuesAmount\" >= 0");
                    table.CheckConstraint("CK_StudentDues_DueType", "\"DuesType\" IN ('Tuition', 'Hostel', 'Library', 'Lab', 'Sports', 'Other')");
                    table.ForeignKey(
                        name: "FK_StudentDues_students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BankClientId = table.Column<int>(type: "integer", nullable: false),
                    BankReference = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<PaymentTransactionStatusEnum>(type: "payment_transaction_status_enum", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaymentType = table.Column<PaymentTypeEnum>(type: "payment_type_enum", nullable: false),
                    PaymentChannel = table.Column<PaymentChannelEnum>(type: "payment_channel_enum", nullable: false),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    InternalReference = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrencyType = table.Column<CurrencyEnum>(type: "currency_enum", nullable: false),
                    IdempotencyKeyId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.TransactionId);
                    table.CheckConstraint("CK_PaymentTransaction_Amount", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_BankClients_BankClientId",
                        column: x => x.BankClientId,
                        principalTable: "BankClients",
                        principalColumn: "BankClientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_IdempotencyKeys_IdempotencyKeyId",
                        column: x => x.IdempotencyKeyId,
                        principalTable: "IdempotencyKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankClients_ClientId",
                table: "BankClients",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyKeys_BankClientId_Key",
                table: "IdempotencyKeys",
                columns: new[] { "BankClientId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_bank_client_id",
                table: "payments",
                column: "bank_client_id");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_BankClientId_BankReference",
                table: "PaymentTransactions",
                columns: new[] { "BankClientId", "BankReference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_IdempotencyKeyId",
                table: "PaymentTransactions",
                column: "IdempotencyKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_InternalReference",
                table: "PaymentTransactions",
                column: "InternalReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_StudentId",
                table: "PaymentTransactions",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentDues_StudentId",
                table: "StudentDues",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_students_AdmissionNumber",
                table: "students",
                column: "AdmissionNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "StudentDues");

            migrationBuilder.DropTable(
                name: "IdempotencyKeys");

            migrationBuilder.DropTable(
                name: "students");

            migrationBuilder.DropTable(
                name: "BankClients");
        }
    }
}
