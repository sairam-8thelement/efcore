using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace EfCore36741Repro;

/// <summary>
/// Ties each provider fixture to an xunit collection so every test class
/// that exercises a given provider (<c>ExecuteUpdateCompiledQueryTests</c>,
/// <c>ExecuteDeleteCompiledQueryTests</c>, <c>SqlEquivalenceTests</c>) shares
/// exactly one fixture instance for that provider -- one already-probed
/// connection, one already-named database -- instead of each test class
/// probing/naming its own.
///
/// This also puts every test for one provider into the same xunit test
/// collection, which xunit runs sequentially by default (collections are
/// xunit's unit of parallelization, not classes). That sequencing is load
/// -bearing here, not just an optimization: <see cref="ProviderFixture.NewSeededContextAsync"/>
/// and <see cref="ProviderFixture.NewSeededContext"/> wipe and reseed the
/// fixture's single shared database on every call, so two tests sharing a
/// fixture must never run concurrently against it.
/// </summary>
[CollectionDefinition(Name)]
public sealed class SqliteCollection : ICollectionFixture<SqliteFixture>
{
    public const string Name = "EfCore36741 SQLite provider";
}

[CollectionDefinition(Name)]
public sealed class SqlServerCollection : ICollectionFixture<SqlServerFixture>
{
    public const string Name = "EfCore36741 SqlServer provider";
}

[CollectionDefinition(Name)]
public sealed class NpgsqlCollection : ICollectionFixture<NpgsqlFixture>
{
    public const string Name = "EfCore36741 Npgsql provider";
}

/// <summary>
/// Shared "should this test actually run" guard for the provider-derived
/// test classes. SQLite's fixture is always available, so this is a
/// transparent pass-through for it; for SqlServer/Npgsql, when the fixture
/// could not reach its server, this writes the fixture's recorded
/// <see cref="ProviderFixture.SkipReason"/> to the test's own output before
/// returning <see langword="false"/> -- so a skip is always visible and
/// attributable in that test's TRX entry, never a silent omission.
/// </summary>
internal static class ProviderGuard
{
    public static bool ShouldRun(ProviderFixture fixture, ITestOutputHelper output)
    {
        if (fixture.IsAvailable)
        {
            return true;
        }

        output.WriteLine($"SKIPPED: {fixture.SkipReason}");
        return false;
    }
}

/// <summary>
/// Builds a brand-new <see cref="ReproContext"/> against the same physical
/// database a connection string points at, without wiping or reseeding
/// anything. This is what lets a test verify an ExecuteUpdate/ExecuteDelete
/// result "through a new context, never from tracked state": <see cref="ProviderFixture"/>'s
/// only public context factories (<see cref="ProviderFixture.NewSeededContextAsync"/>/
/// <see cref="ProviderFixture.NewSeededContext"/>) always wipe and reseed,
/// which would destroy the very mutation under test if used for the
/// verification read too.
///
/// Callers capture the connection string from the context that performed the
/// write (<c>context.Database.GetConnectionString()</c>) <em>before</em>
/// disposing it, then pass that string here once the write-side context is
/// disposed -- decoupling this helper, and the verification read, from the
/// write-side context's lifetime entirely.
/// </summary>
internal static class FreshReadContext
{
    public static ReproContext For(ProviderFixture fixture, string connectionString)
    {
        var builder = new DbContextOptionsBuilder<ReproContext>();

        switch (fixture)
        {
            case SqliteFixture:
                builder.UseSqlite(connectionString);
                break;
            case SqlServerFixture:
                builder.UseSqlServer(connectionString);
                break;
            case NpgsqlFixture:
                builder.UseNpgsql(connectionString);
                break;
            default:
                throw new NotSupportedException(
                    $"No fresh-read-context provider mapping for fixture type '{fixture.GetType().Name}'.");
        }

        return new ReproContext(builder.Options);
    }
}
