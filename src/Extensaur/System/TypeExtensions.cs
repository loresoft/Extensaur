#pragma warning disable IDE0130 // Namespace does not match folder structure

#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace System;

/// <summary>
/// Provides extension methods for <see cref="Type"/> to enhance type inspection and reflection operations.
/// </summary>
[ExcludeFromCodeCoverage]
#if PUBLIC_EXTENSIONS
public
#endif
static class TypeExtensions
{
    /// <summary>
    /// Gets the underlying type argument of the specified nullable type, or the type itself if it's not nullable.
    /// This method safely handles both nullable value types (Nullable&lt;T&gt;) and reference types.
    /// </summary>
    /// <param name="type">The type to examine for an underlying nullable type.</param>
    /// <returns>
    /// The underlying type argument if <paramref name="type"/> is a nullable value type;
    /// otherwise, returns the original <paramref name="type"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public static Type GetUnderlyingType(this Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return Nullable.GetUnderlyingType(type) ?? type;
    }

    /// <summary>
    /// Determines whether the specified type can accept null values.
    /// Reference types and nullable value types can accept null values, while non-nullable value types cannot.
    /// </summary>
    /// <param name="type">The type to check for nullability.</param>
    /// <returns>
    /// <c>true</c> if the specified <paramref name="type"/> can accept null values
    /// (i.e., it's a reference type or a nullable value type); otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public static bool IsNullable(this Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (!type.IsValueType)
            return true;

        return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>));
    }

    /// <summary>
    /// Determines whether the specified type implements the given interface type.
    /// </summary>
    /// <typeparam name="TInterface">The interface type to check for implementation. Must be an interface type.</typeparam>
    /// <param name="type">The type to check for interface implementation.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="type"/> implements the <typeparamref name="TInterface"/> interface;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <typeparamref name="TInterface"/> is not an interface type.</exception>
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
