using System.Diagnostics.CodeAnalysis;

namespace GnomeStack.Functional;

[SuppressMessage("ReSharper", "LocalVariableHidesMember")]
public class Result : IResult<Nil, Error>
{
    private readonly ResultState state;

    private readonly Error error;

    public Result()
    {
        this.state = ResultState.Ok;
    }

    public Result(Error error)
    {
        this.state = ResultState.Err;
        this.error = error;
    }

    public Result(Exception exception)
        : this(Functional.Error.Convert(exception))
    {
    }

    public static Result Default { get; } = new Result();

    public bool IsOk => this.state == ResultState.Ok;

    public bool IsError => this.state == ResultState.Err;

    public static implicit operator Result(Nil value)
        => new();

    public static implicit operator Error?(Result result)
        => result.error;

    public static implicit operator Result(ValueTuple value)
        => new();

    public static implicit operator Result(Error error)
        => new(error);

    public static implicit operator Result(Exception exception)
        => new(exception);

    public static implicit operator Result(ValueResult result)
        => result.IsOk ? new() : new(result.UnwrapError());

    public static implicit operator Result(ValueResult<Nil, Error> result)
        => result.IsOk ? new() : new(result.UnwrapError());

    public static implicit operator Result(ValueResult<ValueTuple, Error> result)
        => result.IsOk ? new() : new(result.UnwrapError());

    public static implicit operator Result(ValueResult<Nil> result)
        => result.IsOk ? new() : new(result.UnwrapError());

    public static implicit operator Result(ValueResult<ValueTuple> result)
        => result.IsOk ? new() : new(result.UnwrapError());

    public static implicit operator Result(Result<Nil, Error> result)
        => result.IsOk ? new() : new(result.UnwrapError());

    public static implicit operator Result(Result<ValueTuple, Error> result)
        => result.IsOk ? new() : new(result.UnwrapError());

    public static implicit operator Result(Result<Nil> result)
        => result.IsOk ? new() : new(result.UnwrapError());

    public static implicit operator Result(Result<ValueTuple> result)
        => result.IsOk ? new() : new(result.UnwrapError());

    public static Result<TValue, TError> Ok<TValue, TError>(TValue value)
        where TError : notnull
    {
        return new Result<TValue, TError>(value);
    }

    public static Result<TValue> Ok<TValue>(TValue value)
    {
        return new Result<TValue>(value);
    }

    public static ValueResult OkValue()
        => ValueResult.Default;

    public static ValueResult<TValue> OkValue<TValue>(TValue value)
        => new(value);

    public static Result Error(Error error)
        => new(error);

    public static Result Error(Exception exception)
        => new(exception);

    public static Result<TValue> Error<TValue>(Error error)
    {
        return new Result<TValue>(error);
    }

    public static Result<TValue> Error<TValue>(Exception exception)
    {
        return new Result<TValue>(Functional.Error.Convert(exception));
    }

    public static Result<TValue, TError> Error<TError, TValue>(TError error)
        where TError : notnull
    {
        Error<string>(new Exception());
        return new Result<TValue, TError>(error);
    }

    public static ValueResult ErrorValue(Error error)
        => new(error);

    public static ValueResult ErrorValue(Exception exception)
        => new(Functional.Error.Convert(exception));

    public static ValueResult<TValue> ErrorValue<TValue>(Error error)
        => new(error);

    public static ValueResult<TValue> ErrorValue<TValue>(Exception exception)
        => new(Functional.Error.Convert(exception));

    public static ValueResult<TValue, TError> ErrorValue<TValue, TError>(TError error)
        => new(error);


