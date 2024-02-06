using GnomeStack.Functional;

namespace GnomeStack.Extras.Functional;

public static class ExceptionExtensions
{
    public static Error ToError(this Exception exception)
    {
        return Error.Convert(exception);
    }

    public static Result<TValue> ToResult<TValue>(this Exception exception)
        where TValue : notnull
    {
        return Result<TValue>.Error(Error.Convert(exception));
    }
}