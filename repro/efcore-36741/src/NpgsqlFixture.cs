using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EfCore36741Repro;

/// <summary>
/// Probes for a reachable PostgreSQL server and, if found, points this
/// fixture's single DbContextOptions at a database unique to this fixture
/// instance (created by EnsureCreated on first use). If no server is
/// reachable, <see cref="ProviderFixture.IsAvailable"/> is false and
/// <see cref="ProviderFixture.SkipReason"/> explains why -- tests must
/// report this provider as skipped, not omit it.
///
/// Connection details are read from the <c>EFCORE36741_NPGSQL_CONNSTRING</c>
/// environment variable if set (pointing at an existing reachable
/// server/login), else default to a local Postgres instance on the default
/// port.
/// </summary>
public sealed class NpgsqlFixture : ProviderFixture
{
    public override string Name => "Npgsql";

    private const string DefaultProbeConnectionString =
        "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres;Timeout=5;Command Timeout=5";

    protected override async Task<DbContextOptions<ReproContext>> ProbeAndBuildOptionsAsync(Guid fixtureId)
    {
        var probeConnectionString =
            Environment.GetEnvironmentVariable("EFCORE36741_NPGSQL_CONNSTRING")
            ?? DefaultProbeConnectionString;

        try
        {
            using var connection = new NpgsqlConnection(probeConnectionString);
            await connection.OpenAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "no reachable PostgreSQL server at the configured connection " +
                "(Docker daemon may not be running, or no container listening on the configured port): " +
                ex.Message,
                ex);
        }

        var databaseBuilder = new NpgsqlConnectionStringBuilder(probeConnectionString)
        {
            Database = $"efcore36741_repro_{fixtureId:N}"
        };

        var optionsBuilder = new DbContextOptionsBuilder<ReproContext>()
            .UseNpgsql(databaseBuilder.ConnectionString);
        ConfigureCommon(optionsBuilder, Sql);

        return optionsBuilder.Options;
    }
}
