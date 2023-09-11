using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TestJob.Domain.Entity;

namespace TestJob.Domain.Context;

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Item> Items { get; set; }
    public DbSet<CartUnit> CartUnits { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderUnit> OrderUnits { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseMySql("Server=localhost;Database=fr;User=root;Password=0099;"
                , new MySqlServerVersion(new Version(8, 0, 34)));
        }
    }
}