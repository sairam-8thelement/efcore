using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace EfCore36741Repro;

/// <summary>
/// Base for the per-provider xunit fixtures. A concrete fixture probes its
/// provider's availability once (<see cref="InitializeAsync"/>) and, if
/// reachable, builds exactly one <see cref="DbContextOptions{TContext}"/> for
/// its entire lifetime, pointed at one uniquely-named database (a GUID baked
/// into the SQLite filename / SQL Server or Npgsql database name).
///
/// Reusing that single options instance for every <see cref="NewSeededContext"/>
/// / <see cref="NewSeededContextAsync"/> call is deliberate: every
/// <see cref="ReproContext"/> built from one fixture therefore shares the
/// same cached model, which is required for compiled-query delegates
/// (EF.CompileQuery/EF.CompileAsyncQuery) to run against more than one
/// context instance from the same fixture without tripping
/// CompiledQueryDifferentModel -- a different failure mode than the one this
/// repro is about. "Fresh" per call means the shared database is wiped
/// (EnsureDeleted) and recreated/reseeded on every call, not that a new
/// database is created each time; the one GUID per fixture is what makes
/// xunit's default parallel test-class execution safe (no two fixture
/// instances, even of the same provider, ever share a database).
/// </summary>
public abstract class ProviderFixture : IAsyncLifetime
{
    private DbContextOptions<ReproContext>? _options;

    /// <summary>Provider display name: "SQLite", "SqlServer", or "Npgsql".</summary>
    public abstract string Name { get; }

    /// <summary>True once <see cref="InitializeAsync"/> has successfully probed and configured the provider.</summary>
    public bool IsAvailable { get; private set; }

    /// <summary>Human-readable reason the provider is unavailable; null iff <see cref="IsAvailable"/>.</summary>
    public string? SkipReason { get; private set; }

    /// <summary>SQL captured via LogTo for every context built from this fixture.</summary>
    public SqlCapture Sql { get; } = new();

    /// <summary>
    /// Probe the provider and build the single <see cref="DbContextOptions{TContext}"/>
    /// this fixture will reuse for its whole lifetime, using <paramref name="fixtureId"/>
    /// to name a database unique to this fixture instance. Implementations
    /// must throw (any exception) if the provider cannot be reached --
    /// <see cref="InitializeAsync"/> turns that into <see cref="IsAvailable"/> = false
    /// and a <see cref="SkipReason"/> rather than letting the exception propagate.
    /// </summary>
    protected abstract Task<DbContextOptions<ReproContext>> ProbeAndBuildOptionsAsync(Guid fixtureId);

    public async Task InitializeAsync()
    {
        try
        {
            _options = await ProbeAndBuildOptionsAsync(Guid.NewGuid());
            IsAvailable = true;
            SkipReason = null;
        }
        catch (Exception ex)
        {
            _options = null;
            IsAvailable = false;
            SkipReason = $"{Name}: {ex.Message}";
        }
    }

    public virtual Task DisposeAsync() => Task.CompletedTask;

    /// <summary>
    /// A context over this fixture's database, wiped and reseeded with
    /// exactly <see cref="Seed.RowCount"/> rows (async).
    /// </summary>
    public async Task<ReproContext> NewSeededContextAsync()
    {
        var context = NewContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        await Seed.SeedAsync(context);
        return context;
    }

    /// <summary>
    /// A context over this fixture's database, wiped and reseeded with
    /// exactly <see cref="Seed.RowCount"/> rows (sync).
    /// </summary>
    public ReproContext NewSeededContext()
    {
        var context = NewContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        Seed.SeedSync(context);
        return context;
    }

    private ReproContext NewContext()
    {
        if (!IsAvailable || _options is null)
        {
            throw new InvalidOperationException(
                $"{Name} fixture is not available ({SkipReason ?? "unknown reason"}). " +
                "Check IsAvailable (and skip the test with the recorded SkipReason) before calling NewSeededContext[Async].");
        }

        return new ReproContext(_options);
    }

    /// <summary>
    /// Wires the shared SQL-capture sink into a provider's options builder.
    /// Concrete fixtures call this after <c>.UseSqlite/UseSqlServer/UseNpgsql(...)</c>
    /// and before building <c>.Options</c>.
    /// </summary>
    protected static void ConfigureCommon(DbContextOptionsBuilder<ReproContext> builder, SqlCapture sql)
    {
        builder
            .EnableSensitiveDataLogging()
            .LogTo(sql.Log, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information);
    }
}
