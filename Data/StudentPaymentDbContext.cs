//This is the gateway to the database, inherits from DbContext (EF Core base class)
//It represents a session with the database

namespace StudentPayments_API.Data;
using Microsoft.EntityFrameworkCore;
using StudentPayments_API.Models;

public class StudentPaymentsDbContext : DbContext
{
    //Constructor that receives database configuration and pass the options to base DbContext class
    public StudentPaymentsDbContext(DbContextOptions<StudentPaymentsDbContext> options) 
    : base(options) {}
    
    public DbSet<Student> Students { get; set; } //DbSet<t> represents a table in the database
    public DbSet<Payment> Payments { get; set; } //DbSet<t> represents a table in the database
    public DbSet<StudentDues> StudentDues { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Register the PostgreSQL enum for correct mapping
        modelBuilder.HasPostgresEnum<ProgramEnum>();
        modelBuilder.HasPostgresEnum<EnrollmentStatusEnum>("enrollment_enum");
        modelBuilder.HasPostgresEnum<PaymentTypeEnum>("payment_type_enum");
        modelBuilder.HasPostgresEnum<PaymentChannelEnum>("payment_channel_enum");
        modelBuilder.Entity<Payment>();
    }
}