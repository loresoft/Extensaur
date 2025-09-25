#pragma warning disable IDE0130 // Namespace does not match folder structure

#nullable enable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Reflection;

/// <summary>
/// A <see langword="base"/> class for member accessors that provides common functionality for accessing member metadata and database mapping information.
/// </summary>
/// <remarks>
/// This abstract class implements the <see cref="IMemberAccessor"/> interface and provides lazy-loaded access to
/// Data Annotations attributes for database mapping scenarios. Derived classes must implement the specific
/// member access logic for properties, fields, or other member types.
/// </remarks>
[ExcludeFromCodeCoverage]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
#if PUBLIC_EXTENSIONS
public
#endif
abstract class MemberAccessor : IMemberAccessor, IEquatable<IMemberAccessor?>
{
    /// <summary>
    /// Lazy-loaded <see cref="ColumnAttribute"/> for database column mapping information.
    /// </summary>
    private readonly Lazy<ColumnAttribute?> _columnAttribute;

    /// <summary>
    /// Lazy-loaded <see cref="KeyAttribute"/> for primary key identification.
    /// </summary>
    private readonly Lazy<KeyAttribute?> _keyAttribute;

    /// <summary>
    /// Lazy-loaded <see cref="NotMappedAttribute"/> for excluding members from database mapping.
    /// </summary>
    private readonly Lazy<NotMappedAttribute?> _notMappedAttribute;

    /// <summary>
    /// Lazy-loaded <see cref="DatabaseGeneratedAttribute"/> for database-generated value identification.
    /// </summary>
    private readonly Lazy<DatabaseGeneratedAttribute?> _databaseGeneratedAttribute;

    /// <summary>
    /// Lazy-loaded <see cref="ConcurrencyCheckAttribute"/> for optimistic concurrency control.
    /// </summary>
    private readonly Lazy<ConcurrencyCheckAttribute?> _concurrencyCheckAttribute;

    /// <summary>
    /// Lazy-loaded <see cref="ForeignKeyAttribute"/> for foreign key relationship mapping.
    /// </summary>
    private readonly Lazy<ForeignKeyAttribute?> _foreignKeyAttribute;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberAccessor"/> class.
    /// </summary>
    /// <param name="memberInfo">The <see cref="System.Reflection.MemberInfo"/> instance that describes the member.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="memberInfo"/> is <see langword="null"/>.</exception>
    protected MemberAccessor(MemberInfo memberInfo)
    {
        MemberInfo = memberInfo ?? throw new ArgumentNullException(nameof(memberInfo));

        _columnAttribute = new Lazy<ColumnAttribute?>(() => MemberInfo.GetCustomAttribute<ColumnAttribute>(true));
        _keyAttribute = new Lazy<KeyAttribute?>(() => MemberInfo.GetCustomAttribute<KeyAttribute>(true));
        _notMappedAttribute = new Lazy<NotMappedAttribute?>(() => MemberInfo.GetCustomAttribute<NotMappedAttribute>(true));
        _databaseGeneratedAttribute = new Lazy<DatabaseGeneratedAttribute?>(() => MemberInfo.GetCustomAttribute<DatabaseGeneratedAttribute>(true));
        _concurrencyCheckAttribute = new Lazy<ConcurrencyCheckAttribute?>(() => MemberInfo.GetCustomAttribute<ConcurrencyCheckAttribute>(true));
        _foreignKeyAttribute = new Lazy<ForeignKeyAttribute?>(() => MemberInfo.GetCustomAttribute<ForeignKeyAttribute>(true));
    }

    /// <summary>
    /// Gets the <see cref="Type"/> of the member.
    /// </summary>
    /// <value>The <see cref="Type"/> of the member, such as the property type or field type.</value>
    public abstract Type MemberType { get; }

    /// <summary>
    /// Gets the <see cref="System.Reflection.MemberInfo"/> that describes this member.
    /// </summary>
    /// <value>The <see cref="System.Reflection.MemberInfo"/> instance containing reflection metadata for this member.</value>
    public MemberInfo MemberInfo { get; }

