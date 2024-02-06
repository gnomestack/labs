// ReSharper disable ParameterHidesMember
namespace GnomeStack.Functional;


#pragma warning disable S4035 // Classes implementing "IEquatable<T>" should be sealed
public class Result<TValue, TError> : IResult<TValue, TError>
{
    private TValue value;

    private TError error;

    private ResultState state;

    public Result(TValue value)
        : this(ResultState.Ok, value, default!)
    {
    }

    public Result(TError error)
        : this(ResultState.Err, default!, error)
    {
    }

    internal Result(ResultState state, TValue value, TError error)
    {
        this.state = state;
        this.value = value;
        this.error = error;
    }

    public bool IsOk => this.state == ResultState.Ok;

    public bool IsError => this.state == ResultState.Err;

    public static implicit operator Result<TValue, TError>(TValue value)
        => Ok(value);

    public static implicit operator Result<TValue, TError>(TError error)
        => Error(error);

    public static implicit operator Result<TValue, TError>(ValueResult<TValue, TError> value)
        => value.IsOk ? new(value.Unwrap()) : new(value.UnwrapError());

    public static implicit operator Task<Result<TValue, TError>>(Result<TValue, TError> value)
        => Task.FromResult(value);

    public static implicit operator ValueTask<Result<TValue, TError>>(Result<TValue, TError> value)
        => new(value);

    public static implicit operator Result<TValue, TError>(Option<TValue> value)
    {
        var valueName = typeof(TValue).FullName;
        var errorName = typeof(TError).FullName;
        var inner = value.Expect(
            $"Implicit conversion from Option<{valueName}> to Result<{valueName}," +
            $" {errorName}> failed because the option was None.");

        return Ok(inner);
    }

    public static implicit operator Result<TValue, TError>(Option<TError> error)
    {
        var valueName = typeof(TValue).FullName;
        var errorName = typeof(TError).FullName;
        var inner = error.Expect(
            $"Implicit conversion from Option<{errorName}> to Result<{valueName}," +
            $" {errorName}> failed because the option was None.");

        return Error(inner);
    }

    public static bool operator ==(Result<TValue, TError>? left, IResult<TValue, TError>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Result<TValue, TError>? left, IResult<TValue, TError>? right)
    {
        return !Equals(left, right);
    }

    public static bool operator ==(Result<TValue, TError>? left, TValue? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Result<TValue, TError>? left, TValue? right)
    {
        return !Equals(left, right);
    }

    public static Result<TValue, TError> Ok(TValue value)
        => new(ResultState.Ok, value, default!);

    public static Result<TValue, TError> Ok(Func<TValue> generate)
        => new(ResultState.Ok, generate(), default!);

    public static Result<TValue, TError> Error(TError error)
        => new(ResultState.Err, default!, error);

    /// <summary>
    /// Boolean AND operation on two results where Ok is true and Err is false. If both are Ok, the
    /// second result is returned. If either is Err, the first error is returned.
    /// </summary>
    /// <param name="other">The second value.</param>
    /// <returns>A result.</returns>
    public Result<TValue, TError> And(IResult<TValue, TError> other)
    {
        if (this.IsError)
            return this.error;

        if (other.IsError)
            return new Result<TValue, TError>(other.UnwrapError());

        return new(other.Unwrap());
    }

    /// <summary>
    /// Boolean AND operation on two results where Ok is true and Err is false. If both are Ok, the
    /// second result is returned. If either is Err, the first error is returned.
    /// </summary>
    /// <param name="other">The second value.</param>
    /// <returns>A result.</returns>
    public Result<TValue, TError> And(Result<TValue, TError> other)
    {
        if (this.IsError)
            return this.error;

        return other;
    }

    /// <summary>
    /// Boolean AND operation on two results where Ok is true and Err is false. If both are Ok, the
    /// second result is returned. If either is Err, the first error is returned.
    /// </summary>
    /// <param name="other">The second value.</param>
    /// <returns>A result.</returns>
    public Result<TValue, TError> And(TValue other)
    {
        if (this.IsError)
            return this.error;

        return new(other);
    }

    /// <summary>
    /// Boolean AND operation on two results where Ok is true and Err is false. If both are Ok, the
    /// second result is returned. If either is Err, the first error is returned.
    /// </summary>
    /// <param name="other">The second value is lazily generated by the function.</param>
    /// <returns>A result.</returns>
    public Result<TValue, TError> And(Func<TValue> other)
    {
        if (this.IsError)
            return this.error;

        return new(other());
    }

