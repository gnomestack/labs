using System.Diagnostics;
using System.Runtime.InteropServices;

using GnomeStack.Text;

using static GnomeStack.IO.PathUtil;

namespace GnomeStack.IO;

public static class PathWin32
{
    private static readonly char[] UnixDirectorySeparatorChars = { '/' };

    internal const string NTPathPrefix = @"\??\";
    internal const string ExtendedPathPrefix = @"\\?\";
    internal const string UncPathPrefix = @"\\";
    internal const string UncExtendedPrefixToInsert = @"?\UNC\";
    internal const string UncExtendedPathPrefix = @"\\?\UNC\";
    internal const string UncNTPathPrefix = @"\??\UNC\";
    internal const string DevicePathPrefix = @"\\.\";
    internal const int MaxShortPath = 260;
    internal const int MaxShortDirectoryPath = 248;
    internal const char WindowsDirectorySeparatorChar = '\\';
    internal const char UnixDirectorySeparatorChar = '/';
    internal const char VolumeSeparatorChar = ':';

    /// <summary><c>\\?\, \\.\, \??\</c>.</summary>
    internal const int DevicePrefixLength = 4;

    /// <summary> <c>\\</c> .</summary>
    internal const int UncPrefixLength = 2;

    /// <summary><c>\\?\UNC\, \\.\UNC\</c>.</summary>
    internal const int UncExtendedPrefixLength = 8;

    public static bool IsPathRooted(ReadOnlySpan<char> path)
    {
        int length = path.Length;
        return (length >= 1 && IsDirectorySeparator(path[0]))
               || (length >= 2 && IsValidDriveChar(path[0]) && path[1] == Path.VolumeSeparatorChar);
    }

    internal static bool IsEmpty(ReadOnlySpan<char> path)
    {
        if (path.IsEmpty)
            return true;

        foreach (char c in path)
        {
            if (c != ' ')
                return false;
        }

        return true;
    }
    
    /// <summary>
    /// Calls GetFullPathName on the given path.
    /// </summary>
    /// <param name="path">The path name. MUST be null terminated after the span.</param>
    /// <param name="builder">Builder that will store the result.</param>
    private static void GetFullPathName(ReadOnlySpan<char> path, ref ValueStringBuilder builder)
    {
        // If the string starts with an extended prefix we would need to remove it from the path before we call GetFullPathName as
        // it doesn't root extended paths correctly. We don't currently resolve extended paths, so we'll just assert here.
        Debug.Assert(IsPartiallyQualified(path) || !IsExtended(path));

        uint result;
        while ((result = Interop.Kernel32.GetFullPathNameW(ref MemoryMarshal.GetReference(path), (uint)builder.Capacity, ref builder.GetPinnableReference(), IntPtr.Zero)) > builder.Capacity)
        {
            // Reported size is greater than the buffer size. Increase the capacity.
            builder.EnsureCapacity(checked((int)result));
        }

        if (result == 0)
        {
            Marshal.
            // Failure, get the error and throw
            int errorCode = Marshal.GetLastPInvokeError();
            if (errorCode == 0)
                errorCode = Interop.Errors.ERROR_BAD_PATHNAME;
            throw Win32Marshal.GetExceptionForWin32Error(errorCode, path.ToString());
        }

        builder.Length = (int)result;
    }

    internal static int GetRootLength(ReadOnlySpan<char> path)
    {
        int pathLength = path.Length;
        int i = 0;

        bool deviceSyntax = IsDevice(path);
        bool deviceUnc = deviceSyntax && IsDeviceUNC(path);

        if ((!deviceSyntax || deviceUnc) && pathLength > 0 && IsDirectorySeparator(path[0]))
        {
            // UNC or simple rooted path (e.g. "\foo", NOT "\\?\C:\foo")
            if (deviceUnc || (pathLength > 1 && IsDirectorySeparator(path[1])))
            {
                // UNC (\\?\UNC\ or \\), scan past server\share

                // Start past the prefix ("\\" or "\\?\UNC\")
                i = deviceUnc ? UncExtendedPrefixLength : UncPrefixLength;

                // Skip two separators at most
                int n = 2;
                while (i < pathLength && (!IsDirectorySeparator(path[i]) || --n > 0))
                    i++;
            }
            else
            {
                // Current drive rooted (e.g. "\foo")
                i = 1;
            }
        }
        else if (deviceSyntax)
        {
            // Device path (e.g. "\\?\.", "\\.\")
            // Skip any characters following the prefix that aren't a separator
            i = DevicePrefixLength;
            while (i < pathLength && !IsDirectorySeparator(path[i]))
                i++;

            // If there is another separator take it, as long as we have had at least one
            // non-separator after the prefix (e.g. don't take "\\?\\", but take "\\?\a\")
            if (i < pathLength && i > DevicePrefixLength && IsDirectorySeparator(path[i]))
                i++;
        }
        else if (pathLength >= 2
            && path[1] == VolumeSeparatorChar
            && IsValidDriveChar(path[0]))
        {
            // Valid drive specified path ("C:", "D:", etc.)
            i = 2;

            // If the colon is followed by a directory separator, move past it (e.g "C:\")
            if (pathLength > 2 && IsDirectorySeparator(path[2]))
                i++;
        }

        return i;
    }

