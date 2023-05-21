namespace isgood.Database;

using Microsoft.EntityFrameworkCore;

using isgood.Models;

public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=isgood.db");
    }
}