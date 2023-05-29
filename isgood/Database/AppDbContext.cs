using Microsoft.EntityFrameworkCore;

using isgood.Models;

namespace isgood.Database;

public class AppDbContext : DbContext
{
    public DbSet<Product> Product { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=isgood.db");
    }
}