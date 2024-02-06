using System.Diagnostics.CodeAnalysis;

namespace GnomeStack.Functional;

public interface IResult<TValue, TError> : IResult,
    IEquatable<IResult<TValue, TError>>,
    IEquatable<TValue>
{
    /// <summary>
    /// Deconstructs the result into its components.
    /// </summary>
    /// <param name="ok"><see langword="true"/> when ok; otherwise, <see langword="false" />.</param>
    /// <param name="value">The value of the result.</param>
    void Deconstruct(out bool ok, out TValue? value);

    /// <summary>
    /// Deconstructs the result into its components.
    /// </summary>
    /// <param name="ok"><see langword="true"/> when ok; otherwise, <see langword="false" />.</param>
    /// <param name="value">The value of the result.</param>
    /// <param name="error">The error of the result.</param>
    void Deconstruct(out bool ok, out TValue? value, out TError? error);

    /// <summary>
    /// Returns the value or throws a ResultException.
    /// </summary>
    /// <returns>The value.</returns>
    TValue Unwrap();

    /// <summary>
    /// Returns the error or throws a ResultException.
    /// </summary>
    /// <returns>The error.</returns>
    TError UnwrapError();
}