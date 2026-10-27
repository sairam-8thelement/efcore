using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace EfCore36741Repro;

/// <summary>
/// AC#2-#4, #6: the inline <c>ExecuteUpdateAsync</c> control, the async- and
/// sync-compiled <c>ExecuteUpdate</c> variants under investigation, each
/// with an unfiltered and a parameterized-<c>Where</c> variant, plus a
/// <c>TagWith</c> variant of the sync-compiled form.
///
/// Every assertion below is the CORRECT outcome -- nothing here encodes the
/// EF Core 10.0.9 regression as an expected failure. On 10.0.9 the three
/// compiled variants are expected (per the regression this project
/// reproduces) to throw instead of returning that correct outcome, and that
/// throw is exactly what produces the AC#7 failing baseline from this same,
/// unmodified code; on a fixed build the same code passes outright (AC#9).
/// </summary>
public abstract class ExecuteUpdateCompiledQueryTestsBase
{
    private static readonly DateTime NewLastActive = new(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);

    private readonly ProviderFixture _fixture;
    private readonly ITestOutputHelper _output;

    protected ExecuteUpdateCompiledQueryTestsBase(ProviderFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _output.WriteLine(EnvironmentInfo.Describe());
    }

    [Fact]
    public async Task Inline_ExecuteUpdateAsync_updates_all_rows()
    {
        if (!ProviderGuard.ShouldRun(_fixture, _output))
        {
            return;
        }

        int affected;
        using (var context = await _fixture.NewSeededContextAsync())
        {
            affected = await context.Users.ExecuteUpdateAsync(
                s => s.SetProperty(u => u.LastActive, NewLastActive));
        }

        Assert.Equal(Seed.RowCount, affected);
    }

    [Fact]
    public async Task Inline_ExecuteUpdateAsync_with_filter_updates_only_target()
    {
        if (!ProviderGuard.ShouldRun(_fixture, _output))
        {
            return;
        }

        string connectionString;
        int affected;
        using (var context = await _fixture.NewSeededContextAsync())
        {
            connectionString = context.Database.GetConnectionString()!;

            affected = await context.Users
                .Where(u => u.Id == Seed.TargetId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.LastActive, NewLastActive));
        }

        Assert.Equal(1, affected);

