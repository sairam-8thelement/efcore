using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace EfCore36741Repro;

/// <summary>
/// AC#5-#6: compiled <c>ExecuteDelete</c>, sync- and async-compiled, each
/// with an unfiltered and a parameterized-<c>Where</c> variant, plus the
/// distinct <c>ExecuteDeleteAsync</c> spelling inside an async-compiled
/// body.
///
/// Per this task's inputs, compiled sync-spelled <c>ExecuteDelete</c>
/// demonstrably SUCCEEDS on EF Core 10.0.9 (unlike every compiled
/// <c>ExecuteUpdate</c> form) -- so nothing here assumes or encodes a
/// failure; every assertion is the correct, unconditional outcome. The one
/// exception the inputs call out, <c>ExecuteDeleteAsync</c> inside an
/// async-compiled body, is exercised the same way: asserted to succeed, with
/// its actual 10.0.9 pass/fail behavior standing as the AC#7 evidence.
/// </summary>
public abstract class ExecuteDeleteCompiledQueryTestsBase
{
    private readonly ProviderFixture _fixture;
    private readonly ITestOutputHelper _output;

    protected ExecuteDeleteCompiledQueryTestsBase(ProviderFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _output.WriteLine(EnvironmentInfo.Describe());
    }

    [Fact]
    public void CompiledSync_ExecuteDelete_removes_all_rows()
    {
        if (!ProviderGuard.ShouldRun(_fixture, _output))
        {
            return;
        }

        var compiled = EF.CompileQuery((ReproContext ctx) => ctx.Users.ExecuteDelete());

        string connectionString;
        int affected;
        using (var context = _fixture.NewSeededContext())
        {
            connectionString = context.Database.GetConnectionString()!;

            affected = TranslationEvidence.Capture(
                nameof(CompiledSync_ExecuteDelete_removes_all_rows), _fixture, _output,
                () => compiled(context));
        }

        Assert.Equal(Seed.RowCount, affected);

        using var readContext = FreshReadContext.For(_fixture, connectionString);
        Assert.Empty(readContext.Users.AsNoTracking().ToList());
    }

    [Fact]
    public void CompiledSync_ExecuteDelete_with_filter_removes_only_target()
    {
        if (!ProviderGuard.ShouldRun(_fixture, _output))
        {
            return;
        }

        var compiled = EF.CompileQuery(
            (ReproContext ctx, int id) => ctx.Users.Where(u => u.Id == id).ExecuteDelete());

        string connectionString;
        int affected;
        using (var context = _fixture.NewSeededContext())
        {
            connectionString = context.Database.GetConnectionString()!;

            affected = TranslationEvidence.Capture(
                nameof(CompiledSync_ExecuteDelete_with_filter_removes_only_target), _fixture, _output,
                () => compiled(context, Seed.TargetId));
        }

        Assert.Equal(1, affected);

        using var readContext = FreshReadContext.For(_fixture, connectionString);
        AssertOnlyTargetRemoved(readContext);
    }

