namespace GnomeStack.Functional;

public readonly struct ValueResult : IResult<Nil, Error>
{
    private readonly ResultState state;

    private readonly Error error;

    public ValueResult()
    {
        this.state = ResultState.Ok;
        this.error = default!;
    }

    public ValueResult(Error error)
    {
        this.state = ResultState.Err;
        this.error = error;
    }

    public ValueResult(Exception exception)
        : this(Functional.Error.Convert(exception))
    {
    }

    public static ValueResult Default { get; } = new();

    public bool IsOk => this.state == ResultState.Ok;

    public bool IsError => this.state == ResultState.Err;

    public static implicit operator ValueResult(Nil value)
        => new();

    public static implicit operator Error?(ValueResult result)
        => result.error;

    public static implicit operator ValueResult(ValueTuple value)
        => new();

    public static implicit operator ValueResult(Error error)
        => new(error);

    public static implicit operator ValueResult(Exception exception)
        => new(exception);

    public static implicit operator ValueResult(ValueResult<Nil, Error> result)
        => result.IsOk ? new() : new(result.UnwrapError());

    public static implicit operator ValueResult(ValueResult<ValueTuple, Error> result)
        => result.IsOk ? new() : new(result.UnwrapError());

    public static implicit operator ValueResult(ValueResult<Nil> result)
        => result.IsOk ? new() : new(result.UnwrapError());

    public static implicit operator ValueResult(ValueResult<ValueTuple> result)
        => result.IsOk ? new() : new(result.UnwrapError());

    public static implicit operator ValueResult<Nil, Error>(ValueResult result)
        => result.IsOk ? new(default(Nil)) : new(result.UnwrapError());

    public static implicit operator ValueResult<ValueTuple, Error>(ValueResult result)
        => result.IsOk ? new(default(ValueTuple)) : new(result.UnwrapError());

    public static implicit operator ValueResult<Nil>(ValueResult result)
        => result.IsOk ? new(default(Nil)) : new(result.UnwrapError());

    public static implicit operator ValueResult<ValueTuple>(ValueResult result)
        => result.IsOk ? new(default(ValueTuple)) : new(result.UnwrapError());

    public static ValueResult<TValue, TError> Ok<TValue, TError>(TValue value)
        where TError : notnull
    {
        return new(value);
    }

    public static ValueResult<TValue> Ok<TValue>(TValue value)
    {
        return new(value);
    }

    public static ValueResult<TValue> Error<TValue>(Error error)
    {
        return new(error);
    }

    public static ValueResult<TValue> Error<TValue>(Exception exception)
    {
        return new(Functional.Error.Convert(exception));
    }

    public static ValueResult<TValue, TError> Error<TError, TValue>(TError error)
        where TError : notnull
    {
        Error<string>(new Exception());
        return new(error);
    }

    public static ValueResult Try(Action action)
    {
        try
        {
            action();
            return Default;
        }
        catch (Exception ex)
        {
            return new ValueResult(ex);
        }
    }

    public static ValueResult<TValue> Try<TValue>(Func<TValue> func)
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

    public static async Task<ValueResult> TryAsync(Func<Task> action)
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

    public static async Task<ValueResult> TryAsync(
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

    public static async Task<ValueResult<TValue>> TryAsync<TValue>(
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

    public static async Task<ValueResult<TValue>> TryAsync<TValue>(
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

    public static async Task<ValueResult<TValue, TError>> TryAsync<TValue, TError>(
        Func<Task<TValue>> action,
        Func<Exception, TError> error)
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
            return new(error(ex));
        }
    }

    public static async Task<ValueResult<TValue, TError>> TryAsync<TValue, TError>(
        Func<CancellationToken, Task<TValue>> action,
        Func<Exception, TError> error,
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
            return new(error(ex));
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

    public bool Equals(ValueResult other)
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

        if (obj is ValueResult r1)
            return this.Equals(r1);

        if (obj is IResult<Nil, Error> r2)
            return this.Equals(r2);

        if (obj is Nil)
            return this.IsOk;

        if (obj is Error error)
            return this.IsError && this.error.Equals(error);

        return false;
    }

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