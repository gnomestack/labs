namespace GnomeStack.Functional;

public readonly struct ValueResult<TValue> : IResult<TValue, Error>
{
    private readonly ResultState state;

    private readonly TValue value;

    private readonly Error error;

    public ValueResult(TValue value)
    {
        this.state = ResultState.Ok;
        this.value = value;
        this.error = default!;
    }

    public ValueResult(Error error)
    {
        this.state = ResultState.Err;
        this.value = default!;
        this.error = error;
    }

    public bool IsOk => this.state == ResultState.Ok;

    public bool IsError => this.state == ResultState.Err;

    public static implicit operator ValueResult<TValue>(TValue value)
        => new(value);

    public static implicit operator Task<ValueResult<TValue>>(ValueResult<TValue> value)
        => Task.FromResult(value);

    public static implicit operator ValueTask<ValueResult<TValue>>(ValueResult<TValue> value)
        => new(value);

    public static implicit operator ValueResult<TValue>(Result<TValue> result)
        => result.IsOk ? new(result.Unwrap()) : new(result.UnwrapError());

    public static implicit operator ValueResult<TValue>(Result<TValue, Error> result)
        => result.IsOk ? new(result.Unwrap()) : new(result.UnwrapError());

    public static implicit operator ValueResult<TValue>(ValueResult<TValue, Error> result)
        => result.IsOk ? new(result.Unwrap()) : new(result.UnwrapError());

    public static implicit operator ValueResult<TValue>(Error error)
        => new(error);

    public static implicit operator ValueResult<TValue>(Exception exception)
        => new(Functional.Error.Convert(exception));

    public static implicit operator ValueResult<TValue>(Option<TValue> value)
    {
        var valueName = typeof(TValue).FullName;
        var inner = value.Expect(
            $"Implicit conversion from Option<{valueName}> to Result<{valueName}> failed because the option was None.");

        return new(inner);
    }

    public static implicit operator ValueResult<TValue>(Option<Error> error)
    {
        var valueName = typeof(TValue).FullName;
        var errorName = typeof(Error).FullName;
        var inner = error.Expect(
            $"Implicit conversion from Option<{Error}> to Result<{valueName}> failed because the option was None.");

        return new(inner);
    }

    public static bool operator ==(ValueResult<TValue>? left, IResult<TValue, Error>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ValueResult<TValue>? left, IResult<TValue, Error>? right)
    {
        return !Equals(left, right);
    }

    public static bool operator ==(ValueResult<TValue>? left, TValue? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ValueResult<TValue>? left, TValue? right)
    {
        return !Equals(left, right);
    }

    public static ValueResult<TValue> Ok(TValue value)
        => new(value);

    public static ValueResult<TValue> Ok(Func<TValue> generate)
        => new(generate());

    public static Result<TValue> Error(Error error)
        => new(error);

    public static Result<TValue> Exception(Error error)
        => new(error);

    /// <summary>
    /// Boolean AND operation on two results where Ok is true and Err is false. If both are Ok, the
    /// second result is returned. If either is Err, the first error is returned.
    /// </summary>
    /// <param name="other">The second value.</param>
    /// <returns>A result.</returns>
    public ValueResult<TValue> And(IResult<TValue, Error> other)
    {
        if (this.IsError)
            return this;

        if (other.IsError)
            return new(other.UnwrapError());

        return new(other.Unwrap());
    }

    /// <summary>
    /// Boolean AND operation on two results where Ok is true and Err is false. If both are Ok, the
    /// second result is returned. If either is Err, the first error is returned.
    /// </summary>
    /// <param name="other">The second value.</param>
    /// <returns>A result.</returns>
    public ValueResult<TValue> And(ValueResult<TValue> other)
    {
        if (this.IsError)
            return this;

        return other;
    }

    /// <summary>
    /// Boolean AND operation on two results where Ok is true and Err is false. If both are Ok, the
    /// second result is returned. If either is Err, the first error is returned.
    /// </summary>
    /// <param name="other">The second value.</param>
    /// <returns>A result.</returns>
    public ValueResult<TValue> And(TValue other)
    {
        if (this.IsError)
            return this;

        return new(other);
    }

    /// <summary>
    /// Boolean AND operation on two results where Ok is true and Err is false. If both are Ok, the
    /// second result is returned. If either is Err, the first error is returned.
    /// </summary>
    /// <param name="other">The second value is lazily generated by the function.</param>
    /// <returns>A result.</returns>
    public ValueResult<TValue> And(Func<TValue> other)
    {
        if (this.IsError)
            return this;

        return new(other());
    }

    /// <summary>
    /// Boolean AND operation on two results where Ok is true and Err is false. If both are Ok, the
    /// second result is returned. If either is Err, the first error is returned.
    /// </summary>
    /// <typeparam name="TOtherValue">The type of the second value.</typeparam>
    /// <param name="other">The second value is generated by the function.</param>
    /// <returns>A result.</returns>
    public ValueResult<TOtherValue> And<TOtherValue>(Func<ValueResult<TOtherValue>> other)
        where TOtherValue : notnull
    {
        if (this.IsError)
            return new(this.error);

        return other();
    }

    public void Deconstruct(out bool ok, out TValue? value)
    {
        ok = this.IsOk;
        value = this.value;
    }

    public void Deconstruct(out bool ok, out TValue? value, out Error? error)
    {
        ok = this.IsOk;
        value = this.value;
        error = this.error;
    }

    /// <summary>
    /// Expect the result to be Ok. If it is an error, throw a <see cref="ResultException"/> with
    /// the given messsage.
    /// </summary>
    /// <param name="message">The message to use.</param>
    /// <returns>The value if the state is Ok.</returns>
    /// <exception cref="ResultException">Thrown when the state is Error.</exception>
    public TValue Expect(string message)
    {
        ResultException.ThrowIfError(this, message);
        return this.value;
    }

    /// <summary>
    /// Expect the result to be Error. If the state is Ok, throw a <see cref="ResultException"/> with
    /// the given message.
    /// </summary>
    /// <param name="message">The message to use.</param>
    /// <returns>The error if the state is Error.</returns>
    /// <exception cref="ResultException">Thrown when the state is Ok.</exception>
    public Error ExpectError(string message)
    {
        if (this.IsOk)
            throw new ResultException(message);

        return this.error;
    }

    public bool Equals(IResult<TValue, Error>? other)
    {
        if (other is null)
            return false;

        if (this.IsOk != other.IsOk)
            return false;

        if (this.IsOk)
            return EqualityComparer<TValue>.Default.Equals(this.value, other.Unwrap());

        return EqualityComparer<Error>.Default.Equals(this.error, other.UnwrapError());
    }

    public bool Equals(TValue? other)
    {
        if (other is null)
            return this.ToValue().IsNone;

        if (this.IsError)
            return false;

        return EqualityComparer<TValue>.Default.Equals(this.value, other);
    }

    public override bool Equals(object? obj)
    {
        return obj is IResult<TValue, Error> other && this.Equals(other);
    }

#pragma warning disable S2328 // "GetHashCode" should not reference mutable fields
    public override int GetHashCode()
    {
        return HashCode.Combine(this.state, this.value, this.error);
    }

    public IEnumerable<TValue> AsEnumerable()
    {
        if (this.IsOk)
            yield return this.value;
    }

    public IEnumerable<Error> AsErrorEnumerable()
    {
        if (this.IsError)
            yield return this.error;
    }

    public ValueResult<TValue> Inspect(Action<TValue> action)
    {
        if (this.IsError)
            return this;

        action(this.value!);
        return this;
    }

    public Task InspectAsync(Func<TValue, Task> action)
    {
        if (this.IsError)
            return Task.CompletedTask;

        return action(this.value!);
    }

    public async Task InspectAsync(Func<TValue, CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        if (this.IsError)
            return;

        await action(this.value!, cancellationToken)
            .ConfigureAwait(false);
    }

    public ValueResult<TValue> InspectError(Action<Error> action)
    {
        if (this.IsOk)
            return this;

        action(this.error);
        return this;
    }

    public Task InspectErrorAsync(Func<Error, Task> action)
    {
        if (this.IsOk)
            return Task.CompletedTask;

        return action(this.error);
    }

    public bool Match(Func<TValue, bool> predicate)
    {
        if (this.IsError)
            return false;

        return predicate(this.value!);
    }

    public bool MatchError(Func<Error, bool> predicate)
    {
        if (this.IsOk)
            return false;

        return predicate(this.error!);
    }

    public ValueResult<TOther, Error> Map<TOther>(TOther other)
        where TOther : notnull
    {
        if (this.IsError)
            return new(this.error);

        return new(other);
    }

    public ValueResult<TOther> Map<TOther>(Func<TValue, TOther> map)
    {
        if (this.IsError)
            return new(this.error);

        return new(map(this.value!));
    }

    public ValueResult<TOther> Map<TOther>(Func<TValue, TOther> map, TOther defaultValue)
    {
        if (this.IsError)
            return new(defaultValue);

        return new(map(this.value!));
    }

    public ValueResult<TOther> Map<TOther>(Func<TValue, TOther> map, Func<TOther> generate)
    {
        if (this.IsError)
            return new(generate());

        return new(map(this.value!));
    }

    public async Task<ValueResult<TOther>> MapAsync<TOther>(Task<TOther> task)
    {
        if (this.IsError)
            return new(this.error);

        var other = await task
            .ConfigureAwait(false);
        return new(other);
    }

    public async Task<ValueResult<TOther>> MapAsync<TOther>(Func<TValue, Task<TOther>> map)
    {
        if (this.IsError)
            return new(this.error);

        var other = await map(this.value!)
            .ConfigureAwait(false);
        return new(other);
    }

    public async Task<ValueResult<TOther>> MapAsync<TOther>(
        Func<TValue, CancellationToken, Task<TOther>> map,
        CancellationToken cancellationToken = default)
        where TOther : notnull
    {
        if (this.IsError)
            return new(this.error);

        var other = await map(this.value!, cancellationToken)
            .ConfigureAwait(false);
        return new(other);
    }

    public ValueResult<TValue, TOtherError> MapError<TOtherError>(Func<Error, TOtherError> map)
        where TOtherError : notnull
    {
        if (this.IsError)
            return new(map(this.error));

        return new(this.value!);
    }

    public async Task<ValueResult<TValue, TOtherError>> MapErrorAsync<TOtherError>(Func<Error, Task<TOtherError>> map)
        where TOtherError : notnull
    {
        if (!this.IsOk)
            return new(this.value!);

        var other = await map(this.error).ConfigureAwait(false);
        return new(other);
    }

    public ValueResult<TOtherValue> MapOrDefault<TOtherValue>(
        Func<TValue, TOtherValue> map,
        TOtherValue defaultValue)
        where TOtherValue : notnull
    {
        if (this.IsError)
            return new(defaultValue);

        return new(map(this.value!));
    }

    public ValueResult<TOtherValue> MapOrDefault<TOtherValue>(
        Func<TValue, TOtherValue> map,
        Func<TOtherValue> generate)
        where TOtherValue : notnull
    {
        if (this.IsError)
            return new ValueResult<TOtherValue>(generate());

        return new(map(this.value!));
    }

    public async Task<ValueResult<TOtherValue>> MapOrDefaultAsync<TOtherValue>(
        Func<TValue, Task<TOtherValue>> map,
        TOtherValue defaultValue)
        where TOtherValue : notnull
    {
        if (this.IsError)
            return new ValueResult<TOtherValue>(defaultValue);

        var other = await map(this.value!)
            .ConfigureAwait(false);

        return new(other);
    }

    public async Task<ValueResult<TOtherValue>> MapOrDefaultAsync<TOtherValue>(
        Func<TValue, Task<TOtherValue>> map,
        Func<TOtherValue> generate)
        where TOtherValue : notnull
    {
        if (this.IsError)
            return new(generate());

        var other = await map(this.value!)
            .ConfigureAwait(false);

        return new(other);
    }

    public async Task<ValueResult<TOtherValue>> MapOrDefaultAsync<TOtherValue>(
        Func<TValue, Task<TOtherValue>> map,
        Func<Task<TOtherValue>> generate)
        where TOtherValue : notnull
    {
        if (this.IsError)
        {
            var defaulted = await generate()
                .ConfigureAwait(false);

            return new ValueResult<TOtherValue>(defaulted);
        }

        var other = await map(this.value!)
            .ConfigureAwait(false);

        return new ValueResult<TOtherValue>(other);
    }

    public async Task<ValueResult<TOtherValue>> MapOrDefaultAsync<TOtherValue>(
        Func<TValue, CancellationToken, Task<TOtherValue>> map,
        Func<CancellationToken, Task<TOtherValue>> generate,
        CancellationToken cancellationToken = default)
        where TOtherValue : notnull
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (this.IsError)
        {
            var defaulted = await generate(cancellationToken)
                .ConfigureAwait(false);

            return new ValueResult<TOtherValue>(defaulted);
        }

        var other = await map(this.value!, cancellationToken)
            .ConfigureAwait(false);

        return new ValueResult<TOtherValue>(other);
    }

    public ValueResult<TValue> Or(ValueResult<TValue> other)
    {
        if (this.IsOk)
            return this;

        if (other.IsOk)
            return other;

        return this;
    }

    public ValueResult<TValue> Or(Func<TValue> other)
    {
        if (this.IsOk)
            return this;

        return new ValueResult<TValue>(other());
    }

    public ValueResult<TValue> Or(Func<ValueResult<TValue>> other)
    {
        if (this.IsOk)
            return this;

        return other();
    }

    /// <summary>
    /// Converts the error to an <see cref="Option{TError}"/>.
    /// </summary>
    /// <returns>Returns the optional error value.</returns>
    public Option<Error> ToError()
        => Option<Error>.From(this.error);

    /// <summary>
    /// Converts the value to an <see cref="Option{TValue}"/>.
    /// </summary>
    /// <returns>Returns the optional value.</returns>
    public Option<TValue> ToValue()
        => Option<TValue>.From(this.value);

    public TValue Unwrap()
    {
        ResultException.ThrowIfError(this);
        return this.value!;
    }

    public TValue Unwrap(TValue defaultValue)
    {
        if (this.IsError)
            return defaultValue;

        return this.value!;
    }

    public TValue Unwrap(Func<TValue> defaultValue)
    {
        if (this.IsError)
            return defaultValue();

        return this.value!;
    }

    public Error UnwrapError()
    {
        if (this.IsOk)
            throw new ResultException($"UnwrapError is invalid when result has value: {this.value}.");

        return this.error;
    }

    public Error UnwrapError(Error defaultError)
    {
        if (this.IsOk)
            return defaultError;

        return this.error;
    }

    public Error UnwrapError(Func<Error> defaultError)
    {
        if (this.IsOk)
            return defaultError();

        return this.error;
    }

    public ValueResult<TValue> ThrowOnError()
    {
        ResultException.ThrowIfError(this);

        return this;
    }
}