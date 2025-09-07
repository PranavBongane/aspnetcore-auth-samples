using Microsoft.EntityFrameworkCore;
using ProductApi.Models;

namespace ProductApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Basic configuration with constraints
        modelBuilder.Entity<Product>(e =>
        {
            e.Property(p => p.Name).IsRequired().HasMaxLength(120);
            e.Property(p => p.Sku).IsRequired().HasMaxLength(64);
            e.HasIndex(p => p.Sku).IsUnique();
            e.Property(p => p.Price).HasPrecision(18,2);
        });
    }
}
