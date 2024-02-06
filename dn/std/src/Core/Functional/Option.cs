using System.Diagnostics.CodeAnalysis;

namespace GnomeStack.Functional;

/// <summary>
///    Provides static methods for creating and manipulating
///    <see cref="Option{TValue}" /> instances.
/// </summary>
public static class Option
{
    public static Nil None()
        => Nil.Value;

#pragma warning disable CS8604 // Possible null reference argument.
    public static Option<T> None<T>()
        where T : notnull
        => new Option<T>(OptionState.None, default);
#pragma warning restore CS8604 // Possible null reference argument.

    public static Option<T> Some<T>(T value)
        where T : notnull
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        return new Option<T>(OptionState.Some, value);
    }

    public static async Task<Option<T>> SomeAsync<T>(Task<T> value)
        where T : notnull
        => Some(await value);

    public static Option<TValue> From<TValue>(TValue? value)
        where TValue : notnull
        => IsNone(value) ? None<TValue>() : Some(value);

    public static Option<TValue> FromNullable<TValue>(TValue? value)
        where TValue : struct
        => value.HasValue ? Some(value.Value) : None();

    public static async Task<Option<T>> FromNullableAsync<T>(Task<T?> value)
        where T : notnull
    {
        var result = await value;
        return IsNone(result) ? None<T>() : Some(result);
    }

    public static bool IsSome([NotNullWhen(true)] object? obj)
        => !Nil.IsNil(obj);

    public static bool IsNone([NotNullWhen(false)] object? obj)
        => Nil.IsNil(obj);
}