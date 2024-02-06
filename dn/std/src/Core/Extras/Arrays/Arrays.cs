using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace GnomeStack.Extras.Arrays;

public static partial class ArrayExtensions
{
    public static T At<T>(this T[] array, int index)
        => array[index];

    /// <summary>
    /// Clears the array of values.
    /// </summary>
    /// <param name="array">The one dimensional array to clear.</param>
    /// <param name="index">The zero-based position to start clearing values.</param>
    /// <param name="length">The number of elements to clear. If the length is less than 0, it is
    /// set to to the length of the array minus the index.</param>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the length exceeds the length of the array.
    /// </exception>
    public static void Clear<T>(this T[] array, int index = 0, int length = -1)
    {
        if (length == -1)
            length = array.Length - index;

        if (length > array.Length - index)
        {
            throw new ArgumentOutOfRangeException(
                nameof(length),
                length,
                $"Length ({length}) + Index ({index}) must not exceed the length of the array ({array.Length}).");
        }

        Array.Clear(array, index, length);
    }

    /// <summary>
    /// Concatenates the two arrays into a single new array.
    /// </summary>
    /// <param name="array1">The first array to concatenate.</param>
    /// <param name="array2">The second array to concatenate.</param>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <returns>A new array.</returns>
    public static T[] Concat<T>(this T[] array1, T[] array2)
    {
        var result = new T[array1.Length + array2.Length];
        Array.Copy(array1, result, array1.Length);
        Array.Copy(array2, 0, result, array1.Length, array2.Length);
        return result;
    }

    /// <summary>
    /// Concatenates the two arrays into a single new array.
    /// </summary>
    /// <param name="array1">The first array to concatenate.</param>
    /// <param name="array2">The second array to concatenate.</param>
    /// <param name="array3">The third array to concatenate.</param>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <returns>A new array.</returns>
    public static T[] Concat<T>(this T[] array1, T[] array2, T[] array3)
    {
        var result = new T[array1.Length + array2.Length + array3.Length];
        Array.Copy(array1, result, array1.Length);
        Array.Copy(array2, 0, result, array1.Length, array2.Length);
        Array.Copy(array3, 0, result, array1.Length + array2.Length, array3.Length);
        return result;
    }

    /// <summary>
    /// Concatenates multiple arrays into a single new array.
    /// </summary>
    /// <param name="array1">The first array to concatinate.</param>
    /// <param name="arrays">The array of arrays to concatenate.</param>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <returns>A new array.</returns>
    #pragma warning disable S2368 // Prefer jagged arrays over multidimensional
    public static T[] Concat<T>(this T[] array1, params T[][] arrays)
    {
        var result = new T[arrays.Sum(a => a.Length) + array1.Length];
        var offset = 0;
        Array.Copy(array1, result, array1.Length);
        offset += array1.Length;

        foreach (var array in arrays)
        {
            Array.Copy(array, 0, result, offset, array.Length);
            offset += array.Length;
        }

        return result;
    }

    /// <summary>
    /// Determines whether the array contains the specified item.
    /// </summary>
    /// <param name="array">The array to check.</param>
    /// <param name="item">The item.</param>
    /// <typeparam name="T">The element type.</typeparam>
    /// <returns>Returns <see langword="true" /> when the item is found in the array; otherwise, <see langword="false" />.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Contains<T>(this T[] array, T item)
        => Array.IndexOf(array, item) != -1;

    public static bool Contains<T>(this T[] array, T item, IEqualityComparer<T> comparer)
    {
        if (comparer == null)
            return Contains(array, item);

        for (var i = 0; i < array.Length; i++)
        {
            if (comparer.Equals(array[i], item))
                return true;
        }

        return false;
    }