    public static Result Try(Action action)
    {
        try
        {
            action();
            return Default;
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    public static Result<TValue> Try<TValue>(Func<TValue> func)
    {
        try
        {
            return new(func());
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    public static Result<TValue, TError> Try<TValue, TError>(Func<TValue> action, Func<Exception, TError> error)
    {
        try
        {
            return new(action());
        }
        catch (Exception ex)
        {
            return new(error(ex));
        }
    }

    public static ValueResult TryValue(Action action)
    {
        try
        {
            action();
            return ValueResult.Default;
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    public static ValueResult<TValue> TryValue<TValue>(Func<TValue> func)
    {
        try
        {
            return new(func());
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    public static ValueResult<TValue, TError> TryValue<TValue, TError>(Func<TValue> action, Func<Exception, TError> error)
    {
        try
        {
            return new(action());
        }
        catch (Exception ex)
        {
            return new(error(ex));
        }
    }

    public static async Task<Result> TryAsync(Func<Task> action)
    {
        try
        {
            await action();
            return Default;
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    public static async Task<Result> TryAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await action(cancellationToken)
                .ConfigureAwait(false);

            return Default;
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    public static async Task<Result<TValue>> TryAsync<TValue>(
        Func<Task<TValue>> action)
    {
        try
        {
            var value = await action()
                .ConfigureAwait(false);

            return new(value);
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    public static async Task<Result<TValue>> TryAsync<TValue>(
        Func<CancellationToken, Task<TValue>> action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var value = await action(cancellationToken)
                .ConfigureAwait(false);

            return new(value);
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    public static async Task<Result<TValue, TError>> TryAsync<TValue, TError>(
        Func<Task<TValue>> action,
        Func<Exception, TError> map)
        where TError : notnull
    {
        try
        {
            var value = await action()
                .ConfigureAwait(false);

            return new(value);
        }
        catch (Exception ex)
        {
            return new(map(ex));
        }
    }

    public static async Task<Result<TValue, TError>> TryAsync<TValue, TError>(
        Func<CancellationToken, Task<TValue>> action,
        Func<Exception, TError> map,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var value = await action(cancellationToken)
                .ConfigureAwait(false);

            return new(value);
        }
        catch (Exception ex)
        {
            return new(map(ex));
        }
    }

    public static async Task<ValueResult> TryValueAsync(Func<Task> action)
    {
        try
        {
            await action();
            return ValueResult.Default;
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    public static async Task<ValueResult> TryValueAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await action(cancellationToken)
                .ConfigureAwait(false);

            return ValueResult.Default;
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    public static async Task<ValueResult<TValue>> TryValueAsync<TValue>(
        Func<Task<TValue>> action)
    {
        try
        {
            var value = await action()
                .ConfigureAwait(false);

            return new(value);
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    public static async Task<ValueResult<TValue>> TryValueAsync<TValue>(
        Func<CancellationToken, Task<TValue>> action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var value = await action(cancellationToken)
                .ConfigureAwait(false);

            return new(value);
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    public static async Task<ValueResult<TValue, TError>> TryValueAsync<TValue, TError>(
        Func<Task<TValue>> action,
        Func<Exception, TError> map)
        where TError : notnull
    {
        try
        {
            var value = await action()
                .ConfigureAwait(false);

            return new(value);
        }
        catch (Exception ex)
        {
            return new(map(ex));
        }
    }

    public static async Task<ValueResult<TValue, TError>> TryValueAsync<TValue, TError>(
        Func<CancellationToken, Task<TValue>> action,
        Func<Exception, TError> map,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var value = await action(cancellationToken)
                .ConfigureAwait(false);

            return new(value);
        }
        catch (Exception ex)
        {
            return new(map(ex));
        }
    }

    public void Deconstruct(out bool ok, out Nil value)
    {
        ok = this.state == ResultState.Ok;
        value = Nil.Value;
    }

    public void Deconstruct(out bool ok, out Nil value, out Error? error)
    {
        ok = this.state == ResultState.Ok;
        value = Nil.Value;
        error = this.error;
    }

    public bool Equals(IResult<Nil, Error> other)
    {
        if (this.IsOk)
            return other.IsOk;

        return this.error.Equals(other.UnwrapError());
    }

    public bool Equals(Result other)
    {
        if (this.IsOk)
            return other.IsOk;

        return this.error.Equals(other.error);
    }

    public bool Equals(Nil other)
    {
        return this.IsOk;
    }

    public bool Equals(Error other)
    {
        if (this.IsOk)
            return false;

        return this.error.Equals(other);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (obj is Result r1)
            return this.Equals(r1);

        if (obj is IResult<Nil, Error> r2)
            return this.Equals(r2);

        if (obj is Nil)
            return this.IsOk;

        if (obj is Error error)
            return this.IsError && this.error.Equals(error);

        return false;
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    [SuppressMessage("Minor Bug", "S2328:\"GetHashCode\" should not reference mutable fields")]
    public override int GetHashCode()
    {
        return this.IsOk ? 0 : this.error.GetHashCode();
    }

    public Nil Unwrap()
    {
        if (this.state == ResultState.Err)
            throw new ResultException(this.error.Message);

        return Nil.Value;
    }

    public Error UnwrapError()
    {
        if (this.state == ResultState.Ok)
            throw new ResultException("Result is Ok.");

        return this.error;
    }
}