#pragma warning disable IDE0130 // Namespace does not match folder structure

#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Generic;

/// <summary>
/// Provides extension methods for <see cref="IEnumerable{T}"/> and <see cref="System.Collections.IEnumerable"/> implementations.
/// </summary>
[ExcludeFromCodeCoverage]
#if PUBLIC_EXTENSIONS
public
#endif
static class EnumerableExtensions
{

    /// <summary>
    /// Returns the element at the specified index in the enumerable sequence.
    /// </summary>
    /// <param name="source">The enumerable sequence to retrieve an element from.</param>
    /// <param name="index">The zero-based index of the element to retrieve.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative or exceeds the number of elements in the sequence.
    /// </exception>
    /// <remarks>
    /// This method provides indexed access to non-generic <see cref="System.Collections.IEnumerable"/> sequences.
    /// If the index is out of range, an exception is thrown rather than returning a default value.
    /// </remarks>
    public static object ElementAt(this IEnumerable source, int index)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        var item = source.ElementAtOrDefault(index);

        if (item != null)
            return item!;

        throw new ArgumentOutOfRangeException(nameof(index), "Index exceeds the number of elements.");
    }

    /// <summary>
    /// Returns the element at the specified index in the enumerable sequence, or <see langword="null"/> if the index is out of range.
    /// </summary>
    /// <param name="source">The enumerable sequence to retrieve an element from.</param>
    /// <param name="index">The zero-based index of the element to retrieve.</param>
    /// <returns>
    /// The element at the specified index if it exists; otherwise, <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// This method provides safe indexed access to non-generic <see cref="System.Collections.IEnumerable"/> sequences.
    /// Performance is optimized by checking if the source is an array or implements <see cref="System.Collections.IList"/>
    /// for direct index access before falling back to enumeration.
    /// Returns <see langword="null"/> for negative indices or when the index exceeds the sequence length.
    /// </remarks>
    public static object? ElementAtOrDefault(this IEnumerable source, int index)
    {
        if (source == null || index < 0)
            return null;

        // Optimization: Check if source is an array for fastest index access
        if (source is Array array)
            return index < array.Length ? array.GetValue(index) : null;

        // Optimization: Check if source implements IList for direct index access
        if (source is IList list)
            return index < list.Count ? list[index] : null;

        // Fallback to enumeration for other IEnumerable implementations
        int currentIndex = 0;
        foreach (var item in source)
        {
            if (currentIndex == index)
                return item;

            currentIndex++;
        }

        return null;
    }


    /// <summary>
    /// Concatenates the string representations of the elements in a sequence, using the specified delimiter between each element.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
    /// <param name="values">The sequence of values to concatenate. Each value will be converted to a string using its <see cref="object.ToString()"/> method.</param>
    /// <param name="delimiter">The string to use as a delimiter between elements. If <see langword="null"/>, a comma (",") is used by default.</param>
    /// <returns>
    /// A string that consists of the string representations of the elements in <paramref name="values"/> delimited by the <paramref name="delimiter"/> string.
    /// If <paramref name="values"/> is empty, returns <see cref="string.Empty"/>.
    /// </returns>
    /// <remarks>
    /// <see langword="null"/> elements in the sequence will be converted to empty strings in the result.
    /// This method is equivalent to calling <see cref="string.Join{T}(string, IEnumerable{T})"/> with the provided parameters.
    /// </remarks>
    public static string ToDelimitedString<T>(this IEnumerable<T?> values, string? delimiter = ",")
        => string.Join(delimiter ?? ",", values);

    /// <summary>
    /// Concatenates the elements of a string sequence, using the specified delimiter between each element.
    /// </summary>
    /// <param name="values">The sequence of string values to concatenate.</param>
    /// <param name="delimiter">The string to use as a delimiter between elements. If <see langword="null"/>, a comma (",") is used by default.</param>
    /// <returns>
    /// A string that consists of the elements in <paramref name="values"/> delimited by the <paramref name="delimiter"/> string.
    /// If <paramref name="values"/> is empty, returns <see cref="string.Empty"/>.
    /// </returns>
    /// <remarks>
    /// <see langword="null"/> elements in the sequence will be treated as empty strings in the result.
    /// This method is equivalent to calling <see cref="string.Join(string, IEnumerable{string})"/> with the provided parameters.
    /// </remarks>
    public static string ToDelimitedString(this IEnumerable<string?> values, string? delimiter = ",")
        => string.Join(delimiter ?? ",", values);
}
