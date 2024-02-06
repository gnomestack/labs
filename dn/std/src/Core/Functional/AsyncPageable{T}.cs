namespace GnomeStack.Functional;

public abstract class AsyncPageable<T> : IAsyncEnumerable<T>
    where T : notnull
{
    protected AsyncPageable()
        => this.CancellationToken = default;

    protected AsyncPageable(CancellationToken cancellationToken)
    {
        this.CancellationToken = cancellationToken;
    }

    public CancellationToken CancellationToken { get; }

    public static AsyncPageable<T> FromPages(IEnumerable<Page<T>> pages)
        => new StaticPageable(pages);

    public abstract System.Collections.Generic.IAsyncEnumerable<Page<T>> AsPages(
        string? continuationToken = default,
        int? pageSizeHint = default);

    public virtual async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        await foreach (Page<T> page in this.AsPages()
                            .ConfigureAwait(false)
                            .WithCancellation(cancellationToken))
        {
            foreach (T value in page.Values)
            {
                yield return value;
            }
        }
    }

    private sealed class StaticPageable : AsyncPageable<T>
    {
        private readonly IEnumerable<Page<T>> pages;

        public StaticPageable(IEnumerable<Page<T>> pages)
        {
            this.pages = pages;
        }

#pragma warning disable 1998 // async function without await
        public override async IAsyncEnumerable<Page<T>> AsPages(string? continuationToken = default, int? pageSizeHint = default)
#pragma warning restore 1998
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