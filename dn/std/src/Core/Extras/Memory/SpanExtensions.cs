using System.Runtime.CompilerServices;

namespace GnomeStack.Extras.Memory;

public static class SpanExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string AsString(this ReadOnlySpan<char> span)
    {
#if NETLEGACY
        return new string(span.ToArray());
#else
        return span.ToString();
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string AsString(this Span<char> span)
    {
#if NETLEGACY
        return new string(span.ToArray());
#else
        return span.ToString();
#endif
    }
}