    /// <summary>
    /// Boolean AND operation on two results where Ok is true and Err is false. If both are Ok, the
    /// second result is returned. If either is Err, the first error is returned.
    /// </summary>
    /// <typeparam name="TOtherValue">The type of the second value.</typeparam>
    /// <param name="other">The second value is generated by the function.</param>
    /// <returns>A result.</returns>
    public Result<TOtherValue, TError> And<TOtherValue>(Func<Result<TOtherValue, TError>> other)
        where TOtherValue : notnull
    {
        if (this.IsError)
            return this.error;

        return other();
    }

    public void Deconstruct(out bool ok, out TValue value)
    {
        ok = this.IsOk;
        value = this.value;
    }

    public void Deconstruct(out bool ok, out TValue value, out TError error)
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
    public TError ExpectError(string message)
    {
        if (this.IsOk)
            throw new ResultException(message);

        return this.error;
    }

    public bool Equals(IResult<TValue, TError>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (this.IsOk != other.IsOk)
            return false;

        if (this.IsOk)
            return EqualityComparer<TValue>.Default.Equals(this.value, other.Unwrap());

        return EqualityComparer<TError>.Default.Equals(this.error, other.UnwrapError());
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
        return obj is IResult<TValue, TError> other && this.Equals(other);
    }

#pragma warning disable S2328 // "GetHashCode" should not reference mutable fields
    public override int GetHashCode()
    {
        return HashCode.Combine(this.state, this.value, this.error);
    }

    public bool IsErrorAnd(Func<TError, bool> predicate)
        => this.IsError && predicate(this.error);

    public bool IsOkAnd(Func<TValue, bool> predicate)
        => this.IsOk && predicate(this.value!);

    public IEnumerable<TValue> AsEnumerable()
    {
        if (this.IsOk)
            yield return this.value;
    }

    public IEnumerable<TError> AsErrorEnumerable()
    {
        if (this.IsError)
            yield return this.error;
    }

    public Result<TValue, TError> Inspect(Action<TValue> action)
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

    public Result<TValue, TError> InspectError(Action<TError> action)
    {
        if (this.IsOk)
            return this;

        action(this.error);
        return this;
    }

    public Task InspectErrorAsync(Func<TError, Task> action)
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

    public bool MatchError(Func<TError, bool> predicate)
    {
        if (this.IsOk)
            return false;

        return predicate(this.error!);
    }

    public Result<TOther, TError> Map<TOther>(TOther other)
        where TOther : notnull
    {
        if (this.IsError)
            return new(this.error);

        return new(other);
    }

    public Result<TOther, TError> Map<TOther>(Func<TValue, TOther> map)
    {
        if (this.IsError)
            return this.error;

        return map(this.value!);
    }

    public Result<TOther, TError> Map<TOther>(Func<TValue, TOther> map, TOther defaultValue)
    {
        if (this.IsError)
            return defaultValue;

        return map(this.value!);
    }

    public Result<TOther, TError> Map<TOther>(Func<TValue, TOther> map, Func<TOther> generate)
    {
        if (this.IsError)
            return generate();

        return map(this.value!);
    }

    public async Task<Result<TOther, TError>> MapAsync<TOther>(Task<TOther> task)
    {
        if (this.IsError)
            return new(this.error);

        var other = await task
            .ConfigureAwait(false);
        return new(other);
    }

    public async Task<Result<TOther, TError>> MapAsync<TOther>(Func<TValue, Task<TOther>> map)
    {
        if (this.IsError)
            return new(this.error);

        var other = await map(this.value!)
            .ConfigureAwait(false);
        return new(other);
    }

