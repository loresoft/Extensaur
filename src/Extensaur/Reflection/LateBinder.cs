#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace System.Reflection;

/// <summary>
/// A static class that provides late-binding operations for types, allowing dynamic access to methods, properties, and fields at runtime.
/// </summary>
/// <remarks>
/// The <see cref="LateBinder"/> class provides a simplified interface for reflection operations,
/// automatically handling type resolution and member lookup. It supports both public and non-public members
/// through configurable binding flags.
/// </remarks>
public static class LateBinder
{
    /// <summary>
    /// Default binding flags for public members including instance members and flattened hierarchy.
    /// </summary>
    public const BindingFlags DefaultPublicFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

    /// <summary>
    /// Default binding flags for both public and non-public members including instance members and flattened hierarchy.
    /// </summary>
    public const BindingFlags DefaultNonPublicFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

    /// <summary>
    /// Searches for the specified public method with the specified name using the provided arguments for type resolution.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to search for the method in.</param>
    /// <param name="name">The name of the method to find.</param>
    /// <param name="arguments">The arguments used to determine the method signature for overload resolution.</param>
    /// <returns>
    /// An <see cref="IMethodAccessor"/> instance for the method if found; otherwise <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <remarks>
    /// This method uses <see cref="DefaultPublicFlags"/> for the search. The arguments are used to determine
    /// the parameter types for method overload resolution.
    /// </remarks>
    public static IMethodAccessor? FindMethod(Type type, string name, params IEnumerable<Type> arguments)
    {
        return FindMethod(type, name, DefaultPublicFlags, arguments);
    }

