using GnomeStack.Functional;

namespace GnomeStack.Extras.Functional;

public static class OptionExtensions
{
    public static Option<TValue> ToOption<TValue>(this TValue value)
        where TValue : notnull
    {
        return Option<TValue>.Some(value);
    }

    public static Option<TValue> ToOption<TValue>(this TValue? value)
        where TValue : struct
    {
        return value.HasValue ?
            Option<TValue>.Some(value.Value) :
            Option<TValue>.None();
    }
}