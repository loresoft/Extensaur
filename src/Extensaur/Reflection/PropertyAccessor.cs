#pragma warning disable IDE0130 // Namespace does not match folder structure

#nullable enable

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Reflection;

/// <summary>
/// An accessor class for <see cref="PropertyInfo"/> that provides high-performance property access through compiled expressions.
/// </summary>
/// <remarks>
/// This class extends <see cref="MemberAccessor"/> to provide specialized functionality for property access.
/// It uses lazy-loaded compiled expressions to provide fast property getting and setting while maintaining
/// the flexibility of reflection. The compiled expressions are created using the <see cref="ExpressionFactory"/>
/// which generates optimized delegates for property access operations.
/// </remarks>
[ExcludeFromCodeCoverage]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
#if PUBLIC_EXTENSIONS
public
#endif
class PropertyAccessor : MemberAccessor
{
    /// <summary>
    /// A lazy-initialized getter function for retrieving the property value with high performance.
    /// </summary>
    private readonly Lazy<Func<object, object?>?> _getter;

    /// <summary>
    /// A lazy-initialized setter action for setting the property value with high performance.
    /// </summary>
    private readonly Lazy<Action<object, object?>?> _setter;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyAccessor"/> class.
    /// </summary>
    /// <param name="memberInfo">The <see cref="PropertyInfo"/> instance that describes the property to access.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="memberInfo"/> is <see langword="null"/>.</exception>
    public PropertyAccessor(PropertyInfo memberInfo) : base(memberInfo)
    {
        if (memberInfo == null)
            throw new ArgumentNullException(nameof(memberInfo));

        Name = memberInfo.Name;
        MemberType = memberInfo.PropertyType;

        HasGetter = memberInfo.CanRead;
        _getter = new Lazy<Func<object, object?>?>(() => ExpressionFactory.CreateGet(memberInfo));

        HasSetter = memberInfo.CanWrite;
        _setter = new Lazy<Action<object, object?>?>(() => ExpressionFactory.CreateSet(memberInfo));
    }

    /// <summary>
    /// Gets the <see cref="Type"/> of the property.
    /// </summary>
    /// <value>The <see cref="Type"/> of the property as defined by <see cref="PropertyInfo.PropertyType"/>.</value>
    public override Type MemberType { get; }

    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    /// <value>The name of the property as defined in the source code.</value>
    public override string Name { get; }

    /// <summary>
    /// Gets a value indicating whether this property has a getter accessor.
    /// </summary>
    /// <value><see langword="true"/> if this property has a getter; otherwise, <see langword="false"/>.</value>
    /// <remarks>
    /// This value is determined by the <see cref="PropertyInfo.CanRead"/> property, which indicates
    /// whether the property has a get accessor that can be invoked.
    /// </remarks>
    public override bool HasGetter { get; }

    /// <summary>
    /// Gets a value indicating whether this property has a setter accessor.
    /// </summary>
    /// <value><see langword="true"/> if this property has a setter; otherwise, <see langword="false"/>.</value>
    /// <remarks>
    /// This value is determined by the <see cref="PropertyInfo.CanWrite"/> property, which indicates
    /// whether the property has a set accessor that can be invoked. This includes init-only properties.
    /// </remarks>
    public override bool HasSetter { get; }

    /// <summary>
    /// Returns the value of the property for the specified instance.
    /// </summary>
    /// <param name="instance">The instance whose property value will be returned. Can be <see langword="null"/> for static properties.</param>
    /// <returns>
    /// The property value for the instance parameter, or <see langword="null"/> if the property value is <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the property does not have a getter or the getter could not be created.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the instance type is incompatible with the property's declaring type.
    /// </exception>
    /// <exception cref="TargetInvocationException">
    /// Thrown when the property getter throws an exception. The original exception can be found in the <see cref="Exception.InnerException"/> property.
    /// </exception>
    public override object? GetValue(object? instance)
    {
        if (_getter == null || !HasGetter)
            throw new InvalidOperationException($"Property '{Name}' does not have a getter.");

        var get = _getter.Value;
        if (get == null)
            throw new InvalidOperationException($"Property '{Name}' does not have a getter.");

        return get(instance!);
    }

    /// <summary>
    /// Sets the value of the property for the specified instance.
    /// </summary>
    /// <param name="instance">The instance whose property value will be set. Can be <see langword="null"/> for static properties.</param>
    /// <param name="value">The new value for this property. The value should be compatible with the property's type.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the property does not have a setter (i.e., it's read-only) or the setter could not be created.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the instance type is incompatible with the property's declaring type, or when the value type is incompatible with the property type.
    /// </exception>
    /// <exception cref="TargetInvocationException">
    /// Thrown when the property setter throws an exception. The original exception can be found in the <see cref="Exception.InnerException"/> property.
    /// </exception>
    public override void SetValue(object? instance, object? value)
    {
        if (_setter == null || !HasSetter)
            throw new InvalidOperationException($"Property '{Name}' does not have a setter.");

        var set = _setter.Value;
        if (set == null)
            throw new InvalidOperationException($"Property '{Name}' does not have a setter.");

        set(instance!, value);
    }

    /// <summary>
    /// Gets a string representation of the <see cref="PropertyAccessor"/> for debugging purposes.
    /// </summary>
    private string DebuggerDisplay => $"Name: {Name}";
}