    /// <summary>
    /// Searches for the specified method with the specified name and binding constraints, using the provided arguments for type resolution.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to search for the method in.</param>
    /// <param name="name">The name of the method to find.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <param name="arguments">The arguments used to determine the method signature for overload resolution.</param>
    /// <returns>
    /// An <see cref="IMethodAccessor"/> instance for the method if found; otherwise <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <remarks>
    /// The arguments are analyzed to determine their types, which are then used for method overload resolution.
    /// Null arguments are treated as <see cref="object"/> type for matching purposes.
    /// </remarks>
    public static IMethodAccessor? FindMethod(Type type, string name, BindingFlags flags, params IEnumerable<Type> arguments)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        var typeAccessor = TypeAccessor.GetAccessor(type);
        return typeAccessor.FindMethod(name, [.. arguments], flags);
    }

    /// <summary>
    /// Searches for a property using a strongly-typed property expression.
    /// </summary>
    /// <typeparam name="T">The object type containing the property specified in the expression.</typeparam>
    /// <param name="propertyExpression">The property expression (e.g. p => p.PropertyName) that identifies the property to find.</param>
    /// <returns>An <see cref="IMemberAccessor"/> instance for the property if found; otherwise <c>null</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyExpression"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the expression is not a <see cref="MemberExpression"/>, 
    /// the <see cref="MemberExpression"/> does not represent a property, or the property is static.</exception>
    /// <remarks>
    /// This method provides compile-time safety for property access by using strongly-typed expressions.
    /// The expression is analyzed to extract the property information without requiring string-based names.
    /// </remarks>
    public static IMemberAccessor? FindProperty<T>(Expression<Func<T>> propertyExpression)
    {
        if (propertyExpression is null)
            throw new ArgumentNullException(nameof(propertyExpression));

        var typeAccessor = TypeAccessor.GetAccessor<T>();
        return typeAccessor.FindProperty(propertyExpression);
    }

    /// <summary>
    /// Searches for the public property with the specified name.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to search for the property in.</param>
    /// <param name="name">The name of the property to find.</param>
    /// <returns>
    /// An <see cref="IMemberAccessor"/> instance for the property if found; otherwise <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <remarks>
    /// This method uses <see cref="DefaultPublicFlags"/> for the search, which includes public instance properties
    /// and properties from the inheritance hierarchy.
    /// </remarks>
    public static IMemberAccessor? FindProperty(Type type, string name)
    {
        return FindProperty(type, name, DefaultPublicFlags);
    }

    /// <summary>
    /// Searches for the specified property using the specified binding constraints.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to search for the property in.</param>
    /// <param name="name">The name of the property to find.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>
    /// An <see cref="IMemberAccessor"/> instance for the property if found; otherwise <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <remarks>
    /// The binding flags control which properties are considered during the search, including visibility,
    /// static vs instance, and inheritance hierarchy traversal.
    /// </remarks>
    public static IMemberAccessor? FindProperty(Type type, string name, BindingFlags flags)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        var typeAccessor = TypeAccessor.GetAccessor(type);
        return typeAccessor.FindProperty(name, flags);
    }

    /// <summary>
    /// Searches for the field with the specified name using both public and non-public instance fields.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to search for the field in.</param>
    /// <param name="name">The name of the field to find.</param>
    /// <returns>
    /// An <see cref="IMemberAccessor"/> instance for the field if found; otherwise <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <remarks>
    /// This method searches both public and non-public instance fields by default, as fields are commonly
    /// used for internal state and may not be publicly accessible.
    /// </remarks>
    public static IMemberAccessor? FindField(Type type, string name)
    {
        return FindField(type, name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }

    /// <summary>
    /// Searches for the field using the specified binding constraints.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to search for the field in.</param>
    /// <param name="name">The name of the field to find.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>
    /// An <see cref="IMemberAccessor"/> instance for the field if found; otherwise <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <remarks>
    /// The binding flags control which fields are considered during the search, including visibility,
    /// static vs instance, and inheritance hierarchy traversal.
    /// </remarks>
    public static IMemberAccessor? FindField(Type type, string name, BindingFlags flags)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        var typeAccessor = TypeAccessor.GetAccessor(type);
        return typeAccessor.FindField(name, flags);
    }

    /// <summary>
    /// Searches for a property or field with the specified name, trying properties first, then fields.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to search for the property or field in.</param>
    /// <param name="name">The name of the property or field to find.</param>
    /// <returns>
    /// An <see cref="IMemberAccessor"/> instance for the property or field if found; otherwise <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <remarks>
    /// This method searches both public and non-public instance members by default. It first attempts
    /// to find a property with the specified name, and if not found, searches for a field.
    /// </remarks>
    public static IMemberAccessor? Find(Type type, string name)
    {
        return Find(type, name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }

    /// <summary>
    /// Searches for a property or field using the specified binding constraints, trying properties first, then fields.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to search for the property or field in.</param>
    /// <param name="name">The name of the property or field to find.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>
    /// An <see cref="IMemberAccessor"/> instance for the property or field if found; otherwise <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <remarks>
    /// This method provides a unified search for both properties and fields. Properties are searched first,
    /// and if no matching property is found, the search continues with fields using the same binding constraints.
    /// </remarks>
    public static IMemberAccessor? Find(Type type, string name, BindingFlags flags)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        var typeAccessor = TypeAccessor.GetAccessor(type);
        return typeAccessor.Find(name, flags);
    }

    /// <summary>
    /// Sets the public property value with the specified name on the target object.
    /// </summary>
    /// <param name="target">The object whose property value will be set.</param>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The new value to be set.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the property is not found in the target object's type.</exception>
    /// <remarks>
    /// This method supports nested property names using dot notation. For example, 'Person.Address.ZipCode' 
    /// will navigate through the object hierarchy to set the final property value.
    /// </remarks>
    public static void SetProperty(object target, string name, object? value)
    {
        SetProperty(target, name, value, DefaultPublicFlags);
    }

    /// <summary>
    /// Sets the property value with the specified name on the target object using the specified binding constraints.
    /// </summary>
    /// <param name="target">The object whose property value will be set.</param>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The new value to be set.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the property is not found in the target object's type.</exception>
    /// <remarks>
    /// This method supports nested property names using dot notation. For example, 'Person.Address.ZipCode' 
    /// will navigate through the object hierarchy to set the final property value.
    /// </remarks>
    public static void SetProperty(object target, string name, object? value, BindingFlags flags)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        var rootType = target.GetType();
        var memberAccessor = FindProperty(rootType, name, flags);

        if (memberAccessor is null)
            throw new InvalidOperationException($"Could not find property '{name}' in type '{rootType.Name}'.");

        memberAccessor.SetValue(target, value);
    }

    /// <summary>
    /// Sets the public field value with the specified name on the target object.
    /// </summary>
    /// <param name="target">The object whose field value will be set.</param>
    /// <param name="name">The name of the field to set.</param>
    /// <param name="value">The new value to be set.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the field is not found in the target object's type.</exception>
    public static void SetField(object target, string name, object? value)
    {
        SetField(target, name, value, DefaultPublicFlags);
    }

    /// <summary>
    /// Sets the field value with the specified name on the target object using the specified binding constraints.
    /// </summary>
    /// <param name="target">The object whose field value will be set.</param>
    /// <param name="name">The name of the field to set.</param>
    /// <param name="value">The new value to be set.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the field is not found in the target object's type.</exception>
    public static void SetField(object target, string name, object? value, BindingFlags flags)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        var rootType = target.GetType();
        var memberAccessor = FindField(rootType, name, flags);

        if (memberAccessor is null)
            throw new InvalidOperationException($"Could not find field '{name}' in type '{rootType.Name}'.");

        memberAccessor.SetValue(target, value);
    }

    /// <summary>
    /// Sets the public property or field value with the specified name on the target object.
    /// </summary>
    /// <param name="target">The object whose property or field value will be set.</param>
    /// <param name="name">The name of the property or field to set.</param>
    /// <param name="value">The new value to be set.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when neither a property nor field with the specified name is found in the target object's type.</exception>
    /// <remarks>
    /// This method first searches for a property with the specified name, and if not found, searches for a field.
    /// </remarks>
    public static void Set(object target, string name, object? value)
    {
        Set(target, name, value, DefaultPublicFlags);
    }

    /// <summary>
    /// Sets the property or field value with the specified name on the target object using the specified binding constraints.
    /// </summary>
    /// <param name="target">The object whose property or field value will be set.</param>
    /// <param name="name">The name of the property or field to set.</param>
    /// <param name="value">The new value to be set.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when neither a property nor field with the specified name is found in the target object's type.</exception>
    /// <remarks>
    /// This method first searches for a property with the specified name, and if not found, searches for a field
    /// using the same binding constraints.
    /// </remarks>
    public static void Set(object target, string name, object? value, BindingFlags flags)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        var rootType = target.GetType();
        var memberAccessor = Find(rootType, name, flags);

        if (memberAccessor is null)
            throw new InvalidOperationException($"Could not find a property or field with a name of '{name}' in type '{rootType.Name}'.");

        memberAccessor.SetValue(target, value);
    }

    /// <summary>
    /// Returns the value of the public property with the specified name from the target object.
    /// </summary>
    /// <param name="target">The object whose property value will be returned.</param>
    /// <param name="name">The name of the property to read.</param>
    /// <returns>The value of the property.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the property is not found in the target object's type.</exception>
    public static object? GetProperty(object target, string name)
    {
        return GetProperty(target, name, DefaultPublicFlags);
    }

    /// <summary>
    /// Returns the value of the property with the specified name from the target object using the specified binding constraints.
    /// </summary>
    /// <param name="target">The object whose property value will be returned.</param>
    /// <param name="name">The name of the property to read.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>The value of the property.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the property is not found in the target object's type.</exception>
    public static object? GetProperty(object target, string name, BindingFlags flags)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        var rootType = target.GetType();
        var memberAccessor = FindProperty(rootType, name, flags);
        if (memberAccessor is null)
            throw new InvalidOperationException($"Could not find property '{name}' in type '{rootType.Name}'.");

        return memberAccessor.GetValue(target);
    }

    /// <summary>
    /// Returns the value of the public field with the specified name from the target object.
    /// </summary>
    /// <param name="target">The object whose field value will be returned.</param>
    /// <param name="name">The name of the field to read.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the field is not found in the target object's type.</exception>
    public static object? GetField(object target, string name)
    {
        return GetField(target, name, DefaultPublicFlags);
    }

    /// <summary>
    /// Returns the value of the field with the specified name from the target object using the specified binding constraints.
    /// </summary>
    /// <param name="target">The object whose field value will be returned.</param>
    /// <param name="name">The name of the field to read.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the field is not found in the target object's type.</exception>
    public static object? GetField(object target, string name, BindingFlags flags)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        var rootType = target.GetType();
        var memberAccessor = FindField(rootType, name, flags);
        if (memberAccessor is null)
            throw new InvalidOperationException($"Could not find field '{name}' in type '{rootType.Name}'.");

        return memberAccessor.GetValue(target);
    }

    /// <summary>
    /// Returns the value of the public property or field with the specified name from the target object.
    /// </summary>
    /// <param name="target">The object whose property or field value will be returned.</param>
    /// <param name="name">The name of the property or field to read.</param>
    /// <returns>The value of the property or field.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when neither a property nor field with the specified name is found in the target object's type.</exception>
    /// <remarks>
    /// This method first searches for a property with the specified name, and if not found, searches for a field.
    /// </remarks>
    public static object? Get(object target, string name)
    {
        return Get(target, name, DefaultPublicFlags);
    }

    /// <summary>
    /// Returns the value of the property or field with the specified name from the target object using the specified binding constraints.
    /// </summary>
    /// <param name="target">The object whose property or field value will be returned.</param>
    /// <param name="name">The name of the property or field to read.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>The value of the property or field.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when neither a property nor field with the specified name is found in the target object's type.</exception>
    /// <remarks>
    /// This method first searches for a property with the specified name, and if not found, searches for a field
    /// using the same binding constraints.
    /// </remarks>
    public static object? Get(object target, string name, BindingFlags flags)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        var rootType = target.GetType();
        var memberAccessor = Find(rootType, name, flags);
        if (memberAccessor is null)
            throw new InvalidOperationException($"Could not find a property or field with a name of '{name}' in type '{rootType.Name}'.");

        return memberAccessor.GetValue(target);
    }

    /// <summary>
    /// Creates an instance of the specified type using its default constructor.
    /// </summary>
    /// <param name="type">The type to create an instance of.</param>
    /// <returns>A new instance of the specified type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no suitable constructor is found for the specified type.</exception>
    /// <remarks>
    /// This method attempts to create an instance using the type's default (parameterless) constructor.
    /// If the type doesn't have a default constructor, an exception will be thrown.
    /// </remarks>
    public static object? CreateInstance(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        var typeAccessor = TypeAccessor.GetAccessor(type);
        if (typeAccessor is null)
            throw new InvalidOperationException($"Could not find constructor for {type.Name}.");

        return typeAccessor.Create();
    }

    /// <summary>
    /// Invokes a method on the target object with the specified name and arguments.
    /// </summary>
    /// <param name="target">The object on which to invoke the method.</param>
    /// <param name="name">The name of the method to invoke.</param>
    /// <param name="arguments">The arguments to pass to the method.</param>
    /// <returns>The return value of the invoked method, or <c>null</c> if the method returns void.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the method is not found in the target object's type.</exception>
    /// <remarks>
    /// This method uses the target object's type to locate and invoke the method. The arguments are used
    /// for method overload resolution.
    /// </remarks>
    public static object? InvokeMethod(object target, string name, params object?[] arguments)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        var rootType = target.GetType();

        return InvokeMethod(rootType, target, name, arguments);
    }

    /// <summary>
    /// Invokes a method on the specified type with the given target, method name, and arguments.
    /// </summary>
    /// <param name="type">The type containing the method to invoke.</param>
    /// <param name="target">The object on which to invoke the method, or <c>null</c> for static methods.</param>
    /// <param name="name">The name of the method to invoke.</param>
    /// <param name="arguments">The arguments to pass to the method.</param>
    /// <returns>The return value of the invoked method, or <c>null</c> if the method returns void.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="name"/> is <c>null</c> or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the method is not found in the specified type.</exception>
    /// <remarks>
    /// This method allows invoking both instance and static methods. For static methods, pass <c>null</c> 
    /// as the target parameter. The arguments are used for method overload resolution.
    /// </remarks>
    public static object? InvokeMethod(Type type, object? target, string name, params object?[] arguments)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        // Convert arguments to their types for method resolution
        var argumentTypes = arguments?.Select(arg => arg?.GetType() ?? typeof(object)) ?? [];
        var methodAccessor = FindMethod(type, name, argumentTypes);

        if (methodAccessor is null)
            throw new InvalidOperationException($"Could not find method '{name}' in type '{type.Name}'.");

        return methodAccessor.Invoke(target, arguments!);
    }
}
