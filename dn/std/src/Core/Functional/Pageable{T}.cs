using System.Collections;

namespace GnomeStack.Functional;

public abstract class Pageable<T> : IEnumerable<T>
{
    protected Pageable()
        => this.CancellationToken = default;

    protected Pageable(CancellationToken cancellationToken)
    {
        this.CancellationToken = cancellationToken;
    }

    public CancellationToken CancellationToken { get; }

    public static Pageable<T> FromPages(IEnumerable<Page<T>> pages)
        => new StaticPageable(pages);

    public abstract IEnumerable<Page<T>> AsPages(
        string? continuationToken = default,
        int? pageSizeHint = default);

    public virtual IEnumerator<T> GetEnumerator()
    {
        foreach (Page<T> page in this.AsPages())
        {
            foreach (T value in page.Values)
            {
                yield return value;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => this.GetEnumerator();

    private sealed class StaticPageable : Pageable<T>
    {
        private readonly IEnumerable<Page<T>> pages;

        public StaticPageable(IEnumerable<Page<T>> pages)
        {
            this.pages = pages;
        }

        public override IEnumerable<Page<T>> AsPages(string? continuationToken = default, int? pageSizeHint = default)
        {
            var shouldReturnPages = continuationToken == null;

            foreach (var page in this.pages)
            {
                if (shouldReturnPages)
                {
                    yield return page;
                }
                else
                {
                    if (continuationToken == page.ContinuationToken)
                    {
                        shouldReturnPages = true;
                    }
                }
            }
        }
    }
}