using Microsoft.EntityFrameworkCore;

using isgood.Models;
using System;

namespace isgood.Database;

public class AppDbContext : DbContext
{
    public DbSet<Product> Product { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=data/isgood.sqlite");
        }
    }

    public void BootstrapDatabase()
    {
        try
        {
            Database.Migrate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"+ isgood: Error bootstrapping the database: {ex}");
        }
    }
}