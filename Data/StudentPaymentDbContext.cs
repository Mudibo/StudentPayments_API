//This is the gateway to the database, inherits from DbContext (EF Core base class)
//It represents a session with the database

using Microsoft.EntityFrameworkCore;
using StudentPayments_API.Models;

public class StudentPaymentsDbContext : DbContext
{
    //Constructor that receives database configuration and pass the options to base DbContext class
    public StudentPaymentsDbContext(DbContextOptions<StudentPaymentsDbContext> options) 
    : base(options) {}
    
    public DbSet<Student> Students { get; set; }
    
}