        using var readContext = FreshReadContext.For(_fixture, connectionString);
        await AssertOnlyTargetChangedAsync(readContext);
    }

    [Fact]
    public async Task CompiledAsync_ExecuteUpdate_updates_all_rows()
    {
        if (!ProviderGuard.ShouldRun(_fixture, _output))
        {
            return;
        }

        var compiled = EF.CompileAsyncQuery(
            (ReproContext ctx) => ctx.Users.ExecuteUpdate(s => s.SetProperty(u => u.LastActive, NewLastActive)));

        int affected;
        using (var context = await _fixture.NewSeededContextAsync())
        {
            affected = await TranslationEvidence.CaptureAsync(
                nameof(CompiledAsync_ExecuteUpdate_updates_all_rows), _fixture, _output,
                () => compiled(context));
        }

        Assert.Equal(Seed.RowCount, affected);
    }

    [Fact]
    public async Task CompiledAsync_ExecuteUpdate_with_filter_updates_only_target()
    {
        if (!ProviderGuard.ShouldRun(_fixture, _output))
        {
            return;
        }

        var compiled = EF.CompileAsyncQuery(
            (ReproContext ctx, int id) => ctx.Users.Where(u => u.Id == id)
                .ExecuteUpdate(s => s.SetProperty(u => u.LastActive, NewLastActive)));

        string connectionString;
        int affected;
        using (var context = await _fixture.NewSeededContextAsync())
        {
            connectionString = context.Database.GetConnectionString()!;

            affected = await TranslationEvidence.CaptureAsync(
                nameof(CompiledAsync_ExecuteUpdate_with_filter_updates_only_target), _fixture, _output,
                () => compiled(context, Seed.TargetId));
        }

        Assert.Equal(1, affected);

        using var readContext = FreshReadContext.For(_fixture, connectionString);
        await AssertOnlyTargetChangedAsync(readContext);
    }

    [Fact]
    public void CompiledSync_ExecuteUpdate_updates_all_rows()
    {
        if (!ProviderGuard.ShouldRun(_fixture, _output))
        {
            return;
        }

        var compiled = EF.CompileQuery(
            (ReproContext ctx) => ctx.Users.ExecuteUpdate(s => s.SetProperty(u => u.LastActive, NewLastActive)));

        int affected;
        using (var context = _fixture.NewSeededContext())
        {
            affected = TranslationEvidence.Capture(
                nameof(CompiledSync_ExecuteUpdate_updates_all_rows), _fixture, _output,
                () => compiled(context));
        }

        Assert.Equal(Seed.RowCount, affected);
    }

    [Fact]
    public void CompiledSync_ExecuteUpdate_with_filter_updates_only_target()
    {
        if (!ProviderGuard.ShouldRun(_fixture, _output))
        {
            return;
        }

        var compiled = EF.CompileQuery(
            (ReproContext ctx, int id) => ctx.Users.Where(u => u.Id == id)
                .ExecuteUpdate(s => s.SetProperty(u => u.LastActive, NewLastActive)));

        string connectionString;
        int affected;
        using (var context = _fixture.NewSeededContext())
        {
            connectionString = context.Database.GetConnectionString()!;

            affected = TranslationEvidence.Capture(
                nameof(CompiledSync_ExecuteUpdate_with_filter_updates_only_target), _fixture, _output,
                () => compiled(context, Seed.TargetId));
        }

        Assert.Equal(1, affected);

        using var readContext = FreshReadContext.For(_fixture, connectionString);
        AssertOnlyTargetChanged(readContext);
    }

    [Fact]
    public void CompiledSync_ExecuteUpdate_with_TagWith_updates_all_rows()
    {
        if (!ProviderGuard.ShouldRun(_fixture, _output))
        {
            return;
        }

        var compiled = EF.CompileQuery(
            (ReproContext ctx) => ctx.Users
                .TagWith("efcore36741-update")
                .ExecuteUpdate(s => s.SetProperty(u => u.LastActive, NewLastActive)));

        int affected;
        using (var context = _fixture.NewSeededContext())
        {
            affected = TranslationEvidence.Capture(
                nameof(CompiledSync_ExecuteUpdate_with_TagWith_updates_all_rows), _fixture, _output,
                () => compiled(context));
        }

        Assert.Equal(Seed.RowCount, affected);
    }

    private static async Task AssertOnlyTargetChangedAsync(ReproContext readContext)
    {
        var rows = await readContext.Users.AsNoTracking().OrderBy(u => u.Id).ToListAsync();
        AssertOnlyTargetChangedCore(rows);
    }

    private static void AssertOnlyTargetChanged(ReproContext readContext)
    {
        var rows = readContext.Users.AsNoTracking().OrderBy(u => u.Id).ToList();
        AssertOnlyTargetChangedCore(rows);
    }

    private static void AssertOnlyTargetChangedCore(List<User> rows)
    {
        Assert.Equal(Seed.RowCount, rows.Count);

        foreach (var expected in Seed.FreshRows())
        {
            var actual = rows.Single(r => r.Id == expected.Id);
            Assert.Equal(
                expected.Id == Seed.TargetId ? NewLastActive : expected.LastActive,
                actual.LastActive);
        }
    }
}

[Collection(SqliteCollection.Name)]
[Trait("Provider", "Sqlite")]
public sealed class SqliteExecuteUpdateCompiledQueryTests : ExecuteUpdateCompiledQueryTestsBase
{
    public SqliteExecuteUpdateCompiledQueryTests(SqliteFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }
}

[Collection(SqlServerCollection.Name)]
[Trait("Provider", "SqlServer")]
public sealed class SqlServerExecuteUpdateCompiledQueryTests : ExecuteUpdateCompiledQueryTestsBase
{
    public SqlServerExecuteUpdateCompiledQueryTests(SqlServerFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }
}

[Collection(NpgsqlCollection.Name)]
[Trait("Provider", "Npgsql")]
public sealed class NpgsqlExecuteUpdateCompiledQueryTests : ExecuteUpdateCompiledQueryTestsBase
{
    public NpgsqlExecuteUpdateCompiledQueryTests(NpgsqlFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }
}
