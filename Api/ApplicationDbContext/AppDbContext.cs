using Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.ApplicationDbContext;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<TimeSpent> TimeSpent { get; set; }
    public DbSet<MonthTimeSpent> MonthTimeSpent { get; set; }
}