    public static bool Contains(this string[] array, string item, StringComparer comparer)
    {
        if (comparer == null)
            return Contains(array, item);

        for (var i = 0; i < array.Length; i++)
        {
            if (comparer.Equals(array[i], item))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Compares two arrays for equality.
    /// </summary>
    /// <param name="left">The left side of the compare.</param>
    /// <param name="right">The right side of the compare.</param>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <returns><c>True</c> when both objects are equal; otherwise, <c>false</c>.</returns>
    public static bool EqualTo<T>(this T[] left, T[] right)
    {
        return EqualTo(left, right, EqualityComparer<T>.Default);
    }

    /// <summary>
    /// Compares two arrays for equality using the <paramref name="comparer"/>.
    /// </summary>
    /// <param name="left">The left side of the compare.</param>
    /// <param name="right">The right side of the compare.</param>
    /// <param name="comparer">The comparer implementation to use.</param>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <returns><c>True</c> when both objects are equal; otherwise, <c>false</c>.</returns>
    public static bool EqualTo<T>(this T[] left, T[] right, IComparer<T> comparer)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left == null || right == null)
            return false;

        if (left.Length != right.Length)
            return false;

        for (int i = 0; i < left.Length; i++)
        {
            var lValue = left[i];
            var rValue = right[i];

            if (comparer.Compare(lValue, rValue) == 0)
                continue;

            return false;
        }

        return true;
    }