    /// <summary>
    /// Returns true if the path uses any of the DOS device path syntaxes. <c>("\\.\", "\\?\", or "\??\")</c>.
    /// </summary>
    internal static bool IsDevice(ReadOnlySpan<char> path)
    {
        // If the path begins with any two separators is will be recognized and normalized and prepped with
        // "\??\" for internal usage correctly. "\??\" is recognized and handled, "/??/" is not.
        return IsExtended(path)
               ||
               (
                   path.Length >= DevicePrefixLength
                   && IsDirectorySeparator(path[0])
                   && IsDirectorySeparator(path[1])
                   && (path[2] == '.' || path[2] == '?')
                   && IsDirectorySeparator(path[3]));
    }

    /// <summary>
    /// Returns true if the path is a device UNC <c>(\\?\UNC\, \\.\UNC\)</c>.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>a boolean.</returns>
    internal static bool IsDeviceUNC(ReadOnlySpan<char> path)
    {
        return path.Length >= UncExtendedPrefixLength
               && IsDevice(path)
               && IsDirectorySeparator(path[7])
               && path[4] == 'U'
               && path[5] == 'N'
               && path[6] == 'C';
    }

    /// <summary>
    /// Returns true if the path uses the canonical form of extended syntax ("\\?\" or "\??\"). If the
    /// path matches exactly (cannot use alternate directory separators) Windows will skip normalization
    /// and path length checks.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>a boolean.</returns>
    internal static bool IsExtended(ReadOnlySpan<char> path)
    {
        // While paths like "//?/C:/" will work, they're treated the same as "\\.\" paths.
        // Skipping of normalization will *only* occur if back slashes ('\') are used.
        return path.Length >= DevicePrefixLength
               && path[0] == '\\'
               && (path[1] == '\\' || path[1] == '?')
               && path[2] == '?'
               && path[3] == '\\';
    }

    /// <summary>
    /// Returns true if the path specified is relative to the current drive or working directory.
    /// Returns false if the path is fixed to a specific drive or UNC path.  This method does no
    /// validation of the path (URIs will be returned as relative as a result).
    /// </summary>
    /// <remarks>
    /// Handles paths that use the alternate directory separator.  It is a frequent mistake to
    /// assume that rooted paths (Path.IsPathRooted) are not relative.  This isn't the case.
    /// "C:a" is drive relative- meaning that it will be resolved against the current directory
    /// for C: (rooted, but relative). "C:\a" is rooted and not relative (the current directory
    /// will not be used to modify the path).
    /// </remarks>
    internal static bool IsPartiallyQualified(ReadOnlySpan<char> path)
    {
        if (path.Length < 2)
        {
            // It isn't fixed, it must be relative.  There is no way to specify a fixed
            // path with one character (or less).
            return true;
        }

        if (IsDirectorySeparator(path[0]))
        {
            // There is no valid way to specify a relative path with two initial slashes or
            // \? as ? isn't valid for drive relative paths and \??\ is equivalent to \\?\
            return !(path[1] == '?' || IsDirectorySeparator(path[1]));
        }

        // The only way to specify a fixed path that doesn't begin with two slashes
        // is the drive, colon, slash format- i.e. C:\
        return !((path.Length >= 3)
            && (path[1] == VolumeSeparatorChar)
            && IsDirectorySeparator(path[2])
            // To match old behavior we'll check the drive character for validity as the path is technically
            // not qualified if you don't have a valid drive. "=:\" is the "=" file's default data stream.
            && IsValidDriveChar(path[0]));
        }

    internal static ReadOnlySpan<char> NormalizeWindowsDirectorySeparators(ReadOnlySpan<char> path)
    {
        if (path.IsEmpty)
            return path;

        char current;

        // Make a pass to see if we need to normalize so we can potentially skip allocating
        bool normalized = true;

        for (int i = 0; i < path.Length; i++)
        {
            current = path[i];
            if (IsDirectorySeparator(current)
                && (current != WindowsDirectorySeparatorChar
                    // Check for sequential separators past the first position (we need to keep initial two for UNC/extended)
                    || (i > 0 && i + 1 < path.Length && IsDirectorySeparator(path[i + 1]))))
            {
                normalized = false;
                break;
            }
        }

        if (normalized)
            return path;

        using var builder = new ValueStringBuilder(stackalloc char[MaxShortPath]);

        int start = 0;
        if (IsDirectorySeparator(path[start]))
        {
            start++;
            builder.Append(WindowsDirectorySeparatorChar);
        }

        for (int i = start; i < path.Length; i++)
        {
            current = path[i];

            // If we have a separator
            if (IsDirectorySeparator(current))
            {
                // If the next is a separator, skip adding this
                if (i + 1 < path.Length && IsDirectorySeparator(path[i + 1]))
                {
                    continue;
                }

                // Ensure it is the primary separator
                current = WindowsDirectorySeparatorChar;
            }

            builder.Append(current);
        }

        var span = new char[builder.Length];
        builder.AsSpan().CopyTo(span);

        return span;
    }
}