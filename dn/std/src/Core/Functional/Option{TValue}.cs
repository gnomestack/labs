using GnomeStack.Standard;

// ReSharper disable ParameterHidesMember
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace GnomeStack.Functional;

/// <summary>
///   Represents an optional value that may or may not be present and provides
///   methods to deal with non present values to avoid throwing exceptions.
/// </summary>
/// <remarks>
///     <para>
///         This type is similar to <see cref="Nullable{TValue}" /> but
///         is not restricted to value types and is a monad that can be
///         chained methods to handle the presence or absence of a value.
///     </para>
///     <para>
///         The implementation is influenced by Rust rather than F#. It is
///         purposefully lightweight and does not provide a lot of the
///         functionality that F# that other implementation provides to allow
///         users to create their own extensions.
///     </para>
/// </remarks>
/// <typeparam name="TValue">The type of the value.</typeparam>
#pragma warning disable S4035 // Classes implementing "IEquatable<T>" should be sealed
public class Option<TValue> :
    IEquatable<Option<TValue>>,
    IOptional<TValue>
{
    private static readonly Option<TValue> s_none = new(OptionState.None, default!);

    private TValue value;

    private OptionState state;

    internal Option(OptionState state, TValue value)
    {
        this.state = state;
        this.value = value;
    }

    /// <summary>
    ///  Gets a value indicating whether this instance has a value.
    /// </summary>
    public bool IsSome => this.state == OptionState.Some;

    /// <summary>
    /// Gets a value indicating whether this instance has no value.
    /// </summary>
    public bool IsNone => this.state == OptionState.None;

    /// <summary>
    /// Implicitly converts <typeparamref name="TValue"/> to <see cref="Option{TValue}"/>.
    /// </summary>
    /// <param name="value">The type of the value.</param>
    public static implicit operator Option<TValue>(TValue value)
        => new(OptionState.Some, value);

#pragma warning disable SA1313
    /// <summary>
    /// Implicitly converts <see cref="ValueTask"/> to <see cref="Option{TValue}"/>.
    /// </summary>
    /// <param name="_">The discarded value.</param>
    public static implicit operator Option<TValue>(ValueTask _)
        => Option.None<TValue>();

    /// <summary>
    /// Implicitly converts <see cref="Nil"/> to <see cref="Option{TValue}"/>.
    /// </summary>
    /// <param name="_">The discarded value.</param>
    public static implicit operator Option<TValue>(Nil _)
        => new(OptionState.None, default!);

    public static implicit operator Option<TValue>(DBNull _)
        => new(OptionState.None, default!);

    public static bool operator ==(Option<TValue> left, Option<TValue> right)
        => left.Equals(right);

    public static bool operator !=(Option<TValue> left, Option<TValue> right)
        => !left.Equals(right);

    public static bool operator ==(Option<TValue> left, TValue right)
        => left.Equals(right);

    public static bool operator !=(Option<TValue> left, TValue right)
        => !left.Equals(right);

    public static Option<TValue> None()
        => s_none;

    public static Option<TValue> Some(TValue value)
        => new(OptionState.Some, value);

    public static Option<TValue> From(TValue? value)
        => Option.IsNone(value) ? None() : Some(value);

    public static bool IsValueNone(object? value)
    {
        return value switch
        {
            IOptional optional => optional.IsNone,
            _ => Nil.IsNil(value),
        };
    }

    /// <summary>
    /// Deconstructs the <see cref="Option{TValue}"/> into
    /// a <typeparamref name="TValue"/> and a <see cref="bool"/> that
    /// is <see langword="true"/> when there is a value and
    /// <see landword="false" /> when the value is none.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="some">A value indicating whether the value is some or none.</param>
    public void Deconstruct(out TValue value, out bool some)
    {
        value = this.value!;
        some = this.IsSome;
    }

    /// <summary>
    /// Determines if the object is equal to the value.
    /// </summary>
    /// <param name="other">The other value.</param>
    /// <remarks>
    ///     <para>
    ///       If the option is none and the other value is <c>null</c>,
    ///       <c>Nil</c>, <c>DBNull</c>, or <c>ValueTuple</c>, then the result is
    ///       is true. Otherwise, the result is true if the inner
    ///       value is equal to the other value, otherwise, false.
    ///     </para>
    /// </remarks>
    /// <returns>
    /// <see langword="true"/> when the values are
    /// equal; otherwise, <see langword="false" />.
    /// </returns>
    public bool Equals(TValue? other)
    {
        if (Option.IsNone(other))
            return this.IsNone;

        return EqualityComparer<TValue>.Default.Equals(this.value, other);
    }

    /// <summary>
    /// Determines if the object is equal to the value.
    /// </summary>
    /// <param name="other">The other value.</param>
    /// <remarks>
    ///     <para>
    ///       If the option is none and the other value is <c>null</c>,
    ///       <c>Nil</c>, <c>DBNull</c>, or <c>ValueTuple</c>, then the result is
    ///       is true. Otherwise, the result is true if the inner
    ///       value is equal to the other value, otherwise, false.
    ///     </para>
    /// </remarks>
    /// <returns>
    /// <see langword="true"/> when the values are
    /// equal; otherwise, <see langword="false" />.
    /// </returns>
    public bool Equals(IOptional<TValue>? other)
    {
        if (Option.IsNone(other))
            return this.IsNone;

        var (value, _) = other;
        return EqualityComparer<TValue>.Default.Equals(this.value, value);
    }

    /// <summary>
    /// Determines if the object is equal to the value.
    /// </summary>
    /// <param name="other">The other value.</param>
    /// <remarks>
    ///     <para>
    ///       Returns true if both options have the none value, otherwise
    ///       a equality comparison is performed to determine if both values
    ///       match.
    ///     </para>
    /// </remarks>
    /// <returns>
    /// <see langword="true"/> when the values are
    /// equal; otherwise, <see langword="false" />.
    /// </returns>
    public bool Equals(Option<TValue>? other)
    {
        if (other is null)
            return false;

        return this.state == other.state &&
               (this.IsNone ||
               (this.IsSome && EqualityComparer<TValue>.Default.Equals(this.value, other.value)));
    }

    // override object.Equals
    public override bool Equals(object? obj)
    {
        if (obj is Option<TValue> other)
            return this.Equals(other);

        if (obj is TValue value)
            return this.Equals(value);

        return false;
    }

    // override object.GetHashCode
#pragma warning disable S2328
    public override int GetHashCode()
    {
        return HashCode.Combine(this.state, this.value);
    }

    /// <summary>
    /// Returns <c>None</c> if the option is <c>None</c>, otherwise,
    /// returns <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The other value to return when this option has a value.</param>
    /// <returns>An option of <paramref name="other"/>.</returns>
    public Option<TValue> And(Option<TValue> other)
    {
        if (this.IsNone)
            return this;

        return other;
    }

    /// <summary>
    /// Returns <c>None</c> if the option is <c>None</c>, otherwise,
    /// returns <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The other value to return when this option has a value.</param>
    /// <returns>An option of <paramref name="other"/>.</returns>
    public Option<TValue> And(TValue other)
    {
        if (this.IsNone)
            return this;

        return other;
    }

    /// <summary>
    /// Returns <c>None</c> if the option is <c>None</c>, otherwise,
    /// returns the result of lazily evaluated <paramref name="generateValue"/>.
    /// </summary>
    /// <param name="generateValue">The other value to return when this option has a value.</param>
    /// <returns>An option of <paramref name="generateValue"/>.</returns>
    public Option<TValue> And(Func<TValue> generateValue)
    {
        if (this.IsNone)
            return Option.None<TValue>();

        return Some(generateValue());
    }

    /// <summary>
    /// Returns <c>None</c> if the option is <c>None</c>, otherwise,
    /// returns the result of lazily evaluated <paramref name="generateOption"/>.
    /// </summary>
    /// <typeparam name="TOther">The type of the other option.</typeparam>
    /// <param name="generateOption">The other value to return when this option has a value.</param>
    /// <returns>An option of <paramref name="generateOption"/>.</returns>
    public Option<TOther> And<TOther>(Func<Option<TOther>> generateOption)
        where TOther : notnull
    {
        if (this.IsNone)
            return Option.None<TOther>();

        return generateOption();
    }

    /// <summary>
    /// Returns the value if the option is <c>Some</c>, otherwise,
    /// throws an exception.
    /// </summary>
    /// <param name="exception">The exception to throw.</param>
    /// <returns>The value or throws.</returns>
    /// <exception cref="Exception">
    /// The exception to throw when the option is <c>None</c>.
    /// </exception>
    public TValue Expect(Exception exception)
    {
        if (this.IsNone)
            throw exception;

        return this.value;
    }

    /// <summary>
    /// Returns the value if the option is <c>Some</c>, otherwise,
    /// generates and throws an exception.
    /// </summary>
    /// <param name="factory">The factory to create an exception.</param>
    /// <returns>The value or throws.</returns>
    /// <exception cref="Exception">
    /// The exception to throw when the option is <c>None</c>.
    /// </exception>
    public TValue Expect(Func<Exception> factory)
    {
        return this.IsNone ? throw factory() : this.value;
    }

    /// <summary>
    /// Returns the value if the option is <c>Some</c>, otherwise,
    /// throws a <see cref="OptionException"/>.
    /// </summary>
    /// <param name="message">The message for the exception.</param>
    /// <returns>The value or throws.</returns>
    /// <exception cref="OptionException">
    /// The exception to throw when the option is <c>None</c>.
    /// </exception>
    public TValue Expect(string message)
    {
        if (this.IsNone)
            throw new OptionException(message);

        return this.value!;
    }

    /// <summary>
    /// Returns the value if the option is <c>Some</c> and matches
    /// the predicate, otherwise returns <c>None</c>.
    /// </summary>
    /// <param name="predicate">The predicate to match.</param>
    /// <returns>An option that is <c>Some</c> and matches
    /// the predicate, otherwise returns <c>None</c>.</returns>
    public Option<TValue> Filter(Func<TValue, bool> predicate)
    {
        if (this.IsNone || !predicate(this.value))
            return Option.None<TValue>();

        return this;
    }

    public Option<TValue> Inspect(Action<TValue> action)
    {
        if (this.IsSome)
            action(this.value);

        return this;
    }

    public bool Match(Func<TValue, bool> predicate)
    {
        if (this.IsNone)
            return false;

        return predicate(this.value);
    }

    /// <summary>
    /// Maps the value to a new value if the option is <c>Some</c>,
    /// otherwise, returns <c>None</c>.
    /// </summary>
    /// <typeparam name="TOther">The type of the other value.</typeparam>
    /// <param name="map">The map function that projects the value.</param>
    /// <returns>The new option.</returns>
    public Option<TOther> Map<TOther>(Func<TValue, TOther> map)
        where TOther : notnull
        => this.IsNone ? Option.None<TOther>() : Option.Some(map(this.value));

    public Option<TOther> MapOrDefault<TOther>(Func<TValue, TOther> map, TOther value)
        where TOther : notnull
    {
        if (this.IsNone)
            return new Option<TOther>(OptionState.Some, value);

        return new Option<TOther>(OptionState.Some, map(this.value));
    }

    public Option<TOther> MapOrDefault<TOther>(Func<TValue, TOther> map, Func<TOther> generate)
        where TOther : notnull
    {
        if (this.IsNone)
            return new Option<TOther>(OptionState.Some, generate());

        return new Option<TOther>(OptionState.Some, map(this.value));
    }

    public async Task<Option<TOther>> MapAsync<TOther>(Func<TValue, Task<TOther>> map)
        where TOther : notnull
    {
        if (this.IsNone)
            return Option<TOther>.None();

        var value = await map(this.value!);
        return new Option<TOther>(OptionState.Some, value);
    }

    public async Task<Option<TOther>> MapAsync<TOther>(
        Func<TValue, CancellationToken, Task<TOther>> map,
        CancellationToken cancellationToken = default)
        where TOther : notnull
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (this.IsNone)
            return Option<TOther>.None();

        var value = await map(this.value!, cancellationToken)
            .ConfigureAwait(false);

        return value;
    }

    public async Task<Option<TOther>> MapOrDefaultAsync<TOther>(
        Func<TValue, Task<TOther>> map,
        Func<Task<TOther>> generate)
        where TOther : notnull
    {
        if (this.IsNone)
        {
            var next = await generate()
                .ConfigureAwait(false);
            return new Option<TOther>(OptionState.Some, next);
        }

        var value = await map(this.value!)
            .ConfigureAwait(false);

        return new Option<TOther>(OptionState.Some, value);
    }

    public async Task<Option<TOther>> MapOrDefaultAsync<TOther>(
        Func<TValue, CancellationToken, Task<TOther>> map,
        Func<CancellationToken, Task<TOther>> generate,
        CancellationToken cancellationToken = default)
        where TOther : notnull
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (this.IsNone)
        {
            var next = await generate(cancellationToken)
                .ConfigureAwait(false);

            return new Option<TOther>(OptionState.Some, next);
        }

        var value = await map(this.value!, cancellationToken)
            .ConfigureAwait(false);

        return new Option<TOther>(OptionState.Some, value);
    }

    /// <summary>
    /// Returns the option if it is <c>Some</c>, otherwise, returns
    /// the <paramref name="other"/> option.
    /// </summary>
    /// <param name="other">The other option.</param>
    /// <returns>An option.</returns>
    public Option<TValue> Or(Option<TValue> other)
        => this.IsNone ? other : this;

    /// <summary>
    /// Returns the option if it is <c>Some</c>, otherwise, returns
    /// the <paramref name="other"/> option.
    /// </summary>
    /// <param name="other">The other option.</param>
    /// <returns>An option.</returns>
    public Option<TValue> Or(TValue other)
        => this.IsNone ? other : this;

    /// <summary>
    /// Returns the option if it is <c>Some</c>, otherwise, returns
    /// the the lazily created option.
    /// </summary>
    /// <param name="factory">The factory to create the other value.</param>
    /// <returns>An option.</returns>
    public Option<TValue> Or(Func<TValue> factory)
        => this.IsNone ? factory() : this;

    /// <summary>
    /// Returns the option if it is <c>Some</c>, otherwise, returns
    /// the the lazily created option.
    /// </summary>
    /// <param name="factory">The factory to create the other value.</param>
    /// <returns>An option.</returns>
    public Option<TValue> Or(Func<Option<TValue>> factory)
        => this.IsNone ? factory() : this;

    public Option<TValue> Replace(TValue value)
    {
        this.value = value!;
        this.state = Nil.IsNil(value) ? OptionState.None : OptionState.Some;
        return this;
    }

    /// <summary>
    /// Takes the value, sets the state to none, returns the value. If the value is none,
    /// then an exception is thrown.
    /// </summary>
    /// <returns>The value.</returns>
    /// <exception cref="OptionException">Thrown when the state is <c>None</c>.</exception>
    public TValue Take()
    {
        this.ThrowIfNone();
        var value = this.value;
        this.value = default!;
        this.state = OptionState.None;
        return value;
    }

    public Result<TValue> ToResult()
        => this.IsNone ? Result.Error<TValue>(new ResultException("No value for option")) : Result.Ok(this.value!);

    public Result<TValue> ToResult(string message)
        => this.IsNone ? Result.Error<TValue>(new ResultException(message)) : Result.Ok(this.value!);

    public Result<TValue> ToResult(Func<Error> generateError)
        => this.IsNone ? Result.Error<TValue>(generateError()) : Result.Ok(this.value!);

    public Result<TValue, TError> ToResult<TError>(Func<TError> generateError)
        => this.IsNone ? new(generateError()) : new(this.value!);

    public ValueResult<TValue> ToValueResult()
        => this.IsNone ? new(new ResultException("No value for option")) : new(this.value!);

    public ValueResult<TValue> ToValueResult(string message)
        => this.IsNone ? new(new ResultException(message)) : new(this.value!);

    public ValueResult<TValue> ToValueResult(Func<Error> generateError)
        => this.IsNone ? new(generateError()) : new(this.value!);

    public ValueResult<TValue, TError> ToValueResult<TError>(Func<TError> generateError)
        => this.IsNone ? new(generateError()) : new(this.value!);

    /// <summary>
    /// Returns the underlying value if it is <c>Some</c>, otherwise, throws
    /// an exception.
    /// </summary>
    /// <returns>The value or throws.</returns>
    /// <exception cref="OptionException">
    /// Thrown when the value is <c>None</c>.
    /// </exception>
    public TValue Unwrap()
    {
        this.ThrowIfNone();
        return this.value;
    }

    /// <summary>
    /// Returns the underlying value if it is <c>Some</c>; otherwise,
    /// returns the <paramref name="defaultValue"/>.
    /// </summary>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The value or default value.</returns>
    public TValue Unwrap(TValue defaultValue)
        => this.IsNone ? defaultValue : this.value!;

    /// <summary>
    /// Returns the underlying value if it is <c>Some</c>; otherwise,
    /// returns the lazily created <paramref name="factory"/>.
    /// </summary>
    /// <param name="factory">The factory to create the default value.</param>
    /// <returns>The value or default value.</returns>
    public TValue Unwrap(Func<TValue> factory)
        => this.IsNone ? factory() : this.value!;

    /// <summary>
    /// Zips the option with another option and returns a tuple of the
    /// values if both options are <c>Some</c>; otherwise, returns <c>None</c>.
    /// </summary>
    /// <typeparam name="TOther">The type of the other option.</typeparam>
    /// <param name="other">The other option to zip.</param>
    /// <returns>The zipped tupple option.</returns>
    public Option<(TValue, TOther)> Zip<TOther>(Option<TOther> other)
        where TOther : notnull
    {
        if (this.IsNone || other.IsNone)
            return Option.None<(TValue, TOther)>();

        return Option.Some((this.value!, other.value!));
    }

    /// <summary>
    /// Throws an exception if the option is <c>None</c>.
    /// </summary>
    /// <returns>The option if an exception is not thrown.</returns>
    /// <exception cref="OptionException">
    /// Throws when the value is <c>None</c>.
    /// </exception>
    public Option<TValue> ThrowIfNone()
    {
        OptionException.ThrowIfNone(this);
        return this;
    }
}