using System.Diagnostics;
using System.Runtime.Versioning;

using GnomeStack.Extras.Strings;

namespace GnomeStack.Standard;

public static partial class Env
{
    private static readonly Lazy<string[]> s_argv = new Lazy<string[]>(Environment.GetCommandLineArgs);
#if NETLEGACY
    private static readonly Lazy<Process> s_process = new Lazy<Process>(Process.GetCurrentProcess);
#endif

    private static readonly Lazy<int> s_getProcessId = new(() =>
    {
#if NETLEGACY
        return s_process.Value.Id;
#else
        return Environment.ProcessId;
#endif
    });

    private static Lazy<bool> isInteractive = new(() =>
    {
        if (!Environment.UserInteractive)
            return false;

        if (Argv.Contains("--non-interactive", StringComparer.OrdinalIgnoreCase) ||
            Argv.Contains("-NonInteractive", StringComparer.OrdinalIgnoreCase))
            return false;

        // false positive of a deference without the !
        if (TryGet("DEBIAN_FRONTEND", out var frontend) &&
            frontend.Equals("noninteractive", StringComparison.OrdinalIgnoreCase))
            return false;

        if (TryGet("CI", out var ci) &&
            ci.Equals("true", StringComparison.OrdinalIgnoreCase))
            return false;

        if (TryGet("TF_BUILD", out var tfBuild) &&
            tfBuild.Equals("true", StringComparison.OrdinalIgnoreCase))
            return false;

        if (TryGet("JENKINS_URL", out var jenkinsUrl) &&
            !jenkinsUrl.IsNullOrWhiteSpace())
            return false;

        return true;
    });

    public static bool Is64BitProcess => Environment.Is64BitProcess;

    [UnsupportedOSPlatform("browser")]
    public static string? ProcessPath => Argv.FirstOrDefault();

    [UnsupportedOSPlatform("browser")]
    public static int ProcessId
    {
        get
        {
#if !NETLEGACY
            if (OperatingSystem.IsBrowser())
                throw new PlatformNotSupportedException("Browser does not support ProcessId.");
#endif

            return s_getProcessId.Value;
        }
    }

    public static IReadOnlyList<string> Argv => s_argv.Value;

    public static string User => Environment.UserName;

    public static string UserDomain => Environment.UserDomainName;

    public static string Host => Environment.MachineName;

    public static string NewLine => Environment.NewLine;

    public static string CurrentDirectory
    {
        get => Environment.CurrentDirectory;
        set => Environment.CurrentDirectory = value;
    }

    public static bool UserInteractive
    {
        get => isInteractive.Value;
        set => isInteractive = new Lazy<bool>(() => value);
    }

    public static string Home => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
}