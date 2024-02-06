namespace GnomeStack.Functional;

public class ResultException : System.Exception
{
    public ResultException()
    {
    }

    public ResultException(string message)
        : base(message)
    {
    }

    public ResultException(string message, System.Exception inner)
        : base(message, inner)
    {
    }

    public static void ThrowIfError(IResult result)
    {
        if (result.IsError)
            throw new OptionException($"{result.GetType().FullName} Failed with an error.");
    }

    public static void ThrowIfError<TValue, TError>(IResult<TValue, TError> result)
    {
        if (!result.IsError)
            return;

        var e = result.UnwrapError();
        if (e is Exception ex)
            throw new ResultException(ex.Message, ex);

        if (e is ExceptionError exError)
            throw new ResultException(exError.Message, exError.ToException());

        if (e is Error error)
            throw new ResultException(error.Message);

        if (e is string message)
            throw new ResultException(message);

        throw new ResultException($"Result failed with error {e}.");
    }

    public static void ThrowIfError<TValue, TError>(IResult<TValue, TError> result, string errorMessage)
    {
        if (!result.IsError)
            return;

        var e = result.UnwrapError();
        if (e is Exception ex)
            throw new ResultException($"{errorMessage}: {ex.Message}", ex);

        if (e is ExceptionError exError)
            throw new ResultException($"{errorMessage}: {exError.Message}", exError.ToException());

        if (e is Error error)
            throw new ResultException($"{errorMessage}: {error.Message}");

        if (e is string message)
            throw new ResultException($"{errorMessage}: {message}");

        throw new ResultException($"{errorMessage}: {e}.");
    }
}