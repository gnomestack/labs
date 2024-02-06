namespace GnomeStack.Functional;

public abstract class Page<T>
{
    public IReadOnlyList<T> Values { get; }

    public string? ContinuationToken { get; }
}