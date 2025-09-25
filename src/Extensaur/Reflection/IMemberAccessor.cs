#pragma warning disable IDE0130 // Namespace does not match folder structure

#nullable enable

namespace System.Reflection;

/// <summary>
/// An <see langword="interface"/> for late binding member accessors that provides access to member values.
/// </summary>
/// <remarks>
/// This interface extends <see cref="IMemberInformation"/> to provide runtime access to member values
/// through reflection-based getters and setters. Implementations of this interface typically use
/// compiled expressions for high-performance member access.
/// </remarks>
#if PUBLIC_EXTENSIONS
public
#endif
interface IMemberAccessor : IMemberInformation
{
    /// <summary>
    /// Returns the value of the member for the specified instance.
    /// </summary>
    /// <param name="instance">The instance whose member value will be returned. Can be <see langword="null"/> for static members.</param>
    /// <returns>The member value for the instance parameter, or <see langword="null"/> if the member value is <see langword="null"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the member does not support getting values.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance type is incompatible with the member's declaring type.</exception>
    object? GetValue(object? instance);

    /// <summary>
    /// Sets the <paramref name="value"/> of the member for the specified instance.
    /// </summary>
    /// <param name="instance">The instance whose member value will be set. Can be <see langword="null"/> for static members.</param>
    /// <param name="value">The new value for this member. The value should be compatible with the member's type.</param>
    /// <exception cref="InvalidOperationException">Thrown when the member does not support setting values (e.g., read-only properties or fields).</exception>
    /// <exception cref="ArgumentException">Thrown when the instance type is incompatible with the member's declaring type, or when the value type is incompatible with the member type.</exception>
    void SetValue(object? instance, object? value);
}
