using System.Runtime.InteropServices;

namespace GnomeStack.Standard;

#pragma warning disable S3400 // Methods should not return constants
public static partial class Os
{
    private static readonly Lazy<OsReleaseInfo> s_lazy = new(CreateOsReleaseInfo);

    // based on code from nuke.build
    private static readonly Lazy<bool> s_isWsl = new(() =>
    {
        if (!IsLinux())
            return false;

        try
        {
            var version = File.ReadAllText("/proc/version");
            return version.Contains("Microsoft", StringComparison.OrdinalIgnoreCase);
        }
        catch (IOException)
        {
            return false;
        }
    });

    private static readonly Lazy<bool> s_isWindowsServer = new(() => IsWindows() && Release.Variant.Equals("Server", StringComparison.OrdinalIgnoreCase));

    public static OsReleaseInfo Release => s_lazy.Value;

    public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static bool IsWsl() => s_isWsl.Value;

    public static bool IsWindowsBuildVersionAtLeast(int major, int minor = 0, int build = 0, int revision = 0)
    {
        if (!IsWindows())
            return false;

        var v = Environment.OSVersion.Version;
        return v.Major > major ||
               (v.Major == major && v.Minor > minor) ||
               (v.Major == major && v.Minor == minor && v.Build > build) ||
               (v.Major == major && v.Minor == minor && v.Build == build && v.Revision >= revision);
    }

    public static bool IsWindowsVersionAtLeast(int major, int minor = 0)
    {
        if (!IsWindows() || IsWindowsServer())
            return false;

        var v = Release.Version;
        return v.Major > major ||
               (v.Major == major && v.Minor > minor);
    }

    public static bool IsWindowsServerVersionAtLeast(int major, int minor = 0)
    {
        if (!IsWindowsServer())
            return false;

        var v = Release.Version;
        return v.Major > major ||
               (v.Major == major && v.Minor > minor);
    }

    public static bool IsWindowsServer() => s_isWindowsServer.Value;

    public static bool IsMacOS() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

#if NET8_0_OR_GREATER
    public static bool IsWasi() => OperatingSystem.IsWasi();
#else
    public static bool IsWasi() => false;
#endif

#if NET5_0_OR_GREATER
    public static bool IsMacCatalyst() => OperatingSystem.IsMacCatalyst();

    public static bool IsIOS() => OperatingSystem.IsIOS();

    public static bool IsTvOS() => OperatingSystem.IsTvOS();

    public static bool IsBrowser() => OperatingSystem.IsBrowser();

    public static bool IsFreeBSD() => OperatingSystem.IsFreeBSD();

    public static bool IsAndroid() => OperatingSystem.IsAndroid();

#else
    public static bool IsMacCatalyst() => false;

    public static bool IsIOS() => false;

    public static bool IsTvOS() => false;

    public static bool IsBrowser() => false;

    public static bool IsFreeBSD() => false;

    public static bool IsAndroid() => false;

#endif
    public static bool IsUbuntu() => Release.Id.Equals("ubuntu", StringComparison.OrdinalIgnoreCase);

    public static bool IsDebian() => Release.Id.Equals("debian", StringComparison.OrdinalIgnoreCase);

    public static bool IsDebianLike()
        => Release.Id.Equals("debian", StringComparison.OrdinalIgnoreCase) ||
           Release.IdLike?.Equals("debian", StringComparison.OrdinalIgnoreCase) == true ||
           Release.IdLike?.Equals("ubuntu", StringComparison.OrdinalIgnoreCase) == true;

    public static bool IsRedHat() => Release.Id.Equals("rhel", StringComparison.OrdinalIgnoreCase);

    public static bool IsCentOS() => Release.Id.Equals("centos", StringComparison.OrdinalIgnoreCase);

    public static bool IsFedora() => Release.Id.Equals("fedora", StringComparison.OrdinalIgnoreCase);

    public static bool IsSUSE() => Release.Id.Equals("suse", StringComparison.OrdinalIgnoreCase);

    public static bool IsOpenSUSE() => Release.Id.Equals("opensuse", StringComparison.OrdinalIgnoreCase);

