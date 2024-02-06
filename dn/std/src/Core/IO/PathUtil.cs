using GnomeStack.Standard;
using GnomeStack.Text;

namespace GnomeStack.IO;

internal static class PathUtil
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
    internal const char WindowsVolumeSeparatorChar = ':';

    // \\?\, \\.\, \??\
    internal const int DevicePrefixLength = 4;

    // \\
    internal const int UncPrefixLength = 2;

    // \\?\UNC\, \\.\UNC\
    internal const int UncExtendedPrefixLength = 8;

    internal static ReadOnlySpan<char> Trim(ReadOnlySpan<char> segment, char pathCharacter)
    {
        var span = pathCharacter is '/' or '\\' ?
            new char[] { '/', '\\' } :
            new char[] { pathCharacter, '/', '\\' };
#if !NETLEGACY
        return segment.Trim(span);
#else
        var s = 0;
        for (var i = 0; i < segment.Length; i++)
        {
            char c = segment[i];
            if (Array.IndexOf(span, c) == -1)
                break;
            s++;
        }

        // we no longer have any characters
        if (s == 0)
            return ReadOnlySpan<char>.Empty;

        segment = segment.Slice(s);
        var l = segment.Length;
        for (var i = segment.Length - 1; i >= 0; i--)
        {
            char c = segment[i];
            if (Array.IndexOf(span, c) == -1)
                break;

            l--;
        }

        if (l == segment.Length)
            return segment;

        return segment.Slice(0, l);
#endif
    }

    internal static ReadOnlySpan<char> TrimStart(ReadOnlySpan<char> segment, char pathCharacter)
    {
        var span = pathCharacter is '/' or '\\' ?
            new char[] { '/', '\\' } :
            new char[] { pathCharacter, '/', '\\' };
        var s = 0;
        foreach (var c in segment)
        {
            if (Array.IndexOf(span, c) == -1)
                break;

            s++;
        }

        if (s == 0)
            return segment;

        if (s == segment.Length - 1)
            return ReadOnlySpan<char>.Empty;

        return segment.Slice(s);
    }

    internal static ReadOnlySpan<char> TrimEnd(ReadOnlySpan<char> segment, char pathCharacter)
    {
        var span = pathCharacter is '/' or '\\' ?
            new char[] { '/', '\\' } :
            new char[] { pathCharacter, '/', '\\' };

        // do not use index base for loop
        var l = segment.Length;
        for (var i = segment.Length - 1; i >= 0; i--)
        {
            char c = segment[i];
            if (Array.IndexOf(span, c) == -1)
                break;

            l--;
        }

        if (l == segment.Length)
            return segment;

        return segment.Slice(0, l);
    }

    internal static bool IsDirectorySeparator(char c)
        => c is '/' or '\\';

    internal static bool IsValidDriveChar(char value)
    {
        return (uint)((value | 0x20) - 'a') <= (uint)('z' - 'a');
    }

    internal static bool IsPathRooted(ReadOnlySpan<char> path)
    {
        if (Os.IsWindows())
        {

        }

        return path.Length > 0 && path[0] == Path.DirectorySeparatorChar;
    }

    internal static ReadOnlySpan<char> GetUnixPathRoot(ReadOnlySpan<char> path)
    {
        if (path.IsEmpty)
            return ReadOnlySpan<char>.Empty;

        return IsPathRooted(path) ?
            UnixDirectorySeparatorChars :
            ReadOnlySpan<char>.Empty;
    }

    internal static bool IsWindowsPathEmpty(ReadOnlySpan<char> path)
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


    internal static int GetUnixRootLength(ReadOnlySpan<char> path)
    {
        return path.Length > 0 && IsDirectorySeparator(path[0]) ? 1 : 0;
    }

   
}