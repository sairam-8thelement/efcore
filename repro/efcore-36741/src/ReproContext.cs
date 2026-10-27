using Microsoft.EntityFrameworkCore;

namespace EfCore36741Repro;

/// <summary>
/// The DbContext under test. Intentionally has no behavior beyond mapping
/// <see cref="User"/> -- the regression under investigation is in the query
/// pipeline (compiled ExecuteUpdate/ExecuteDelete translation), not in any
/// context-level customization.
/// </summary>
public class ReproContext : DbContext
{
    public ReproContext(DbContextOptions<ReproContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedNever();
            entity.Property(u => u.LastActive).IsRequired();
        });
    }
}
