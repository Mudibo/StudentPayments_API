//This is the gateway to the database, inherits from DbContext (EF Core base class)
//It represents a session with the database

namespace StudentPayments_API.Data;
using Microsoft.EntityFrameworkCore;
using StudentPayments_API.Models;
using StudentPayments_API.Models.Enums;

public class StudentPaymentsDbContext : DbContext
{
    //Constructor that receives database configuration and pass the options to base DbContext class
    public StudentPaymentsDbContext(DbContextOptions<StudentPaymentsDbContext> options)
    : base(options) { }

    public DbSet<Student> Students { get; set; } //DbSet<t> represents a table in the database
    public DbSet<StudentDues> StudentDues { get; set; }
    public DbSet<BankClient> BankClients { get; set; }
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    public DbSet<IdempotencyKey> IdempotencyKeys { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Enums
        modelBuilder.HasPostgresEnum<ProgramEnum>("program_enum");
        modelBuilder.HasPostgresEnum<EnrollmentStatusEnum>("enrollment_enum");
        modelBuilder.HasPostgresEnum<PaymentTypeEnum>("payment_type_enum");
        modelBuilder.HasPostgresEnum<PaymentChannelEnum>("payment_channel_enum");
        modelBuilder.HasPostgresEnum<IdempotencyResourceTypeEnum>("idempotency_resource_type_enum");
        modelBuilder.HasPostgresEnum<PaymentTransactionStatusEnum>("payment_transaction_status_enum");
        modelBuilder.HasPostgresEnum<CurrencyEnum>("currency_enum");

        //Student
        modelBuilder.Entity<Student>()
            .HasKey(s => s.StudentId);
        modelBuilder.Entity<Student>()
            .HasIndex(s => s.AdmissionNumber)
            .IsUnique();
        modelBuilder.Entity<Student>()
            .Property(s => s.Role)
            .HasDefaultValue("Student");
        modelBuilder.Entity<Student>()
            .HasMany(s => s.StudentDues)
            .WithOne(sd => sd.Student)
            .HasForeignKey(sd => sd.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Student>()
            .HasMany(s => s.PaymentTransactions)
            .WithOne(pt => pt.Student)
            .HasForeignKey(pt => pt.StudentId);

        //StudentDues
        modelBuilder.Entity<StudentDues>()
            .HasKey(sd => sd.DueId);
        modelBuilder.Entity<StudentDues>()
            .Property(sd => sd.DuesType)
            .HasConversion<string>();
        modelBuilder.Entity<StudentDues>()
            .ToTable(t => t.HasCheckConstraint("CK_StudentDues_DuesAmount", "\"DuesAmount\" >= 0"))
            .ToTable(t => t.HasCheckConstraint("CK_StudentDues_DueType", "\"DuesType\" IN ('Tuition', 'Hostel', 'Library', 'Lab', 'Sports', 'Other')"));

        //BankClient
        modelBuilder.Entity<BankClient>()
            .HasKey(bc => bc.BankClientId);
        modelBuilder.Entity<BankClient>()
            .HasIndex(bc => bc.ClientId)
            .IsUnique();
        modelBuilder.Entity<BankClient>()
            .HasMany(bc => bc.PaymentTransactions)
            .WithOne(pt => pt.BankClient)
            .HasForeignKey(pt => pt.BankClientId);
        modelBuilder.Entity<BankClient>()
            .HasMany(bc => bc.IdempotencyKeys)
            .WithOne(ik => ik.BankClient)
            .HasForeignKey(ik => ik.BankClientId)
            .OnDelete(DeleteBehavior.Cascade);

        //IdempotencyKey
        modelBuilder.Entity<IdempotencyKey>()
            .HasKey(ik => ik.Id);
        modelBuilder.Entity<IdempotencyKey>()
            .HasIndex(ik => new { ik.BankClientId, ik.Key })
            .IsUnique();
        modelBuilder.Entity<IdempotencyKey>()
            .HasOne(ik => ik.PaymentTransaction)
            .WithOne(pt => pt.IdempotencyKey)
            .HasForeignKey<PaymentTransaction>(pt => pt.IdempotencyKeyId)
            .OnDelete(DeleteBehavior.Cascade);

        //PaymentTransaction
        modelBuilder.Entity<PaymentTransaction>()
            .HasKey(pt => pt.TransactionId);
        modelBuilder.Entity<PaymentTransaction>()
            .HasIndex(pt => new { pt.BankClientId, pt.BankReference })
            .IsUnique();
        modelBuilder.Entity<PaymentTransaction>()
            .HasIndex(pt => pt.InternalReference)
            .IsUnique();
        modelBuilder.Entity<PaymentTransaction>()
            .HasIndex(pt => pt.IdempotencyKeyId)
            .IsUnique();
        modelBuilder.Entity<PaymentTransaction>()
            .ToTable(t => t.HasCheckConstraint("CK_PaymentTransaction_Amount", "\"Amount\" > 0"));
    }
}