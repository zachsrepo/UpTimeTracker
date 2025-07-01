using Microsoft.EntityFrameworkCore;
using UpTimeTracker.Models;

public class AppDbContext : DbContext
{
    public DbSet<MonitoredService> MonitoredServices => Set<MonitoredService>();
    public DbSet<UptimeLog> UptimeLogs => Set<UptimeLog>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
