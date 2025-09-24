#nullable enable

namespace System;

/// <summary>
/// Extension methods for <see cref="Type"/>.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Gets the underlying type argument of the specified nullable type.
    /// </summary>
    /// <param name="type">Type that describes a closed generic nullable type</param>
    /// <returns>Returns the underlying type argument of the specified nullable type.</returns>
    public static Type GetUnderlyingType(this Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return Nullable.GetUnderlyingType(type) ?? type;
    }

    /// <summary>
    /// Determines whether the specified <paramref name="type"/> can be null.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>
    ///   <c>true</c> if the specified <paramref name="type"/> can be null; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullable(this Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (!type.IsValueType)
            return true;

        return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>));
    }

    /// <summary>
    /// Determines whether the specified type implements an interface.
    /// </summary>
    /// <typeparam name="TInterface">The type of the interface.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if type implements the interface; otherwise <c>false</c></returns>
    /// <exception cref="InvalidOperationException">Only interfaces can be implemented.</exception>
    public static bool Implements<TInterface>(this Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        var interfaceType = typeof(TInterface);

        if (!interfaceType.IsInterface)
            throw new InvalidOperationException("Only interfaces can be implemented.");

        return interfaceType.IsAssignableFrom(type);
    }
}
