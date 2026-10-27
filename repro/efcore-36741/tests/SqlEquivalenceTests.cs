using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace EfCore36741Repro;

/// <summary>
/// AC#10: runs the inline and compiled variants of the same query under
/// separate <see cref="SqlCapture.Scope"/> labels and asserts the
/// normalized SQL text is identical. Both <see cref="SqlCapture.RawFor"/>
/// texts are always written to test output -- even if the compiled side
/// throws -- via a <c>try</c>/<c>finally</c> (never a swallowing
/// <c>catch</c>), so a 10.0.9 report can show the inline SQL next to
/// whatever the compiled side managed to emit before failing.
/// </summary>
public abstract class SqlEquivalenceTestsBase
{
    private static readonly DateTime NewLastActive = new(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);

    private readonly ProviderFixture _fixture;
    private readonly ITestOutputHelper _output;

    protected SqlEquivalenceTestsBase(ProviderFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _output.WriteLine(EnvironmentInfo.Describe());
    }

    [Fact]
    public async Task Compiled_and_inline_emit_identical_sql_for_update()
    {
        if (!ProviderGuard.ShouldRun(_fixture, _output))
        {
            return;
        }

        _fixture.Sql.Reset();

        // Routed through a local variable, not the Seed.TargetId const directly: a
        // direct reference to a const field is a C#-compile-time literal, which EF
        // embeds as a SQL constant rather than a parameter -- breaking the SQL-shape
        // comparison below against the compiled side's genuine delegate parameter.
        var targetId = Seed.TargetId;

        int inlineAffected;
        using (var inlineContext = await _fixture.NewSeededContextAsync())
        using (_fixture.Sql.Scope("inline"))
        {
            inlineAffected = await inlineContext.Users
                .Where(u => u.Id == targetId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.LastActive, NewLastActive));
        }

        var compiled = EF.CompileAsyncQuery(
            (ReproContext ctx, int id) => ctx.Users.Where(u => u.Id == id)
                .ExecuteUpdate(s => s.SetProperty(u => u.LastActive, NewLastActive)));

        int compiledAffected;
        try
        {
            using var compiledContext = await _fixture.NewSeededContextAsync();
            using (_fixture.Sql.Scope("compiled"))
            {
                compiledAffected = await TranslationEvidence.CaptureAsync(
                    nameof(Compiled_and_inline_emit_identical_sql_for_update), _fixture, _output,
                    () => compiled(compiledContext, Seed.TargetId));
            }
        }
        finally
        {
            _output.WriteLine($"inline SQL:{Environment.NewLine}{_fixture.Sql.RawFor("inline")}");
            _output.WriteLine($"compiled SQL:{Environment.NewLine}{_fixture.Sql.RawFor("compiled")}");
        }

        Assert.Equal(inlineAffected, compiledAffected);
        Assert.Equal(_fixture.Sql.NormalizedFor("inline"), _fixture.Sql.NormalizedFor("compiled"));
    }

    [Fact]
    public async Task Compiled_and_inline_emit_identical_sql_for_delete()
    {
        if (!ProviderGuard.ShouldRun(_fixture, _output))
        {
            return;
        }

        _fixture.Sql.Reset();

        // See the comment in Compiled_and_inline_emit_identical_sql_for_update: a
        // direct Seed.TargetId reference is a compile-time literal, not a parameter.
        var targetId = Seed.TargetId;

        int inlineAffected;
        using (var inlineContext = await _fixture.NewSeededContextAsync())
        using (_fixture.Sql.Scope("inline"))
        {
            inlineAffected = await inlineContext.Users
                .Where(u => u.Id == targetId)
                .ExecuteDeleteAsync();
        }

        var compiled = EF.CompileAsyncQuery(
            (ReproContext ctx, int id) => ctx.Users.Where(u => u.Id == id).ExecuteDelete());

        int compiledAffected;
        try
        {
            using var compiledContext = await _fixture.NewSeededContextAsync();
            using (_fixture.Sql.Scope("compiled"))
            {
                compiledAffected = await TranslationEvidence.CaptureAsync(
                    nameof(Compiled_and_inline_emit_identical_sql_for_delete), _fixture, _output,
                    () => compiled(compiledContext, Seed.TargetId));
            }
        }
        finally
        {
            _output.WriteLine($"inline SQL:{Environment.NewLine}{_fixture.Sql.RawFor("inline")}");
            _output.WriteLine($"compiled SQL:{Environment.NewLine}{_fixture.Sql.RawFor("compiled")}");
        }

        Assert.Equal(inlineAffected, compiledAffected);
        Assert.Equal(_fixture.Sql.NormalizedFor("inline"), _fixture.Sql.NormalizedFor("compiled"));
    }
}

[Collection(SqliteCollection.Name)]
[Trait("Provider", "Sqlite")]
public sealed class SqliteSqlEquivalenceTests : SqlEquivalenceTestsBase
{
    public SqliteSqlEquivalenceTests(SqliteFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }
}

[Collection(SqlServerCollection.Name)]
[Trait("Provider", "SqlServer")]
public sealed class SqlServerSqlEquivalenceTests : SqlEquivalenceTestsBase
{
    public SqlServerSqlEquivalenceTests(SqlServerFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }
}

[Collection(NpgsqlCollection.Name)]
[Trait("Provider", "Npgsql")]
public sealed class NpgsqlSqlEquivalenceTests : SqlEquivalenceTestsBase
{
    public NpgsqlSqlEquivalenceTests(NpgsqlFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }
}
