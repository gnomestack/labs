using System.Runtime.CompilerServices;

using GnomeStack.Extras.Arrays;
using GnomeStack.Extras.Strings;
using GnomeStack.Standard;

namespace GnomeStack.IO;

public readonly struct PathBuffer
{
    private static readonly ConditionalWeakTable<char[], string> Cache = new();

    private readonly char[] buffer;

    private readonly char pathSeparator;

    public PathBuffer()
    {
        this.buffer = Array.Empty<char>();
        this.Length = 0;
        this.pathSeparator = Path.DirectorySeparatorChar;
    }

    public PathBuffer(char pathSeparator)
    {
        this.buffer = Array.Empty<char>();
        this.Length = 0;
        this.pathSeparator = pathSeparator;
    }

    public PathBuffer(ReadOnlySpan<char> path, char pathSeparator = char.MinValue)
    {
        this.pathSeparator = pathSeparator == char.MinValue ? Path.DirectorySeparatorChar :
            pathSeparator;

        this.buffer = new char[path.Length];
        this.Length = path.Length;

        var l = path.Length;
        for (var i = path.Length - 1; i >= 0; i--)
        {
            char c = path[i];
            if (c == this.pathSeparator)
            {
                l--;
                break;
            }
        }

        path.Slice(0, l).CopyTo(this.buffer);
    }

    public PathBuffer(string path, char pathSeparator = char.MinValue)
        : this(path.AsSpan(), pathSeparator)
    {
    }

    public int Length { get; }

    public bool IsEmpty => this.Length == 0;

    public char this[int index]
    {
        get => this.buffer[index];
        set => this.buffer[index] = value;
    }

    public static implicit operator PathBuffer(string path)
        => new PathBuffer(path.AsSpan());

    public static implicit operator PathBuffer(ReadOnlySpan<char> path)
        => new PathBuffer(path);

    public static implicit operator PathBuffer(PathBuilder path)
        => new PathBuffer(path.ToSpan());

    public static implicit operator PathBuffer(char[] path)
        => new PathBuffer(path);

    public static implicit operator string(PathBuffer path)
        => path.ToString();

    public static implicit operator ReadOnlySpan<char>(PathBuffer path)
        => path.buffer.AsSpan(0, path.Length);

    public PathBuffer Join(ReadOnlySpan<char> path)
    {
        var l = path.Length;
        if (l == 0)
            return this;

        path = Trim(path);
        var span = new char[this.Length + l + 1];
        this.buffer.AsSpan(0, this.Length).CopyTo(span);
        span[this.Length] = this.pathSeparator;
        path.CopyTo(span.AsSpan(this.Length + 1));
        return new PathBuffer(span);
    }

    public PathBuffer Join(string path)
        => this.Join(path.AsSpan());

    public PathBuffer Join(in PathBuffer path)
        => this.Join(path.buffer.AsSpan(0, path.Length));

    public PathBuffer Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2)
    {
        var l1 = path1.Length;
        var l2 = path2.Length;
        if (l1 == 0 && l2 == 0)
            return this;

        path1 = Trim(path1);
        path2 = Trim(path2);
        var span = new char[this.Length + l1 + l2 + 2];
        this.buffer.AsSpan(0, this.Length).CopyTo(span);
        span[this.Length] = this.pathSeparator;
        path1.CopyTo(span.AsSpan(this.Length + 1));
        span[this.Length + l1 + 1] = this.pathSeparator;
        path2.CopyTo(span.AsSpan(this.Length + l1 + 2));
        return new PathBuffer(span);
    }

    public PathBuffer Join(string path1, string path2)
        => this.Join(path1.AsSpan(), path2.AsSpan());

    public PathBuffer Join(in PathBuffer path1, in PathBuffer path2)
        => this.Join(path1.buffer.AsSpan(0, path1.Length), path2.buffer.AsSpan(0, path2.Length));

    public PathBuffer GetDirectoryName()
    {
#if NETLEGACY
        if (this.IsEmpty)
            return default;

        var path = this.buffer.Slice(0, this.Length);
        int end = PathUtil.GetDirectoryNameOffset(path);
        return end >= 0 ? path.Slice(0, end) : ReadOnlySpan<char>.Empty;
#else
        return Path.GetDirectoryName(this.ToSpan());
#endif
    }

    public PathBuffer GetFileName()
    {
#if NETLEGACY
        var path = this.ToSpan();
        int root = GetPathRoot(path).Length;

        // We don't want to cut off "C:\file.txt:stream" (i.e. should be "file.txt:stream")
        // but we *do* want "C:Foo" => "Foo". This necessitates checking for the root.

        int i = Path.DirectorySeparatorChar == Path.AltDirectorySeparatorChar ?
            path.LastIndexOf(Path.DirectorySeparatorChar) :
            path.LastIndexOfAny(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return path.Slice(i < root ? root : i + 1);
#else
        return Path.GetFileName(this.ToSpan());
#endif
    }

    public PathBuffer GetFileNameWithoutExtension()
    {
#if NETLEGACY
        return new PathBuffer(Path.GetFileNameWithoutExtension(this.ToString()), this.pathSeparator);
#else
        return Path.GetFileNameWithoutExtension(this.ToSpan());
#endif
    }

    public PathBuffer GetFullPath()
    {
#if NETLEGACY
        return new PathBuffer(Path.GetFullPath(this.ToString()), this.pathSeparator);
#else
        return Path.GetFullPath(this);
#endif
    }

#if !NETLEGACY
    public PathBuffer GetFullPath(string basePath)
        => Path.GetFullPath(this, basePath);

    public bool IsFullyQualified()
        => Path.IsPathFullyQualified(this.ToSpan());
#endif

    public bool IsRooted()
    {
#if NETLEGACY
        return Path.IsPathRooted(this.ToString());
#else
        return Path.IsPathRooted(this.ToSpan());
#endif
    }

    public bool HasExtension()
    {
#if NETLEGACY
        var l = this.Length;
        if (l == 0)
            return false;

        for (var i = l - 1; i >= 0; i--)
        {
            if (this.buffer[i] == '.')
                return true;

            if (this.buffer[i] == '/' || this.buffer[i] == '\\')
                return false;
        }

        return false;
#else
        return Path.HasExtension(this.buffer.AsSpan(0, this.Length));
#endif
    }

    public PathBuffer ChangeExtension(string extension)
        => Path.ChangeExtension(this, extension);

    public PathBuffer ChangeExtension(ReadOnlySpan<char> extension)
        => Path.ChangeExtension(this, extension.AsString());

    public bool Exists()
    {
#if NET7_0_OR_GREATER
        return Path.Exists(this);
#else
        return File.Exists(this) || Directory.Exists(this);
#endif
    }

    public ReadOnlySpan<char> GetExtension()
    {
        #if NETLEGACY
        var l = this.Length;
        if (l == 0)
            return ReadOnlySpan<char>.Empty;

        for (var i = l - 1; i >= 0; i--)
        {
            var c = this.buffer[i];
            if (c is '/' or '\\')
                return ReadOnlySpan<char>.Empty;

            if (this.buffer[i] == '.')
                return new ReadOnlySpan<char>(this.buffer, i, l - i);
        }

        return ReadOnlySpan<char>.Empty;
#else
        return Path.GetExtension(this.buffer.AsSpan(0, this.Length));
#endif
    }

    public string GetExtensionAsString()
    {
        var l = this.Length;
        if (l == 0)
            return string.Empty;

        for (var i = l - 1; i >= 0; i--)
        {
            var c = this.buffer[i];
            if (c is '/' or '\\')
                return string.Empty;

            if (this.buffer[i] == '.')
                return new string(this.buffer, i, l - i);
        }

        return string.Empty;
    }

    public IEnumerable<string> EnumerateSegments()
        => this.EnumerateSegments(Path.DirectorySeparatorChar);

    public IEnumerable<string> EnumerateSegments(char separator)
    {
        var span = new char[256];
        var bufferIndex = 0;
        for (var i = 0; i < this.Length; i++)
        {
            if (this.buffer[i] == separator)
            {
                if (bufferIndex > 0)
                {
                    yield return new string(span, 0, bufferIndex);
                    bufferIndex = 0;
                }
            }
            else
            {
                span[bufferIndex++] = this.buffer[i];
            }
        }
    }

    public ReadOnlySpan<char> ToSpan()
        => this.buffer.AsSpan(0, this.Length);

    public PathBuffer ToBuffer()
        => new(this.buffer.AsSpan(0, this.Length), this.pathSeparator);

    public override string ToString()
    {
        if (Cache.TryGetValue(this.buffer, out var s))
            return s;

        s = new string(this.buffer, 0, this.Length);
        Cache.Add(this.buffer, s);
        return s;
    }

    public PathBuffer TrimEndingSeparator()
    {
        if (this.Length == 0)
            return this;

#if NETLEGACY
        if (this.pathSeparator is '/' or '\\')
            return new PathBuffer(this.ToString().TrimEnd(new[] { '/', '\\' }), this.pathSeparator);

        return new PathBuffer(this.ToString().TrimEnd(new[] { '/', '\\', this.pathSeparator }), this.pathSeparator);
#else
        var span = this.buffer.AsSpan(0, this.Length);
        if (this.pathSeparator is '/' or '\\')
            return new(span.TrimEnd(new[] { '/', '\\' }));

        return new(span.TrimEnd(new[] { '/', '\\', this.pathSeparator }));
#endif
    }

    
}