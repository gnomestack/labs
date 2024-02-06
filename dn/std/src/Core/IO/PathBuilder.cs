using System.Runtime.CompilerServices;

using GnomeStack.Extras.Memory;
using GnomeStack.Standard;

namespace GnomeStack.IO;

public class PathBuilder
{
    private char[] buffer;

    private char pathSeparator = Path.DirectorySeparatorChar;

    public PathBuilder()
    {
        this.buffer = new char[260];
        this.Length = 0;
    }

    public PathBuilder(int capacity)
    {
        this.buffer = new char[capacity];
        this.Length = 0;
    }

    public PathBuilder(char pathSeparator)
    {
        this.buffer = new char[260];
        this.pathSeparator = pathSeparator;
        this.Length = 0;
    }

    public PathBuilder(ReadOnlySpan<char> path, char pathSeparator = char.MinValue)
    {
        this.pathSeparator = pathSeparator == char.MinValue ? Path.DirectorySeparatorChar : pathSeparator;
        this.buffer = new char[Math.Min(path.Length + 100, 260)];
        this.Length = path.Length;
        path.CopyTo(this.buffer);
    }

    public PathBuilder(string path, char pathSeparator = char.MinValue)
        : this(path.AsSpan(), pathSeparator)
    {
    }

    public int Length { get; private set; }

    public char this[int index]
    {
        get => this.buffer[index];
        set => this.buffer[index] = value;
    }

    public static PathBuilder From(string path, char pathSeparator = char.MinValue)
        => new(path);

    public static PathBuilder From(ReadOnlySpan<char> path, char pathSeparator = char.MinValue)
        => new(path);
    
    public PathBuffer GetDirectoryName()

    public bool HasExtension()
    {
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
    }

    public void SetExtension(string extension)
    {
        if (extension.Length == 0)
            return;

        int max = this.Length - 1;
        int pos = max;
        while (pos > 0 && this.buffer[pos] is not '.')
        {
            if (this.buffer[pos] is '/' or '\\')
            {
                pos = max;
                break;
            }

            pos--;
        }

        if (pos == 0)
            return;

        if (pos == max)
        {
            if (this.buffer.Length < this.Length + extension.Length)
                this.EnsureCapacity(this.Length + this.buffer.Length);

            extension.CopyTo(0, this.buffer, this.Length, extension.Length);
            this.Length += extension.Length;
            return;
        }

        if (this.buffer.Length < this.Length + extension.Length)
            this.EnsureCapacity(this.Length + extension.Length);

        Array.Copy(this.buffer, pos, this.buffer, this.Length, max - pos + 1);
        this.Length += max - pos + 1;
        extension.CopyTo(0, this.buffer, pos, extension.Length);
        this.Length += extension.Length;
    }

    public bool Pop()
    {
        if (this.Length == 0)
            return false;

        this.Length--;
        while (this.Length > 0 && this.buffer[this.Length] is not '/' and not '\\')
        {
            this.Length--;
        }

        return true;
    }

    public PathBuilder Push(string value)
        => this.Push(value.AsSpan());

    public PathBuilder Push(ReadOnlySpan<char> value)
    {
        value = PathUtil.TrimEnd(value, this.pathSeparator);

        if (value.Length == 0)
            return this;

        // replace the buffer if the value is rooted
        if (this.Length == 0 || PathUtil.IsPathRooted(value))
        {
            this.EnsureCapacity(value.Length);
            value.CopyTo(this.buffer);
            this.Length = value.Length;
            return this;
        }

        this.EnsureCapacity(value.Length + 1);
        if (this.buffer[this.Length - 1] != this.pathSeparator)
        {
            this.buffer[this.Length] = this.pathSeparator;
            this.Length++;
        }

        value.CopyTo(this.buffer.AsSpan(this.Length));

        return this;
    }

    public void Clear()
    {
        this.Length = 0;
        Array.Clear(this.buffer, 0, this.buffer.Length);
    }

    public PathBuffer ToPath()
        => new PathBuffer(this.buffer.AsSpan(0, this.Length), this.pathSeparator);

    public ReadOnlySpan<char> ToSpan()
    {
        return this.buffer.AsSpan(0, this.Length);
    }

    public ReadOnlySpan<char> ToUnixSpan()
    {
        var span = this.buffer.AsSpan(0, this.Length);
        for (var i = 0; i < span.Length; i++)
        {
            if (span[i] == '\\')
                span[i] = '/';
        }

        return span;
    }

    public ReadOnlySpan<char> ToWindowsSpan()
    {
        var span = this.buffer.AsSpan(0, this.Length);
        for (var i = 0; i < span.Length; i++)
        {
            if (span[i] == '/')
                span[i] = '\\';
        }

        return span;
    }

    public override string ToString()
    {
        return new string(this.buffer, 0, this.Length);
    }

    public string ToUnixString()
    {
        var span = this.buffer.AsSpan(0, this.Length);
        for (var i = 0; i < span.Length; i++)
        {
            if (span[i] == '\\')
                span[i] = '/';
        }

        return span.AsString();
    }

    public string ToWindowsString()
    {
        var span = this.buffer.AsSpan(0, this.Length);
        for (var i = 0; i < span.Length; i++)
        {
            if (span[i] == '/')
                span[i] = '\\';
        }

        return span.AsString();
    }

    private void EnsureCapacity(int capacity)
    {
        if (this.buffer.Length < capacity)
        {
            var newBuffer = new char[capacity];
            Array.Copy(this.buffer, newBuffer, this.Length);
            this.buffer = newBuffer;
        }
    }
    
    private stat
}