    public static bool IsAlpine() => Release.Id.Equals("alpine", StringComparison.OrdinalIgnoreCase);

    public static bool IsArch() => Release.Id.Equals("arch", StringComparison.OrdinalIgnoreCase);

    private static OsReleaseInfo CreateOsReleaseInfo()
    {
        var v = Environment.OSVersion.Version;
#if NET8_0_OR_GREATER
        if (OperatingSystem.IsWasi())
        {
            var os = new OsReleaseInfo();
            os.Id = "wasi";
            os.Name = "WASI";
            os.VersionLabel = v.ToString();
            os.VersionId = v.ToString();
            os.PrettyName = $"{os.Name} {os.VersionLabel}";
            return os;
        }
#endif
#if NET5_0_OR_GREATER
        if (OperatingSystem.IsWindows())
            return GetWindowsOsVersion();

        if (OperatingSystem.IsMacOS())
            return GetMacOsReleaseInfo();

        if (OperatingSystem.IsLinux())
            return GetLinuxOsRelease();

        if (OperatingSystem.IsMacCatalyst())
            return GetMacCatalystReleaseInfo();

        if (OperatingSystem.IsIOS())
            return GetIOSReleaseInfo();

        if (OperatingSystem.IsTvOS())
            return GetTvOsReleaseInfo();

        if (OperatingSystem.IsBrowser())
        {
            var os = new OsReleaseInfo();
            os.Id = "browser";
            os.Name = "Browser";
            os.VersionLabel = v.ToString();
            os.VersionId = v.ToString();
            os.PrettyName = $"{os.Name} {os.VersionLabel}";
            return os;
        }

        if (OperatingSystem.IsFreeBSD())
        {
            var os = new OsReleaseInfo();
            os.Id = "freebsd";
            os.Name = "FreeBSD";
            os.VersionLabel = v.ToString();
            os.VersionId = v.ToString();
            os.PrettyName = $"{os.Name} {os.VersionLabel}";
            return os;
        }

        if (OperatingSystem.IsAndroid())
        {
            var os = new OsReleaseInfo();
            os.Id = "android";
            os.Name = "Android";
            os.VersionLabel = v.ToString();
            os.VersionId = v.ToString();
            os.PrettyName = $"{os.Name} {os.VersionLabel}";
            return os;
        }
#else
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return GetWindowsOsVersion();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
             return GetLinuxOsRelease();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return GetMacOsReleaseInfo();
#endif
        var uos = new OsReleaseInfo();
        uos.Id = "unknown";
        uos.Name = "Unknown";
        uos.VersionLabel = v.ToString();
        uos.VersionId = v.ToString();
        return uos;
    }
}

public class OsReleaseInfo
{
    private readonly Dictionary<string, string> props = new(StringComparer.OrdinalIgnoreCase);

    public string Id { get; internal set; } = string.Empty;

    public string IdLike { get; internal set; } = string.Empty;

    public string Name { get; internal set; } = string.Empty;

    public Version Version { get; internal set; } = new(0, 0);

    public string VersionLabel { get; internal set; } = string.Empty;

    public string VersionCodename { get; internal set; } = string.Empty;

    public string VersionId { get; internal set; } = string.Empty;

    public string PrettyName { get; set; } = string.Empty;

    public string AnsiColor { get; set; } = string.Empty;

    public string CpeName { get; set; } = string.Empty;

    public string HomeUrl { get; set; } = string.Empty;

    public string DocumentationUrl { get; set; } = string.Empty;

    public string SupportUrl { get; set; } = string.Empty;

    public string BugReportUrl { get; set; } = string.Empty;

    public string PrivacyPolicyUrl { get; set; } = string.Empty;

    public string BuildId { get; set; } = string.Empty;

    public string Variant { get; set; } = string.Empty;

    public string VariantId { get; set; } = string.Empty;

    public string? this[string key]
    {
        get => this.props.TryGetValue(key, out var value) ? value : null;
        set
        {
            if (value is null)
            {
                this.props.Remove(key);
            }
            else
            {
                this.props[key] = value;
            }
        }
    }
}