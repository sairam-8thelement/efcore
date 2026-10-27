using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;

namespace EfCore36741Repro;

/// <summary>
/// Describes the environment a test run actually executed against, read from
/// the runtime rather than from build-time MSBuild properties. This matters
/// because <c>-p:EFCoreVersion=X</c> only pins the requested package version;
/// what the test process actually loaded is what's reported here and is what
/// <see cref="TranslationEvidence"/> uses to name its output files, so a
/// mis-attributed run (e.g. a stale restore) is self-evident rather than
/// silently mislabeled.
/// </summary>
public static class EnvironmentInfo
{
    /// <summary>The version of the <c>Microsoft.EntityFrameworkCore</c> assembly actually loaded at runtime.</summary>
    public static string EfCoreAssemblyVersion { get; } = ResolveEfCoreAssemblyVersion();

    /// <summary>The .NET runtime description (e.g. ".NET 10.0.x").</summary>
    public static string RuntimeVersion { get; } = RuntimeInformation.FrameworkDescription;

    /// <summary>The OS description plus kernel/version string.</summary>
    public static string OperatingSystem { get; } = ResolveOperatingSystem();

    /// <summary>A one-line human-readable summary for inclusion in reports/issue comments.</summary>
    public static string Describe() =>
        $"EF Core {EfCoreAssemblyVersion}, {RuntimeVersion}, {OperatingSystem}";

    private static string ResolveEfCoreAssemblyVersion()
    {
        var assembly = typeof(DbContext).Assembly;

        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (!string.IsNullOrEmpty(informationalVersion))
        {
            // Strip any "+<commit sha>" build metadata suffix, e.g. "10.0.9+abcdef..." -> "10.0.9".
            var plusIndex = informationalVersion.IndexOf('+');
            return plusIndex >= 0 ? informationalVersion[..plusIndex] : informationalVersion;
        }

        return assembly.GetName().Version?.ToString() ?? "unknown";
    }

    private static string ResolveOperatingSystem()
    {
        var description = RuntimeInformation.OSDescription.Trim();
        var kernel = Environment.OSVersion.VersionString;
        return $"{description} (kernel: {kernel})";
    }
}