    public async Task<Result<TOther, TError>> MapAsync<TOther>(
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

    public Result<TValue, TOtherError> MapError<TOtherError>(Func<TError, TOtherError> map)
        where TOtherError : notnull
    {
        if (this.IsError)
            return map(this.error);

        return this.value!;
    }

    public async Task<Result<TValue, TOtherError>> MapErrorAsync<TOtherError>(Func<TError, Task<TOtherError>> map)
        where TOtherError : notnull
    {
        if (!this.IsOk)
            return new(this.value!);

        var other = await map(this.error).ConfigureAwait(false);
        return new(other);
    }

    public Result<TOtherValue, TError> MapOrDefault<TOtherValue>(
        Func<TValue, TOtherValue> map,
        TOtherValue defaultValue)
        where TOtherValue : notnull
    {
        if (this.IsError)
            return new(defaultValue);

        return map(this.value!);
    }

    public Result<TOtherValue, TError> MapOrDefault<TOtherValue>(
        Func<TValue, TOtherValue> map,
        Func<TOtherValue> generate)
        where TOtherValue : notnull
    {
        if (this.IsError)
            return new(generate());

        return map(this.value!);
    }

    public async Task<Result<TOtherValue, TError>> MapOrDefaultAsync<TOtherValue>(
        Func<TValue, Task<TOtherValue>> map,
        TOtherValue defaultValue)
        where TOtherValue : notnull
    {
        if (this.IsError)
            return new(defaultValue);

        var other = await map(this.value!)
            .ConfigureAwait(false);

        return new(other);
    }

    public async Task<Result<TOtherValue, TError>> MapOrDefaultAsync<TOtherValue>(
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

    public async Task<Result<TOtherValue, TError>> MapOrDefaultAsync<TOtherValue>(
        Func<TValue, Task<TOtherValue>> map,
        Func<Task<TOtherValue>> generate)
        where TOtherValue : notnull
    {
        if (this.IsError)
        {
            var defaulted = await generate()
                .ConfigureAwait(false);

            return new(defaulted);
        }

        var other = await map(this.value!)
            .ConfigureAwait(false);

        return new(other);
    }

    public async Task<Result<TOtherValue, TError>> MapOrDefaultAsync<TOtherValue>(
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

            return new(defaulted);
        }

        var other = await map(this.value!, cancellationToken)
            .ConfigureAwait(false);

        return new(other);
    }

    public Result<TValue, TError> Or(Result<TValue, TError> other)
    {
        if (this.IsOk)
            return this;

        if (other.IsOk)
            return other;

        return this;
    }

    public Result<TValue, TError> Or(Func<TValue> other)
    {
        if (this.IsOk)
            return this;

        return new(other());
    }

    public Result<TValue, TError> Or(Func<Result<TValue, TError>> other)
    {
        if (this.IsOk)
            return this;

        return other();
    }

    /// <summary>
    /// Converts the error to an <see cref="Option{TError}"/>.
    /// </summary>
    /// <returns>Returns the optional error value.</returns>
    public Option<TError> ToError()
        => this.IsError ? this.error : Option<TError>.None();

    /// <summary>
    /// Converts the value to an <see cref="Option{TValue}"/>.
    /// </summary>
    /// <returns>Returns the optional value.</returns>
    public Option<TValue> ToValue()
        => this.IsOk ? this.value : Option<TValue>.None();

    /// <summary>
    /// Updates the value of the result if it is Ok. Otherwise, the error is returned.
    /// </summary>
    /// <param name="value">The value that replaces the underlying value.</param>
    /// <returns>The result.</returns>
    public Result<TValue, TError> Replace(TValue value)
    {
        this.value = value;
        this.error = default!;
        this.state = ResultState.Ok;
        return this;
    }

    /// <summary>
    /// Updates the value of the result if it is Ok. Otherwise, the error is returned.
    /// </summary>
    /// <param name="generate">The function that lazily generates the new underlying value.</param>
    /// <returns>The result.</returns>
    public Result<TValue, TError> Replace(Func<TValue> generate)
    {
        this.value = generate();
        this.error = default!;
        this.state = ResultState.Ok;
        return this;
    }

    /// <summary>
    /// Updates the value of the result if it is Ok. Otherwise, the error is returned.
    /// </summary>
    /// <param name="update">The function that lazily updates the underlying value.</param>
    /// <returns>The result.</returns>
    public Result<TValue, TError> Replace(Func<TValue, TValue> update)
    {
        this.ThrowOnError();
        this.value = update(this.value);
        return this;
    }

    public Result<TValue, TError> ReplaceError(TError error)
    {
        this.error = error;
        this.state = ResultState.Err;
        return this;
    }

    public Result<TValue, TError> ReplaceError(Func<TError> generate)
    {
        this.error = generate();
        this.state = ResultState.Err;
        return this;
    }

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

    public TError UnwrapError()
    {
        if (this.IsOk)
            throw new ResultException($"UnwrapError is invalid when result has value: {this.value}.");

        return this.error;
    }

    public TError UnwrapError(TError defaultError)
    {
        if (this.IsOk)
            return defaultError;

        return this.error;
    }

    public TError UnwrapError(Func<TError> defaultError)
    {
        if (this.IsOk)
            return defaultError();

        return this.error;
    }

    public Result<TValue, TError> ThrowOnError()
    {
        ResultException.ThrowIfError(this);

        return this;
    }
}