    /// <summary>
    /// Compares two arrays for equality using the <paramref name="comparer"/>.
    /// </summary>
    /// <param name="left">The left side of the compare.</param>
    /// <param name="right">The right side of the compare.</param>
    /// <param name="comparer">The comparison delegate to use.</param>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <returns><c>True</c> when both objects are equal; otherwise, <c>false</c>.</returns>
    public static bool EqualTo<T>(this T[]? left, T[]? right, Comparison<T> comparer)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left == null || right == null)
            return false;

        if (left.Length != right.Length)
            return false;

        for (int i = 0; i < left.Length; i++)
        {
            var lValue = left[i];
            var rValue = right[i];

            if (comparer(lValue, rValue) == 0)
                continue;

            return false;
        }

        return true;
    }

    /// <summary>
    /// Compares two arrays for equality using the <paramref name="comparer"/>.
    /// </summary>
    /// <param name="left">The left side of the compare.</param>
    /// <param name="right">The right side of the compare.</param>
    /// <param name="comparer">The equality comparer implementation to use.</param>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <returns><c>True</c> when both objects are equal; otherwise, <c>false</c>.</returns>
    public static bool EqualTo<T>(this T[]? left, T[]? right, IEqualityComparer<T> comparer)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left == null || right == null)
            return false;

        if (left.Length != right.Length)
            return false;

        for (int i = 0; i < left.Length; i++)
        {
            var lValue = left[i];
            var rValue = right[i];

            if (comparer.Equals(lValue, rValue))
                continue;

            return false;
        }

        return true;
    }

    /// <summary>
    /// Iterates through the array and calls the action for each element.
    /// </summary>
    /// <param name="array">The array to iterate.</param>
    /// <param name="action">The action to apply to each item.</param>
    public static void ForEach(this Array array, Action<Array, int[]> action)
    {
        if (array.LongLength == 0)
            return;

        var walker = new ArrayTraverse(array);
        do action(array, walker.Position);
        while (walker.Step());
    }

    /// <summary>
    /// Creates a new <see cref="ArraySegment{T}"/> from the given array.
    /// </summary>
    /// <param name="array">The one dimensional array.</param>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <returns>A new <see cref="ArraySegment{T}"/>.</returns>
    public static ArraySegment<T> Segment<T>(this T[] array)
        => Segment(array, 0, array.Length);

    /// <summary>
    /// Creates a new <see cref="ArraySegment{T}"/> from the given array.
    /// </summary>
    /// <param name="array">The one dimensional array.</param>
    /// <param name="start">The zero-based position that will be the start of the segment.</param>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <returns>A new <see cref="ArraySegment{T}"/>.</returns>
    public static ArraySegment<T> Segment<T>(this T[] array, int start)
        => Segment(array, start, array.Length - start);

    /// <summary>
    /// Creates a new <see cref="ArraySegment{T}"/> from the given array.
    /// </summary>
    /// <param name="array">The one dimensional array.</param>
    /// <param name="start">The zero-based position that will be the start of the segment.</param>
    /// <param name="length">The number of elements from the array to include in the segment.</param>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <returns>A new <see cref="ArraySegment{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the <paramref name="array"/> is null.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the <paramref name="start"/> or <paramref name="length"/> is less than zero.
    /// Thrown when the <paramref name="start"/> plus <paramref name="length"/> is greater than
    /// <paramref name="array"/>'s length.
    /// </exception>
    public static ArraySegment<T> Segment<T>(this T[] array, int start, int length)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        if (start < 0)
            throw new ArgumentOutOfRangeException(nameof(start));

        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        if ((start + length) > array.Length)
            throw new ArgumentOutOfRangeException(nameof(length));

        return new ArraySegment<T>(array, start, length);
    }

    /// <summary>
    /// Creates a slice of the given array as a <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="array">The one dimensional array to slice from.</param>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <returns>A <see cref="Span{T}"/>.</returns>
    public static Span<T> Slice<T>(this T[] array)
        => Slice(array, 0, array.Length);

    /// <summary>
    /// Creates a slice of the given array as a <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="array">The one dimensional array to slice from.</param>
    /// <param name="start">The zero-based position to start the slice.</param>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <returns>A <see cref="Span{T}"/>.</returns>
    public static Span<T> Slice<T>(this T[] array, int start)
        => Slice(array, start, array.Length - start);

    /// <summary>
    /// Creates a slice of the given array as a <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="array">The one dimensional array to slice from.</param>
    /// <param name="start">The zero-based position to start the slice.</param>
    /// <param name="length">The number of elements to include in the slice.</param>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <returns>A <see cref="Span{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the <paramref name="array"/> is null.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the <paramref name="start"/> or <paramref name="length"/> is less than zero.
    /// Thrown when the <paramref name="start"/> plus <paramref name="length"/> is greater than
    /// <paramref name="array"/>'s length.
    /// </exception>
    public static Span<T> Slice<T>(this T[] array, int start, int length)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        if (start < 0)
            throw new ArgumentOutOfRangeException(nameof(start));

        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        if ((start + length) > array.Length)
            throw new ArgumentOutOfRangeException(nameof(length));

        return array.AsSpan(start, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<T>(this T[] array)
        => Array.Sort(array);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<T>(this T[] array, IComparer<T>? comparer)
        => Array.Sort(array, comparer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<T>(this T[] array, Comparer<T> comparer)
        => Array.Sort(array, comparer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<T>(this T[] array, Comparison<T> comparison)
        => Array.Sort(array, comparison);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<T>(this T[] array, int index)
        => Array.Sort(array, index, array.Length - index);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<T>(this T[] array, int index, int length, IComparer<T> comparer)
        => Array.Sort(array, index, length, comparer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<T>(this T[] array, int index, int length, Comparer<T> comparer)
        => Array.Sort(array, index, length, comparer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<T>(this T[] array, int index, int length, Comparison<T> comparison)
        => Array.Sort(array, index, length, new ArrayComparer<T>(comparison));

    /// <summary>
    /// Swaps the values of the two items in the array.
    /// </summary>
    /// <param name="array">The one dimensional array where values will be swapped.</param>
    /// <param name="index1">The first zero-based index where the first item exists.</param>
    /// <param name="index2">The second zero-based index where the second item exists.</param>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Swap<T>(this T[] array, int index1, int index2)
    {
        (array[index1], array[index2]) = (array[index2], array[index1]);
    }

    private sealed class ArrayComparer<T> : Comparer<T>
    {
        private readonly Comparison<T> comparison;

        public ArrayComparer(Comparison<T> comparison)
        {
            this.comparison = comparison;
        }

        public override int Compare(T? x, T? y)
            => this.comparison(x!, y!);
    }

    private sealed class ArrayTraverse
    {
        private readonly int[] maxLengths;

        public ArrayTraverse(Array array)
        {
            this.maxLengths = new int[array.Rank];
            for (int i = 0; i < array.Rank; ++i)
            {
                this.maxLengths[i] = array.GetLength(i) - 1;
            }

            this.Position = new int[array.Rank];
        }

        public int[] Position { get; }

        public bool Step()
        {
            for (int i = 0; i < this.Position.Length; ++i)
            {
                if (this.Position[i] >= this.maxLengths[i])
                {
                    continue;
                }

                this.Position[i]++;
                for (int j = 0; j < i; j++)
                {
                    this.Position[j] = 0;
                }

                return true;
            }

            return false;
        }
    }
}