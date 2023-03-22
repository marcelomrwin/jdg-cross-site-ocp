using System;
using EmployeeNetCoreApp.Model;
using Microsoft.EntityFrameworkCore;

namespace EmployeeNetCoreApp.Data
{
	public class DataContext: DbContext
	{
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; } = default!;        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        
            modelBuilder.Entity<Employee>()
                .Property(b => b.CreateDate)
                .HasDefaultValue(DateTime.UtcNow);
            modelBuilder.Entity<Employee>()
                .Property(b => b.CreatedBy)
                .HasDefaultValue(1);
            modelBuilder.Entity<Employee>()
                .Property(b => b.UpdatedBy)
                .HasDefaultValue(null);
            modelBuilder.Entity<Employee>()
             .Property(b => b.UpdatedDate)
             .HasDefaultValue(null);
            modelBuilder.Entity<Employee>()
                .Property(p => p.Version)
                .HasDefaultValue(1);
        }
        
    }
}