    /// <summary>
    /// Gets the name of the member.
    /// </summary>
    /// <value>The name of the member as defined in the source code.</value>
    public abstract string Name { get; }

    /// <summary>
    /// Gets a value indicating whether this member has a getter method or accessor.
    /// </summary>
    /// <value><see langword="true"/> if this member has a getter; otherwise, <see langword="false"/>.</value>
    /// <remarks>
    /// This indicates whether the member's value can be read. For properties, this corresponds to having a get accessor.
    /// For fields, this is typically always <see langword="true"/> unless the field is inaccessible.
    /// </remarks>
    public abstract bool HasGetter { get; }

    /// <summary>
    /// Gets a value indicating whether this member has a setter method or accessor.
    /// </summary>
    /// <value><see langword="true"/> if this member has a setter; otherwise, <see langword="false"/>.</value>
    /// <remarks>
    /// This indicates whether the member's value can be modified. For properties, this corresponds to having a set accessor.
    /// For fields, this is <see langword="false"/> for readonly or const fields.
    /// </remarks>
    public abstract bool HasSetter { get; }

    /// <summary>
    /// Gets the database column name that this member is mapped to.
    /// </summary>
    /// <value>
    /// The database column name that this member is mapped to. This value comes from the <see cref="ColumnAttribute.Name"/> property.
    /// If no <see cref="ColumnAttribute"/> is present, returns the member name.
    /// </value>
    public string Column => _columnAttribute.Value?.Name ?? Name;

    /// <summary>
    /// Gets the database provider specific data type of the column this member is mapped to.
    /// </summary>
    /// <value>
    /// The database provider specific data type of the column this member is mapped to,
    /// such as "varchar(50)", "int", or "decimal(18,2)". This value comes from the <see cref="ColumnAttribute.TypeName"/> property.
    /// Returns <see langword="null"/> if no <see cref="ColumnAttribute"/> is present or if <see cref="ColumnAttribute.TypeName"/> is not specified.
    /// </value>
    public string? ColumnType => _columnAttribute.Value?.TypeName;

    /// <summary>
    /// Gets the zero-based order of the column this member is mapped to.
    /// </summary>
    /// <value>
    /// The zero-based order of the column this member is mapped to, used for controlling column ordering
    /// in database schema generation. This value comes from the <see cref="ColumnAttribute.Order"/> property.
    /// Returns <see langword="null"/> if no <see cref="ColumnAttribute"/> is present or if <see cref="ColumnAttribute.Order"/> is not specified.
    /// </value>
    public int? ColumnOrder => _columnAttribute.Value?.Order;

    /// <summary>
    /// Gets a value indicating whether this member represents the unique identifier for the entity.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this member is a primary key; otherwise, <see langword="false"/>.
    /// This value is determined by the presence of the <see cref="KeyAttribute"/>.
    /// </value>
    /// <remarks>
    /// Primary key members are typically used as unique identifiers in database tables and are often
    /// involved in relationship mapping and entity tracking scenarios.
    /// </remarks>
    public bool IsKey => _keyAttribute.Value != null;

    /// <summary>
    /// Gets a value indicating whether this member should be excluded from database mapping.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this member should be excluded from database mapping; otherwise, <see langword="false"/>.
    /// This value is determined by the presence of the <see cref="NotMappedAttribute"/>.
    /// </value>
    /// <remarks>
    /// Members marked as not mapped are typically computed properties, navigation properties,
    /// or properties that should not be persisted to the database.
    /// </remarks>
    public bool IsNotMapped => _notMappedAttribute.Value != null;

    /// <summary>
    /// Gets a value indicating whether this member participates in optimistic concurrency checking.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this member participates in optimistic concurrency checking; otherwise, <see langword="false"/>.
    /// This value is determined by the presence of the <see cref="ConcurrencyCheckAttribute"/>.
    /// </value>
    /// <remarks>
    /// Concurrency check members are used to detect concurrent modifications to the same entity.
    /// Common examples include timestamp/rowversion columns or version number properties.
    /// </remarks>
    public bool IsConcurrencyCheck => _concurrencyCheckAttribute.Value != null;

