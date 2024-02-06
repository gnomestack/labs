using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace GnomeStack.Standard;

public static partial class Env
{
    private static readonly string Key = IsWindows ? "Path" : "PATH";

    public static EnvPaths Paths { get; } = new();

    public static void AddPath(string path, bool prepend = false, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
    {
        var paths = SplitPath(target);
        if (InternalHasPath(path, paths))
            return;

        var current = GetPath(target);
        if (string.IsNullOrWhiteSpace(current))
        {
            SetPath(path, target);
            return;
        }

        if (prepend)
        {
            var newPath = $"{path}{Path.PathSeparator}{GetPath(target)}";
            SetPath(newPath, target);
        }
        else
        {
            var newPath = $"{GetPath(target)}{Path.PathSeparator}{path}";
            SetPath(newPath, target);
        }
    }

    public static string? GetPath(EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
        => Environment.GetEnvironmentVariable(Key, target);

    /// <summary>
    /// Sets the PATH environment variable.
    /// </summary>
    /// <param name="path">The path value to set for the environment.</param>
    /// <param name="target">The target level to set. On non windows systems, this is currently ignored.</param>
    public static void SetPath(string path, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
    {
#if NETLEGACY
        Environment.SetEnvironmentVariable(Key, path, target);
#else
        if (OperatingSystem.IsWindows())
        {
            Environment.SetEnvironmentVariable(Key, path, target);
        }
        else
        {
            Environment.SetEnvironmentVariable(Key, path, EnvironmentVariableTarget.Process);
        }
#endif
    }

    public static bool HasPath(string path, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
    {
        return InternalHasPath(path, SplitPath(target));
    }

    public static void RemovePath(string path, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
    {
        var paths = SplitPath(target).ToArray();
        if (!InternalHasPath(path, paths))
            return;

        var newPath = string.Join(Path.PathSeparator.ToString(), paths.Where(p => !p.Equals(path, StringComparison.OrdinalIgnoreCase)));
        SetPath(newPath, target);
    }

    private static bool InternalHasPath(string path, IEnumerable<string> paths)
    {
        if (IsWindows)
        {
            foreach (var p in paths)
            {
                if (p.Equals(path, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }
        else
        {
            foreach (var p in paths)
            {
                if (p.Equals(path, StringComparison.Ordinal))
                    return true;
            }
        }

        return false;
    }

    [SuppressMessage("Critical Code Smell", "S3218:Inner class members should not shadow outer class \"static\" or type members")]
    public sealed class EnvPaths : IEnumerable<string>
    {
        public void Add(string path, bool prepend = false, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
            => AddPath(path, prepend, target);

        public bool Contains(string path, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
            => HasPath(path, target);

        // ReSharper disable once MemberHidesStaticFromOuterClass
        public void Remove(string path, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
            => RemovePath(path, target);

        public IEnumerator<string> GetEnumerator()
        {
            foreach (var path in SplitPath())
            {
                yield return path;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();
    }
}