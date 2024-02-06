using System.Runtime.CompilerServices;

namespace GnomeStack.Extras.Memory;

public static class MemoryExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string AsString(this ReadOnlyMemory<char> memory)
        => memory.Span.AsString();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string AsString(this Memory<char> memory)
        => memory.Span.AsString();
}