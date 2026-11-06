using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlanetOverview.Models;

namespace PlanetOverview.Data;

public class AccountDbContext(DbContextOptions<AccountDbContext> options) : IdentityDbContext<AppUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<AppUser>().Property(user => user.DisplayName).HasMaxLength(100);
        builder.Entity<AppUser>().HasIndex(user => user.NormalizedEmail).IsUnique();
    }
}
