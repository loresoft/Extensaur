#pragma warning disable IDE0130 // Namespace does not match folder structure

#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.Collections.Generic;

/// <summary>
/// Provides extension methods for <see cref="ICollection{T}"/> implementations.
/// </summary>
[ExcludeFromCodeCoverage]
#if PUBLIC_EXTENSIONS
public
#endif
static class CollectionExtensions
{
    /// <summary>
    /// Gets the first element from the collection that satisfies the specified predicate,
    /// or creates and adds a new element using the value factory if no matching element is found.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to search in and potentially add to.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="valueFactory">A function that creates a new element when no matching element is found.</param>
    /// <returns>
    /// The first element that satisfies the predicate if found; otherwise,
    /// the new element created by <paramref name="valueFactory"/> and added to the collection.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/>, <paramref name="predicate"/>, or <paramref name="valueFactory"/> is <see langword="null"/>.
    /// </exception>
    public static T FirstOrAdd<T>(this ICollection<T> source, Func<T, bool> predicate, Func<T> valueFactory)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
        if (valueFactory == null)
            throw new ArgumentNullException(nameof(valueFactory));

        // return first match
        foreach (T element in source.Where(predicate))
            return element;

        // no match, use factory
        T value = valueFactory();
        source.Add(value);

        return value;
    }
}
