using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace GnomeStack.Standard;

public static partial class Os
{
    [SupportedOSPlatform("windows")]
    private static OsReleaseInfo GetWindowsOsVersion()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException($"{nameof(GetWindowsOsVersion)} is only supported on Windows.");
        }

        var os = new OsReleaseInfo();
        if (Interop.NtDll.RtlGetVersionEx(out var osvi) != 0)
        {
            throw new InvalidOperationException("Failed to get Windows version information.");
        }

        bool isServer = osvi.wProductType == 0x00000003;
        os.BuildId = Environment.OSVersion.Version.ToString();
        var v = Environment.OSVersion.Version;
        os.Variant = string.Empty;
        os.VariantId = string.Empty;
        os.Id = "windows";
        os.IdLike = "windows";
        os.Name = "Windows";

        if (isServer)
        {
            os.VariantId = "server";
            os.Variant = "Server";

            switch (v.Major)
            {
                case 10:
                    {
                        os.VariantId = "server";
                        os.Variant = "Server";
                        if (v.Build >= 20348)
                        {
                            os.Version = new Version(2022, 0);
                            os.VersionLabel = "Server 2022";
                            os.VersionId = "2022";
                        }
                        else if (v.Build >= 17763)
                        {
                            os.Version = new Version(2019, 0);
                            os.VersionLabel = "Server 2019";
                            os.VersionId = "2019";
                        }
                        else if (v.Build >= 14393)
                        {
                            os.Version = new Version(2016, 0);
                            os.VersionLabel = "Server 2016";
                            os.VersionId = "2016";
                            os.VersionCodename = "Redstone 1";
                        }
                    }

                    break;

                case 6:
                    {
                        switch (v.Minor)
                        {
                            case 0:
                                os.VersionId = "2008";
                                os.VersionLabel = "Server 2008";
                                os.Version = new Version(2008, 0);
                                break;
                            case 1:
                                os.VersionId = "2008-r2";
                                os.VersionLabel = "Server 2008 R2";
                                os.Version = new Version(2008, 2);
                                break;
                            case 2:
                                os.VersionId = "2012";
                                os.VersionLabel = "Server 2012";
                                os.Version = new Version(2012, 0);
                                break;

                            case 3:
                                os.VersionId = "2012-r2";
                                os.VersionLabel = "Server 2012 R2";
                                os.Version = new Version(2012, 2);
                                break;
                            default:
                                throw new NotSupportedException("Unknown Windows version");
                        }
                    }

                    break;

                default:
                    throw new NotSupportedException("Unknown or unsupported Windows version");
            }
        }
        else
        {
            switch (v.Major)
            {
                case 10:
                    {
                        if (v.Build >= 20000)
                        {
                            os.VersionLabel = "11";
                            os.VersionId = "11";
                            os.Version = new Version(11, 0);
                        }
                        else
                        {
                            os.VersionLabel = "10";
                            os.VersionId = "10";
                            os.Version = new Version(10, 0);
                        }
                    }

                    break;

                case 6:
                    {
                        switch (v.Minor)
                        {
                            case 0:
                                os.VersionLabel = "Vista";
                                os.VersionId = "vista";
                                os.Version = new Version(6, 0);
                                break;

                            case 1:
                                os.VersionId = "7";
                                os.VersionId = "7";
                                os.Version = new Version(7, 0);
                                break;

                            case 2:
                                os.VersionId = "8";
                                os.VersionId = "8";
                                os.Version = new Version(8, 0);
                                break;

                            case 3:
                                os.VersionId = "8.1";
                                os.VersionId = "8.1";
                                os.Version = new Version(8, 1);
                                break;
                            default:
                                throw new NotSupportedException("Unknown Windows version");
                        }
                    }

                    break;

                default:
                    throw new NotSupportedException("Unknown or unsupported Windows version");
            }
        }

        os.PrettyName = $"{os.Name} {os.VersionLabel} ({os.BuildId})";
        return os;
    }
}