#pragma warning disable IDE0130 // Namespace does not match folder structure

#nullable enable

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Reflection;

/// <summary>
/// An accessor class for <see cref="FieldInfo"/> that provides late binding access to field members.
/// </summary>
[ExcludeFromCodeCoverage]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
#if PUBLIC_EXTENSIONS
public
#endif
class FieldAccessor : MemberAccessor
{
    /// <summary>
    /// A lazy-initialized getter function for retrieving the field value.
    /// </summary>
    private readonly Lazy<Func<object?, object?>?> _getter;
    
    /// <summary>
    /// A lazy-initialized setter action for setting the field value.
    /// </summary>
    private readonly Lazy<Action<object?, object?>?> _setter;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldAccessor"/> class.
    /// </summary>
    /// <param name="memberInfo">The <see cref="FieldInfo"/> instance to use for this accessor.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="memberInfo"/> is <see langword="null"/>.</exception>
    public FieldAccessor(FieldInfo memberInfo) : base(memberInfo)
    {
        if (memberInfo == null)
            throw new ArgumentNullException(nameof(memberInfo));

        Name = memberInfo.Name;
        MemberType = memberInfo.FieldType;

        _getter = new(() => ExpressionFactory.CreateGet(memberInfo));
        HasGetter = true;

        bool isReadonly = memberInfo.IsInitOnly || memberInfo.IsLiteral;
        if (!isReadonly)
            _setter = new(() => ExpressionFactory.CreateSet(memberInfo));
        else
            _setter = new(() => null);

        HasSetter = !isReadonly;
    }

    /// <summary>
    /// Gets the <see cref="Type"/> of the field.
    /// </summary>
    /// <value>The <see cref="Type"/> of the field.</value>
    public override Type MemberType { get; }

    /// <summary>
    /// Gets the name of the field.
    /// </summary>
    /// <value>The name of the field.</value>
    public override string Name { get; }

    /// <summary>
    /// Gets a value indicating whether this field has a getter.
    /// </summary>
    /// <value><see langword="true"/> if this field has a getter; otherwise, <see langword="false"/>.</value>
    /// <remarks>
    /// Field accessors always have getters unless the field is inaccessible.
    /// </remarks>
    public override bool HasGetter { get; }

    /// <summary>
    /// Gets a value indicating whether this field has a setter.
    /// </summary>
    /// <value><see langword="true"/> if this field has a setter; otherwise, <see langword="false"/>.</value>
    /// <remarks>
    /// Fields do not have setters if they are read-only (marked with <see langword="readonly"/> or <see langword="const"/>).
    /// </remarks>
    public override bool HasSetter { get; }


    /// <summary>
    /// Returns the value of the field for the specified instance.
    /// </summary>
    /// <param name="instance">The object whose field value will be returned. Can be <see langword="null"/> for static fields.</param>
    /// <returns>
    /// The field value for the instance parameter.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the field does not have a getter or the getter could not be created.
    /// </exception>
    public override object? GetValue(object? instance)
    {
        if (_getter == null || !HasGetter)
            throw new InvalidOperationException($"Field '{Name}' does not have a getter.");

        var get = _getter.Value;
        if (get == null)
            throw new InvalidOperationException($"Field '{Name}' does not have a getter.");

        return get(instance);
    }

    /// <summary>
    /// Sets the value of the field for the specified instance.
    /// </summary>
    /// <param name="instance">The object whose field value will be set. Can be <see langword="null"/> for static fields.</param>
    /// <param name="value">The new value for this field.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the field does not have a setter (i.e., it's read-only) or the setter could not be created.
    /// </exception>
    public override void SetValue(object? instance, object? value)
    {
        if (_setter == null || !HasSetter)
            throw new InvalidOperationException($"Field '{Name}' does not have a setter.");

        var set = _setter.Value;
        if (set == null)
            throw new InvalidOperationException($"Field '{Name}' does not have a setter.");

        set(instance, value);
    }

    /// <summary>
    /// Gets a string representation of the <see cref="FieldAccessor"/> for debugging purposes.
    /// </summary>
    private string DebuggerDisplay => $"Name: {Name}";
}
