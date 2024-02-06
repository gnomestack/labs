// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#nullable enable

namespace GnomeStack.Text;

#pragma warning disable SA1202
[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1615:Element return value should be documented")]
[SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1405:Debug.Assert should provide message text")]
[SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1513:Closing brace should be followed by blank line")]
[SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order")]
[SuppressMessage("ReSharper", "LocalVariableHidesMember")]
[SuppressMessage("Minor Code Smell", "S4136:Method overloads should be grouped together")]
[SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:Elements should be ordered by access")]
[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1623:Property summary documentation should match accessors")]
[SuppressMessage("ReSharper", "PartialTypeWithSinglePart")]
internal ref partial struct ValueStringBuilder
{
    private char[]? arrayToReturnToPool;
    private Span<char> chars;
    private int pos;

    public ValueStringBuilder(Span<char> initialBuffer)
    {
        this.arrayToReturnToPool = null;
        this.chars = initialBuffer;
        this.pos = 0;
    }

    public ValueStringBuilder(int initialCapacity)
    {
        this.arrayToReturnToPool = ArrayPool<char>.Shared.Rent(initialCapacity);
        this.chars = this.arrayToReturnToPool;
        this.pos = 0;
    }

    public int Length
    {
        get => this.pos;
        set
        {
            Debug.Assert(value >= 0, "value >= 0");
            Debug.Assert(value <= this.chars.Length, "value <= chars.Length");
            this.pos = value;
        }
    }

    public int Capacity => this.chars.Length;

    public void EnsureCapacity(int capacity)
    {
        // This is not expected to be called this with negative capacity
        Debug.Assert(capacity >= 0, "capacity >= 0");

        // If the caller has a bug and calls this with negative capacity, make sure to call Grow to throw an exception.
        if ((uint)capacity > (uint)this.chars.Length)
            this.Grow(capacity - this.pos);
    }

    /// <summary>
    /// Get a pinnable reference to the builder.
    /// Does not ensure there is a null char after <see cref="Length"/>
    /// This overload is pattern matched in the C# 7.3+ compiler so you can omit
    /// the explicit method call, and write eg "fixed (char* c = builder)".
    /// </summary>
    public ref char GetPinnableReference()
    {
        return ref MemoryMarshal.GetReference(this.chars);
    }

    /// <summary>
    /// Get a pinnable reference to the builder.
    /// </summary>
    /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
    public ref char GetPinnableReference(bool terminate)
    {
        if (terminate)
        {
            EnsureCapacity(Length + 1);
            chars[Length] = '\0';
        }
        return ref MemoryMarshal.GetReference(chars);
    }

    public ref char this[int index]
    {
        get
        {
            Debug.Assert(index < pos);
            return ref chars[index];
        }
    }

    public override string ToString()
    {
        string s = chars.Slice(0, pos).ToString();
        Dispose();
        return s;
    }

    /// <summary>Returns the underlying storage of the builder.</summary>
    public Span<char> RawChars => chars;

    /// <summary>
    /// Returns a span around the contents of the builder.
    /// </summary>
    /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/>.</param>
    public ReadOnlySpan<char> AsSpan(bool terminate)
    {
        if (terminate)
        {
            EnsureCapacity(Length + 1);
            chars[Length] = '\0';
        }
        return chars.Slice(0, pos);
    }

    public ReadOnlySpan<char> AsSpan() => chars.Slice(0, pos);

    public ReadOnlySpan<char> AsSpan(int start) => chars.Slice(start, pos - start);

    public ReadOnlySpan<char> AsSpan(int start, int length) => chars.Slice(start, length);

    public bool TryCopyTo(Span<char> destination, out int charsWritten)
    {
        if (chars.Slice(0, pos).TryCopyTo(destination))
        {
            charsWritten = pos;
            Dispose();
            return true;
        }
        else
        {
            charsWritten = 0;
            Dispose();
            return false;
        }
    }

    public void Insert(int index, char value, int count)
    {
        if (pos > chars.Length - count)
        {
            Grow(count);
        }

        int remaining = pos - index;
        chars.Slice(index, remaining).CopyTo(chars.Slice(index + count));
        chars.Slice(index, count).Fill(value);
        pos += count;
    }

    public void Insert(int index, string? s)
    {
        if (s == null)
        {
            return;
        }

        int count = s.Length;

        if (pos > (chars.Length - count))
        {
            Grow(count);
        }

        int remaining = pos - index;
        chars.Slice(index, remaining).CopyTo(chars.Slice(index + count));
        s
#if !NETCOREAPP
            .AsSpan()
#endif
            .CopyTo(chars.Slice(index));
        pos += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c)
    {
        int pos = this.pos;
        Span<char> chars = this.chars;
        if ((uint)pos < (uint)chars.Length)
        {
            chars[pos] = c;
            this.pos = pos + 1;
        }
        else
        {
            GrowAndAppend(c);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(string? s)
    {
        if (s == null)
        {
            return;
        }

        int pos = this.pos;

        // very common case, e.g. appending strings from NumberFormatInfo like separators, percent symbols, etc.
        if (s.Length == 1 && (uint)pos < (uint)chars.Length)
        {
            chars[pos] = s[0];
            this.pos = pos + 1;
        }
        else
        {
            AppendSlow(s);
        }
    }

    private void AppendSlow(string s)
    {
        int pos = this.pos;
        if (pos > chars.Length - s.Length)
        {
            Grow(s.Length);
        }

        s
#if !NETCOREAPP
            .AsSpan()
#endif
            .CopyTo(chars.Slice(pos));
        this.pos += s.Length;
    }

    public void Append(char c, int count)
    {
        if (pos > chars.Length - count)
        {
            Grow(count);
        }

        Span<char> dst = chars.Slice(pos, count);
        for (int i = 0; i < dst.Length; i++)
        {
            dst[i] = c;
        }
        pos += count;
    }

    public unsafe void Append(char* value, int length)
    {
        int pos = this.pos;
        if (pos > chars.Length - length)
        {
            Grow(length);
        }

        Span<char> dst = chars.Slice(this.pos, length);
        for (int i = 0; i < dst.Length; i++)
        {
            dst[i] = *value++;
        }
        this.pos += length;
    }

    public void Append(scoped ReadOnlySpan<char> value)
    {
        int pos = this.pos;
        if (pos > chars.Length - value.Length)
        {
            Grow(value.Length);
        }

        value.CopyTo(chars.Slice(this.pos));
        this.pos += value.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<char> AppendSpan(int length)
    {
        int origPos = pos;
        if (origPos > chars.Length - length)
        {
            Grow(length);
        }

        pos = origPos + length;
        return chars.Slice(origPos, length);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowAndAppend(char c)
    {
        Grow(1);
        Append(c);
    }

    /// <summary>
    /// Resize the internal buffer either by doubling current buffer size or
    /// by adding <paramref name="additionalCapacityBeyondPos"/> to
    /// <see cref="pos"/> whichever is greater.
    /// </summary>
    /// <param name="additionalCapacityBeyondPos">
    /// Number of chars requested beyond current position.
    /// </param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int additionalCapacityBeyondPos)
    {
        Debug.Assert(additionalCapacityBeyondPos > 0);
        Debug.Assert(pos > chars.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");

        const uint ArrayMaxLength = 0x7FFFFFC7; // same as Array.MaxLength

        // Increase to at least the required size (_pos + additionalCapacityBeyondPos), but try
        // to double the size if possible, bounding the doubling to not go beyond the max array length.
        int newCapacity = (int)Math.Max(
            (uint)(pos + additionalCapacityBeyondPos),
            Math.Min((uint)chars.Length * 2, ArrayMaxLength));

        // Make sure to let Rent throw an exception if the caller has a bug and the desired capacity is negative.
        // This could also go negative if the actual required length wraps around.
        char[] poolArray = ArrayPool<char>.Shared.Rent(newCapacity);

        chars.Slice(0, pos).CopyTo(poolArray);

        char[]? toReturn = arrayToReturnToPool;
        chars = arrayToReturnToPool = poolArray;
        if (toReturn != null)
        {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        char[]? toReturn = arrayToReturnToPool;
        this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
        if (toReturn != null)
        {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }
}