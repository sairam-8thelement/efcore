using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EfCore36741Repro;

/// <summary>
/// Probes for a reachable SQL Server (typically a local Docker container)
/// and, if found, points this fixture's single DbContextOptions at a
/// database unique to this fixture instance (created by EnsureCreated on
/// first use). If no server is reachable, <see cref="ProviderFixture.IsAvailable"/>
/// is false and <see cref="ProviderFixture.SkipReason"/> explains why --
/// tests must report this provider as skipped, not omit it.
///
/// Connection details are read from the <c>EFCORE36741_SQLSERVER_CONNSTRING</c>
/// environment variable if set (pointing at an existing reachable server/login),
/// else default to a local Docker-style SQL Server instance on the default port.
/// </summary>
public sealed class SqlServerFixture : ProviderFixture
{
    public override string Name => "SqlServer";

    private const string DefaultProbeConnectionString =
        "Server=localhost,1433;Database=master;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;Connect Timeout=5";

    protected override async Task<DbContextOptions<ReproContext>> ProbeAndBuildOptionsAsync(Guid fixtureId)
    {
        var probeConnectionString =
            Environment.GetEnvironmentVariable("EFCORE36741_SQLSERVER_CONNSTRING")
            ?? DefaultProbeConnectionString;

        try
        {
            using var connection = new SqlConnection(probeConnectionString);
            await connection.OpenAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "no reachable SQL Server at the configured connection " +
                "(Docker daemon may not be running, or no container listening on the configured port): " +
                ex.Message,
                ex);
        }

        var databaseBuilder = new SqlConnectionStringBuilder(probeConnectionString)
        {
            InitialCatalog = $"EfCore36741Repro_{fixtureId:N}"
        };

        var optionsBuilder = new DbContextOptionsBuilder<ReproContext>()
            .UseSqlServer(databaseBuilder.ConnectionString);
        ConfigureCommon(optionsBuilder, Sql);

        return optionsBuilder.Options;
    }
}