    [Fact]
    public async Task CompiledAsync_body_ExecuteDelete_removes_all_rows()
    {
        if (!ProviderGuard.ShouldRun(_fixture, _output))
        {
            return;
        }

        var compiled = EF.CompileAsyncQuery((ReproContext ctx) => ctx.Users.ExecuteDelete());

        string connectionString;
        int affected;
        using (var context = await _fixture.NewSeededContextAsync())
        {
            connectionString = context.Database.GetConnectionString()!;

            affected = await TranslationEvidence.CaptureAsync(
                nameof(CompiledAsync_body_ExecuteDelete_removes_all_rows), _fixture, _output,
                () => compiled(context));
        }

        Assert.Equal(Seed.RowCount, affected);

        using var readContext = FreshReadContext.For(_fixture, connectionString);
        Assert.Empty(await readContext.Users.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task CompiledAsync_body_ExecuteDelete_with_filter_removes_only_target()
    {
        if (!ProviderGuard.ShouldRun(_fixture, _output))
        {
            return;
        }

        var compiled = EF.CompileAsyncQuery(
            (ReproContext ctx, int id) => ctx.Users.Where(u => u.Id == id).ExecuteDelete());

        string connectionString;
        int affected;
        using (var context = await _fixture.NewSeededContextAsync())
        {
            connectionString = context.Database.GetConnectionString()!;

            affected = await TranslationEvidence.CaptureAsync(
                nameof(CompiledAsync_body_ExecuteDelete_with_filter_removes_only_target), _fixture, _output,
                () => compiled(context, Seed.TargetId));
        }

        Assert.Equal(1, affected);

        using var readContext = FreshReadContext.For(_fixture, connectionString);
        await AssertOnlyTargetRemovedAsync(readContext);
    }

    [Fact]
    public async Task CompiledAsync_body_spelled_ExecuteDeleteAsync_removes_only_target()
    {
        if (!ProviderGuard.ShouldRun(_fixture, _output))
        {
            return;
        }

        // Distinct spelling, deliberately not assumed to behave like the
        // sync-terminal ExecuteDelete() form above: EF.CompileAsyncQuery's
        // single-param-body overload infers TResult from this lambda's
        // natural body type (Task<int>, since ExecuteDeleteAsync is itself
        // async), producing Func<ReproContext,int,Task<Task<int>>> -- a
        // different closed generic method than the TResult=int instantiation
        // used elsewhere in this file for the sync-terminal spelling.
        var compiled = EF.CompileAsyncQuery(
            (ReproContext ctx, int id) => ctx.Users.Where(u => u.Id == id).ExecuteDeleteAsync());

        string connectionString;
        int affected;
        using (var context = await _fixture.NewSeededContextAsync())
        {
            connectionString = context.Database.GetConnectionString()!;

            affected = await TranslationEvidence.CaptureAsync(
                nameof(CompiledAsync_body_spelled_ExecuteDeleteAsync_removes_only_target), _fixture, _output,
                async () => await (await compiled(context, Seed.TargetId)));
        }

        Assert.Equal(1, affected);

        using var readContext = FreshReadContext.For(_fixture, connectionString);
        await AssertOnlyTargetRemovedAsync(readContext);
    }

    private static void AssertOnlyTargetRemoved(ReproContext readContext)
    {
        var rows = readContext.Users.AsNoTracking().OrderBy(u => u.Id).ToList();
        AssertOnlyTargetRemovedCore(rows);
    }

    private static async Task AssertOnlyTargetRemovedAsync(ReproContext readContext)
    {
        var rows = await readContext.Users.AsNoTracking().OrderBy(u => u.Id).ToListAsync();
        AssertOnlyTargetRemovedCore(rows);
    }

    private static void AssertOnlyTargetRemovedCore(List<User> rows)
    {
        Assert.Equal(Seed.RowCount - 1, rows.Count);
        Assert.DoesNotContain(rows, r => r.Id == Seed.TargetId);

        foreach (var expected in Seed.FreshRows().Where(r => r.Id != Seed.TargetId))
        {
            var actual = rows.Single(r => r.Id == expected.Id);
            Assert.Equal(expected.LastActive, actual.LastActive);
        }
    }
}

[Collection(SqliteCollection.Name)]
[Trait("Provider", "Sqlite")]
public sealed class SqliteExecuteDeleteCompiledQueryTests : ExecuteDeleteCompiledQueryTestsBase
{
    public SqliteExecuteDeleteCompiledQueryTests(SqliteFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }
}

[Collection(SqlServerCollection.Name)]
[Trait("Provider", "SqlServer")]
public sealed class SqlServerExecuteDeleteCompiledQueryTests : ExecuteDeleteCompiledQueryTestsBase
{
    public SqlServerExecuteDeleteCompiledQueryTests(SqlServerFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }
}

[Collection(NpgsqlCollection.Name)]
[Trait("Provider", "Npgsql")]
public sealed class NpgsqlExecuteDeleteCompiledQueryTests : ExecuteDeleteCompiledQueryTestsBase
{
    public NpgsqlExecuteDeleteCompiledQueryTests(NpgsqlFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }
}
