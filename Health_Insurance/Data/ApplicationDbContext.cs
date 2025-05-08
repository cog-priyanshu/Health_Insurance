// Data/ApplicationDbContext.cs
using Health_Insurance.Models; // Make sure namespace is correct based on your project name
using Microsoft.EntityFrameworkCore;

namespace Health_Insurance.Data // Ensure this namespace is correct based on your project name
{
    // DbContext is the main class that coordinates Entity Framework functionality
    // for a given data model. It MUST inherit from DbContext.
    public class ApplicationDbContext : DbContext
    {
        // Constructor that accepts DbContextOptions, typically configured in Program.cs/Startup.cs
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet properties represent collections of entities that can be queried from the database.
        // These will map to your database tables.
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Organization> Organizations { get; set; }

        // Add DbSets for the Policy and Enrollment models
        public DbSet<Policy> Policies { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        // Add DbSets for other entities (Policy, Enrollment, Claim) as you create their models.
        // public DbSet<Policy> Policies { get; set; }
        // public DbSet<Enrollment> Enrollments { get; set; }
        // public DbSet<Claim> Claims { get; set; }

        // Optional: Configure model properties using the Fluent API
        // This is an alternative to Data Annotations (like [Required], [StringLength])
        // protected override void OnModelCreating(ModelBuilder modelBuilder)
        // {
        //     // Example: Configure string length for Employee Name
        //     modelBuilder.Entity<Employee>()
        //         .Property(e => e.Name)
        //         .HasMaxLength(100);

        //     // Configure the relationship between Employee and Organization
        //     modelBuilder.Entity<Employee>()
        //         .HasOne(e => e.Organization) // An Employee has one Organization
        //         .WithMany() // An Organization can have many Employees (or specify the navigation property in Organization model)
        //         .HasForeignKey(e => e.OrganizationId); // The foreign key property
        // }
    }
}

