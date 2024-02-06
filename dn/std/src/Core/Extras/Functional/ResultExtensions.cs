using GnomeStack.Functional;

namespace GnomeStack.Extras.Functional;

public static class ResultExtensions
{
    public static Result<TValue> ToResult<TValue>(this TValue value)
        where TValue : notnull
    {
        return Result<TValue>.Ok(value);
    }

    public static Result<TValue> ToResult<TValue>(this TValue? value)
        where TValue : struct
    {
        return value.HasValue ?
            Result<TValue>.Ok(value.Value) :
            Result<TValue>.Error(new NullReferenceException());
    }

    public static Result<TValue, TError> ToResult<TValue, TError>(this TValue value)
        where TValue : notnull
        where TError : notnull
    {
        return Result<TValue, TError>.Ok(value);
    }

    public static Result<TValue, TError> ToResult<TValue, TError>(this TError error)
        where TValue : notnull
        where TError : notnull
    {
        return Result<TValue, TError>.Error(error);
    }

    public static Result<TValue> ToResult<TValue>(Error error)
        where TValue : notnull
    {
        return Result<TValue>.Error(error);
    }
}