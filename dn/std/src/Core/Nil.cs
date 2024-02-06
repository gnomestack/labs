using System.Diagnostics.CodeAnalysis;

using GnomeStack.Functional;

namespace GnomeStack;

/// <summary>
/// Nil is a singleton type that represents the absence of a value
/// and can be used to check for null, Nil, DBNull, ValueTuple, ValueTask,
/// or Task.
/// </summary>
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
public readonly struct Nil : IEquatable<Nil>
{
    /// <summary>
    /// Gets the default value of Nil.
    /// </summary>
    public static Nil Value { get; }

    /// <summary>
    /// Implicitly converts ValueTuple to Nil.
    /// </summary>
    /// <param name="_">The value tuple.</param>
    public static implicit operator Nil(ValueTuple _) => Value;

    /// <summary>
    /// Implicitly converts DbNull to Nil.
    /// </summary>
    /// <param name="_">The db null value.</param>
    public static implicit operator Nil(DBNull _) => Value;

    public static implicit operator Nil(ValueTask _) => Value;

    public static implicit operator Nil(Task _) => Value;

    public static implicit operator DBNull(Nil _) => DBNull.Value;

    public static implicit operator ValueTuple(Nil _) => Value;

    public static implicit operator ValueTask(Nil _) => default;

    public static bool IsNil([NotNullWhen(false)] object? obj)
    {
        return obj switch
        {
            IOptional optional => optional.IsNone,
            _ => obj is null or Nil or DBNull or ValueTuple or ValueTask or Task,
        };
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <param name="other">The other value.</param>
    /// <returns>True.</returns>
    public bool Equals(Nil other) => true;

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <param name="obj">The other object.</param>
    /// <returns><see langword="true"/> when equal to nil; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => IsNil(obj);

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The hashcode.</returns>
    public override int GetHashCode() => 0;

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>The string value of nil.</returns>
    public override string ToString() => "void";
}