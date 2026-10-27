using System.Runtime.CompilerServices;
using System.Text;
using Xunit.Abstractions;

namespace EfCore36741Repro;

/// <summary>
/// Wraps a test's "run the query" step so that, on failure, the exception's
/// message, whether it's wrapped (InnerException), and its first three stack
/// frames are captured to the xunit test output AND appended to
/// <c>results/evidence/&lt;provider-lowercase&gt;-&lt;EfCoreAssemblyVersion&gt;.md</c>
/// -- then the original exception is rethrown completely unmodified (same
/// type, message, and stack trace). On success nothing is recorded.
///
/// This is what lets the exact same test code produce the AC#7 failing
/// baseline on 10.0.9 and the AC#9 passing run on a fixed build: this helper
/// never converts a failure into a pass, it only observes failures as they
/// happen.
/// </summary>
public static class TranslationEvidence
{
    public static T Capture<T>(string testName, ProviderFixture provider, ITestOutputHelper output, Func<T> run)
    {
        try
        {
            return run();
        }
        catch (Exception ex)
        {
            RecordEvidence(testName, provider, output, ex);
            throw;
        }
    }

    public static async Task<T> CaptureAsync<T>(string testName, ProviderFixture provider, ITestOutputHelper output, Func<Task<T>> run)
    {
        try
        {
            return await run();
        }
        catch (Exception ex)
        {
            RecordEvidence(testName, provider, output, ex);
            throw;
        }
    }

    private static readonly object FileLock = new();

    // Captured at compile time via CallerFilePath, so it resolves to this
    // project's source tree regardless of which EFCoreVersion's bin/obj the
    // test process is actually running from.
    private static readonly string ProjectRoot = ResolveProjectRoot();

    private static string ResolveProjectRoot([CallerFilePath] string sourceFilePath = "")
    {
        var srcDir = Path.GetDirectoryName(sourceFilePath)!;
        return Path.GetFullPath(Path.Combine(srcDir, ".."));
    }

    private static void RecordEvidence(string testName, ProviderFixture provider, ITestOutputHelper output, Exception ex)
    {
        var report = BuildReport(testName, provider, ex);

        output.WriteLine(report);

        var evidenceDir = Path.Combine(ProjectRoot, "results", "evidence");
        var fileName = $"{provider.Name.ToLowerInvariant()}-{EnvironmentInfo.EfCoreAssemblyVersion}.md";
        var filePath = Path.Combine(evidenceDir, fileName);

        lock (FileLock)
        {
            Directory.CreateDirectory(evidenceDir);
            File.AppendAllText(filePath, report + Environment.NewLine);
        }
    }

    private static string BuildReport(string testName, ProviderFixture provider, Exception ex)
    {
        var frames = (ex.StackTrace ?? string.Empty)
            .Split('\n')
            .Select(line => line.TrimEnd('\r').Trim())
            .Where(line => line.Length > 0)
            .Take(3)
            .ToArray();

        var sb = new StringBuilder();
        sb.AppendLine($"## {testName}");
        sb.AppendLine();
        sb.AppendLine($"- Provider: {provider.Name}");
        sb.AppendLine($"- EF Core assembly version: {EnvironmentInfo.EfCoreAssemblyVersion}");
        sb.AppendLine($"- Runtime: {EnvironmentInfo.RuntimeVersion}");
        sb.AppendLine($"- OS: {EnvironmentInfo.OperatingSystem}");
        sb.AppendLine($"- Exception type: {ex.GetType().FullName}");
        sb.AppendLine($"- InnerException is null: {ex.InnerException is null}");
        sb.AppendLine("- Message:");
        sb.AppendLine("  ```");
        sb.AppendLine($"  {ex.Message}");
        sb.AppendLine("  ```");
        sb.AppendLine("- First three stack frames:");
        sb.AppendLine("  ```");
        foreach (var frame in frames)
        {
            sb.AppendLine($"  {frame}");
        }

        sb.AppendLine("  ```");

        return sb.ToString();
    }
}
