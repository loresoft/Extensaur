#pragma warning disable IDE0130 // Namespace does not match folder structure

#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Generic;

/// <summary>
/// Provides extension methods for <see cref="IDictionary{TKey, TValue}"/> implementations.
/// </summary>
[ExcludeFromCodeCoverage]
#if PUBLIC_EXTENSIONS
public
#endif
static class DictionaryExtensions
{
    /// <summary>
    /// Gets the value associated with the specified key, or creates and adds a new value using the value factory if the key is not found.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to get or add the value to.</param>
    /// <param name="key">The key to look for in the dictionary.</param>
    /// <param name="valueFactory">A function that creates a new value based on the key when the key is not found.</param>
    /// <returns>
    /// The existing value if the key is found in the dictionary; otherwise,
    /// the new value created by <paramref name="valueFactory"/> and added to the dictionary.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="dictionary"/> or <paramref name="valueFactory"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method will modify the dictionary by adding the new key-value pair if the key is not found.
    /// The value factory function receives the key as a parameter, allowing for key-dependent value creation.
    /// </remarks>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));

        if (valueFactory == null)
            throw new ArgumentNullException(nameof(valueFactory));

        if (dictionary.TryGetValue(key, out var value))
            return value;

        value = valueFactory(key);
        dictionary[key] = value;

        return value;
    }

    /// <summary>
    /// Gets the value associated with the specified key, or adds and returns the provided value if the key is not found.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to get or add the value to.</param>
    /// <param name="key">The key to look for in the dictionary.</param>
    /// <param name="value">The value to add to the dictionary if the key is not found.</param>
    /// <returns>
    /// The existing value if the key is found in the dictionary; otherwise,
    /// the provided <paramref name="value"/> which is added to the dictionary.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="dictionary"/> or <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method will modify the dictionary by adding the new key-value pair if the key is not found.
    /// This overload is useful when you have a pre-computed value to add.
    /// </remarks>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        if (dictionary.TryGetValue(key, out var existing))
            return existing;

        dictionary[key] = value;
        return value;
    }
}
