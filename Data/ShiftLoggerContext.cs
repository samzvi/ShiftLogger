using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShiftLogger.Models;

public class ShiftLoggerContext : IdentityDbContext<User>
{
    public ShiftLoggerContext(DbContextOptions<ShiftLoggerContext> options) : base(options) { }

    public DbSet<Car> Cars { get; set; }
    public DbSet<Shift> Shifts { get; set; }
}