    /// <summary>
    /// Gets a value indicating whether this member's value is generated by the database.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this member's value is database generated; otherwise, <see langword="false"/>.
    /// This value is determined by the presence of the <see cref="DatabaseGeneratedAttribute"/>
    /// with a <see cref="DatabaseGeneratedAttribute.DatabaseGeneratedOption"/> 
    /// other than <see cref="DatabaseGeneratedOption.None"/>.
    /// </value>
    /// <remarks>
    /// Database generated members include identity columns, computed columns, and columns with default values.
    /// These members typically should not be included in INSERT statements.
    /// </remarks>
    public bool IsDatabaseGenerated => _databaseGeneratedAttribute.Value != null
        && _databaseGeneratedAttribute.Value.DatabaseGeneratedOption != DatabaseGeneratedOption.None;

    /// <summary>
    /// Gets the name of the associated navigation property or foreign key(s) for this member.
    /// </summary>
    /// <value>
    /// The name of the associated navigation property or foreign key(s), or <see langword="null"/> if this member
    /// is not associated with a foreign key relationship. This value comes from the <see cref="ForeignKeyAttribute.Name"/> property.
    /// Returns <see langword="null"/> if no <see cref="ForeignKeyAttribute"/> is present.
    /// </value>
    /// <remarks>
    /// This property is used to establish relationships between entities in ORM scenarios.
    /// It can reference either the navigation property name or the foreign key column name(s).
    /// </remarks>
    public string? ForeignKey => _foreignKeyAttribute.Value?.Name;

    /// <summary>
    /// Returns the value of the member for the specified instance.
    /// </summary>
    /// <param name="instance">The instance whose member value will be returned. Can be <see langword="null"/> for static members.</param>
    /// <returns>The member value for the instance parameter, or <see langword="null"/> if the member value is <see langword="null"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the member does not support getting values.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance type is incompatible with the member's declaring type.</exception>
    public abstract object? GetValue(object? instance);

    /// <summary>
    /// Sets the value of the member for the specified instance.
    /// </summary>
    /// <param name="instance">The instance whose member value will be set. Can be <see langword="null"/> for static members.</param>
    /// <param name="value">The new value for this member. The value should be compatible with the member's type.</param>
    /// <exception cref="InvalidOperationException">Thrown when the member does not support setting values (e.g., read-only properties or fields).</exception>
    /// <exception cref="ArgumentException">Thrown when the instance type is incompatible with the member's declaring type, or when the value type is incompatible with the member type.</exception>
    public abstract void SetValue(object? instance, object? value);

    /// <summary>
    /// Determines whether the specified <see cref="IMemberAccessor"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="IMemberAccessor"/> to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="IMemberAccessor"/> is equal to this instance; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Two <see cref="IMemberAccessor"/> instances are considered equal if they reference the same <see cref="System.Reflection.MemberInfo"/>.
    /// </remarks>
    public bool Equals(IMemberAccessor? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Equals(other.MemberInfo, MemberInfo);
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="object"/> is equal to this instance; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Two <see cref="MemberAccessor"/> instances are considered equal if they reference the same <see cref="System.Reflection.MemberInfo"/>.
    /// </remarks>
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj is not MemberAccessor other)
            return false;

        return Equals(other.MemberInfo, MemberInfo);
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
    /// </returns>
    /// <remarks>
    /// The hash code is based on the underlying <see cref="System.Reflection.MemberInfo"/> to ensure consistency with equality comparison.
    /// </remarks>
    public override int GetHashCode()
    {
        return MemberInfo.GetHashCode();
    }

    /// <summary>
    /// Gets a string representation of the <see cref="MemberAccessor"/> for debugging purposes.
    /// </summary>
    private string DebuggerDisplay => $"Name: {Name}";
}
