using Microsoft.EntityFrameworkCore;

namespace EfCore36741Repro;

/// <summary>
/// SQLite is bundled (no external server needed), so this fixture is always
/// available. Each fixture instance gets its own file-backed database under
/// the temp directory, named with a GUID generated once in
/// <see cref="ProviderFixture.InitializeAsync"/>.
/// </summary>
public sealed class SqliteFixture : ProviderFixture
{
    public override string Name => "SQLite";

    protected override Task<DbContextOptions<ReproContext>> ProbeAndBuildOptionsAsync(Guid fixtureId)
    {
        var dir = Path.Combine(Path.GetTempPath(), "efcore36741-repro");
        Directory.CreateDirectory(dir);
        var dbPath = Path.Combine(dir, $"{fixtureId:N}.db");

        var builder = new DbContextOptionsBuilder<ReproContext>()
            .UseSqlite($"Data Source={dbPath}");
        ConfigureCommon(builder, Sql);

        return Task.FromResult(builder.Options);
    }
}
