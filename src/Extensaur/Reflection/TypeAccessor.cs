#pragma warning disable IDE0130 // Namespace does not match folder structure

#nullable enable

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;

namespace System.Reflection;

/// <summary>
/// Provides high-performance reflection-based access to type members including properties, fields, and methods through compiled expressions.
/// </summary>
/// <remarks>
/// This class serves as a factory and cache for member accessors, providing efficient access to type reflection information
/// and database mapping metadata. It uses lazy initialization and caching to optimize performance for repeated member access operations.
/// The class supports Entity Framework-style data annotations for ORM scenarios.
/// </remarks>
public class TypeAccessor
{
    private static readonly ConcurrentDictionary<Type, TypeAccessor> _typeCache = new();
    private readonly ConcurrentDictionary<string, IMemberAccessor?> _memberCache = new();
    private readonly ConcurrentDictionary<int, IMethodAccessor?> _methodCache = new();
    private readonly ConcurrentDictionary<int, IEnumerable<IMemberAccessor>> _propertyCache = new();

    private readonly Lazy<Func<object?>?> _constructor;
    private readonly Lazy<TableAttribute?> _tableAttribute;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeAccessor"/> class.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> this accessor is for.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
    public TypeAccessor(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        Type = type;
        _constructor = new Lazy<Func<object?>?>(() => ExpressionFactory.CreateConstructor(Type));
        _tableAttribute = new Lazy<TableAttribute?>(() => type.GetCustomAttribute<TableAttribute>(true));
    }

    /// <summary>
    /// Gets the <see cref="Type"/> this accessor is for.
    /// </summary>
    /// <value>The <see cref="Type"/> this accessor is for.</value>
    public Type Type { get; }

    /// <summary>
    /// Gets the name of the Type.
    /// </summary>
    /// <value>The name of the Type as returned by <see cref="Type.Name"/>.</value>
    public string Name => Type.Name;

    /// <summary>
    /// Gets the name of the table the class is mapped to for database operations.
    /// </summary>
    /// <value>
    /// The name of the table the class is mapped to. If a <see cref="TableAttribute"/> is present,
    /// returns <see cref="TableAttribute.Name"/>; otherwise, returns <see cref="Type.Name"/>.
    /// </value>
    /// <remarks>
    /// This property is commonly used in ORM scenarios to determine the database table name
    /// associated with the entity type.
    /// </remarks>
    public string TableName => _tableAttribute.Value?.Name ?? Type.Name;

    /// <summary>
    /// Gets the schema of the table the class is mapped to for database operations.
    /// </summary>
    /// <value>
    /// The schema of the table the class is mapped to, or <see langword="null"/> if no schema is specified.
    /// This value comes from <see cref="TableAttribute.Schema"/> if a <see cref="TableAttribute"/> is present.
    /// </value>
    /// <remarks>
    /// This property is commonly used in ORM scenarios to determine the database schema name
    /// associated with the entity type.
    /// </remarks>
    public string? TableSchema => _tableAttribute.Value?.Schema;

    /// <summary>
    /// Creates a new instance of the accessor's type using the default parameterless constructor.
    /// </summary>
    /// <returns>A new instance of the accessor's type.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no suitable constructor is found for the type or the constructor could not be compiled.
    /// </exception>
    /// <remarks>
    /// This method uses compiled expressions for high-performance object creation. The constructor
    /// delegate is lazy-loaded and cached for subsequent calls.
    /// </remarks>
    public object? Create()
    {
        var constructor = _constructor.Value;
        if (constructor == null)
            throw new InvalidOperationException($"Could not find constructor for '{Type.Name}'.");

        return constructor.Invoke();
    }


    #region Method
    /// <summary>
    /// Finds the parameterless method with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name of the method to find.</param>
    /// <returns>An <see cref="IMethodAccessor"/> instance for the method if found; otherwise <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method searches for public, static, and instance methods by default.
    /// Use <see cref="FindMethod(string, Type[], BindingFlags)"/> for more control over the search criteria.
    /// </remarks>
    public IMethodAccessor? FindMethod(string name)
    {
        return FindMethod(name, Type.EmptyTypes);
    }

    /// <summary>
    /// Finds the method with the specified <paramref name="name"/> and parameter types.
    /// </summary>
    /// <param name="name">The name of the method to find.</param>
    /// <param name="parameterTypes">An array of <see cref="Type"/> objects representing the method parameter types.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>An <see cref="IMethodAccessor"/> instance for the method if found; otherwise <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="parameterTypes"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method uses intelligent matching when an exact signature match is not found. It will attempt to find
    /// the best matching method by parameter count and type compatibility. The method result is cached for performance.
    /// </remarks>
    public IMethodAccessor? FindMethod(
        string name,
        Type[] parameterTypes,
        BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
    {
        int key = MethodAccessor.GetKey(name, parameterTypes);
        return _methodCache.GetOrAdd(key, _ => CreateMethodAccessor(name, parameterTypes, flags));
    }


    private MethodAccessor? CreateMethodAccessor(string name, Type[] parameters, BindingFlags flags)
    {
        var info = FindMethod(Type, name, parameters, flags);
        if (info == null)
            return null;

        return new MethodAccessor(info);
    }

    private static MethodInfo? FindMethod(Type type, string name, Type[]? parameterTypes, BindingFlags flags)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        if (parameterTypes == null)
            parameterTypes = Type.EmptyTypes;

        var typeInfo = type.GetTypeInfo();

        //first try full match
        var methodInfo = typeInfo.GetMethod(name, parameterTypes);
        if (methodInfo != null)
            return methodInfo;

        // next, get all that match by name
        var methodsByName = typeInfo.GetMethods(flags)
          .Where(m => m.Name == name)
          .ToList();

        if (methodsByName.Count == 0)
            return null;

        // if only one matches name, return it
        if (methodsByName.Count == 1)
            return methodsByName.FirstOrDefault();

        // next, get all methods that match param count
        var methodsByParamCount = methodsByName
            .Where(m => m.GetParameters().Length == parameterTypes.Length)
            .ToList();

        // if only one matches with same param count, return it
        if (methodsByParamCount.Count == 1)
            return methodsByParamCount.FirstOrDefault();

        // still no match, make best guess by greatest matching param types
        var current = methodsByParamCount.FirstOrDefault();
        int matchCount = 0;

        foreach (var info in methodsByParamCount)
        {
            var paramTypes = info.GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();

            // unsure which way IsAssignableFrom should be checked?
            int count = paramTypes
                .Select(t => t.GetTypeInfo())
                .Where((t, i) => t.IsAssignableFrom(parameterTypes[i]))
                .Count();

            if (count <= matchCount)
                continue;

            current = info;
            matchCount = count;
        }

        return current;
    }

    #endregion

    #region Find
    /// <summary>
    /// Searches for the public property or field with the specified name.
    /// </summary>
    /// <param name="name">The name of the property or field to find.</param>
    /// <returns>An <see cref="IMemberAccessor"/> instance for the property or field if found; otherwise <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method searches for public instance members with hierarchy flattening enabled.
    /// Properties are searched first, then fields. The search is case-sensitive.
    /// </remarks>
    public IMemberAccessor? Find(string name)
    {
        return Find(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
    }

    /// <summary>
    /// Searches for the specified property or field, using the specified binding constraints.
    /// </summary>
    /// <param name="name">The name of the property or field to find.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>An <see cref="IMemberAccessor"/> instance for the property or field if found; otherwise <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method searches for properties first, then fields. The search supports case-insensitive matching
    /// as a fallback if an exact case match is not found. The result is cached for performance.
    /// </remarks>
    public IMemberAccessor? Find(string name, BindingFlags flags)
    {
        return _memberCache.GetOrAdd(name, n => CreateAccessor(n, flags));
    }

    private IMemberAccessor? CreateAccessor(string name, BindingFlags flags)
    {
        // first try property
        var property = FindProperty(Type, name, flags);
        if (property != null)
            return CreatePropertyAccessor(property);

        // next try field
        var field = FindField(Type, name, flags);
        if (field != null)
            return CreateFieldAccessor(field);

        return null;
    }
    #endregion

    #region Column
    /// <summary>
    /// Searches for the public property with the specified database column name.
    /// </summary>
    /// <param name="name">The database column name to search for.</param>
    /// <returns>An <see cref="IMemberAccessor"/> instance for the property if found; otherwise <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method searches for properties that are mapped to the specified database column using the
    /// <see cref="ColumnAttribute"/>. If no attribute match is found, it falls back to property name matching.
    /// The search is case-insensitive and targets public instance members with hierarchy flattening.
    /// </remarks>
    public IMemberAccessor? FindColumn(string name)
    {
        return FindColumn(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
    }

    /// <summary>
    /// Searches for the property with the specified database column name using the specified binding constraints.
    /// </summary>
    /// <param name="name">The database column name to search for.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>An <see cref="IMemberAccessor"/> instance for the property if found; otherwise <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method first checks for properties with a <see cref="ColumnAttribute"/> whose <see cref="ColumnAttribute.Name"/>
    /// matches the specified column name. If no attribute match is found, it searches by property name.
    /// All comparisons are case-insensitive. The result is cached for performance.
    /// </remarks>
    public IMemberAccessor? FindColumn(string name, BindingFlags flags)
    {
        return _memberCache.GetOrAdd(name, n => CreateColumnAccessor(n, flags));
    }

    private PropertyAccessor? CreateColumnAccessor(string name, BindingFlags flags)
    {
        var typeInfo = Type.GetTypeInfo();

        foreach (var p in typeInfo.GetProperties(flags))
        {
            // try ColumnAttribute
            var columnAttribute = p.GetCustomAttribute<ColumnAttribute>();
            if (columnAttribute != null && name.Equals(columnAttribute.Name, StringComparison.OrdinalIgnoreCase))
                return CreatePropertyAccessor(p);

            if (p.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return CreatePropertyAccessor(p);
        }

        return null;
    }
    #endregion

    #region Property
    /// <summary>
    /// Searches for the property using a property expression.
    /// </summary>
    /// <typeparam name="T">The object type containing the property specified in the expression.</typeparam>
    /// <param name="propertyExpression">The property expression (e.g., () => someProperty).</param>
    /// <returns>An <see cref="IMemberAccessor"/> instance for the property if found; otherwise <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyExpression"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the expression is not a <see cref="MemberExpression"/>, the <see cref="MemberExpression"/>
    /// does not represent a property, or the property is static.
    /// </exception>
    /// <remarks>
    /// This method extracts the property name from the lambda expression and performs a standard property search.
    /// Both direct member access and unary expressions (for value types) are supported.
    /// </remarks>
    public IMemberAccessor? FindProperty<T>(Expression<Func<T>> propertyExpression)
    {
        if (propertyExpression == null)
            throw new ArgumentNullException(nameof(propertyExpression));

        if (propertyExpression.Body is UnaryExpression unaryExpression)
            return FindProperty(unaryExpression.Operand as MemberExpression);
        else
            return FindProperty(propertyExpression.Body as MemberExpression);
    }

    /// <summary>
    /// Searches for the property using a property expression.
    /// </summary>
    /// <typeparam name="TSource">The object type containing the property specified in the expression.</typeparam>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="propertyExpression">The property expression (e.g., p => p.PropertyName).</param>
    /// <returns>An <see cref="IMemberAccessor"/> instance for the property if found; otherwise <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyExpression"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the expression is not a <see cref="MemberExpression"/>, the <see cref="MemberExpression"/>
    /// does not represent a property, or the property is static.
    /// </exception>
    /// <remarks>
    /// This method extracts the property name from the lambda expression and performs a standard property search.
    /// Both direct member access and unary expressions (for value types) are supported.
    /// </remarks>
    public IMemberAccessor? FindProperty<TSource, TValue>(Expression<Func<TSource, TValue>> propertyExpression)
    {
        if (propertyExpression == null)
            throw new ArgumentNullException(nameof(propertyExpression));

        if (propertyExpression.Body is UnaryExpression unaryExpression)
            return FindProperty(unaryExpression.Operand as MemberExpression);
        else
            return FindProperty(propertyExpression.Body as MemberExpression);
    }

    /// <summary>
    /// Searches for the public property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property to find.</param>
    /// <returns>An <see cref="IMemberAccessor"/> instance for the property if found; otherwise <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method searches for public instance properties with hierarchy flattening enabled.
    /// The search supports case-insensitive matching as a fallback if an exact case match is not found.
    /// </remarks>
    public IMemberAccessor? FindProperty(string name)
    {
        return FindProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
    }

    /// <summary>
    /// Searches for the specified property, using the specified binding constraints.
    /// </summary>
    /// <param name="name">The name of the property to find.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>An <see cref="IMemberAccessor"/> instance for the property if found; otherwise <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method first attempts an exact case-sensitive match, then falls back to case-insensitive matching
    /// if no exact match is found. The result is cached for performance.
    /// </remarks>
    public IMemberAccessor? FindProperty(string name, BindingFlags flags)
    {
        return _memberCache.GetOrAdd(name, n => CreatePropertyAccessor(n, flags));
    }

    /// <summary>
    /// Gets all property member accessors for the Type.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{IMemberAccessor}"/> of property accessors for the Type.
    /// </returns>
    /// <remarks>
    /// This method returns accessors for public, static, and instance properties.
    /// The results are cached for performance on subsequent calls.
    /// </remarks>
    public IEnumerable<IMemberAccessor> GetProperties()
    {
        return GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
    }

    /// <summary>
    /// Gets the property member accessors for the Type using the specified binding flags.
    /// </summary>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify which properties to include.</param>
    /// <returns>
    /// An <see cref="IEnumerable{IMemberAccessor}"/> of property accessors for the Type matching the specified flags.
    /// </returns>
    /// <remarks>
    /// This method reuses cached property accessors when available and returns an empty collection if no properties match the criteria.
    /// The results are cached based on the binding flags for performance on subsequent calls.
    /// </remarks>
    public IEnumerable<IMemberAccessor> GetProperties(BindingFlags flags)
    {
        return _propertyCache.GetOrAdd((int)flags, _ =>
        {
            var typeInfo = Type.GetTypeInfo();
            var properties = typeInfo.GetProperties(flags);
            if (properties.Length == 0)
                return [];

            // don't convert to linq
            var list = new IMemberAccessor[properties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];

                // Reuse cached property accessors
                list[i] = _memberCache.GetOrAdd(property.Name, __ => CreatePropertyAccessor(property))!;
            }

            return list;
        });
    }

    private PropertyAccessor? CreatePropertyAccessor(string name, BindingFlags flags)
    {
        var info = FindProperty(Type, name, flags);
        return info == null ? null : CreatePropertyAccessor(info);
    }

    private IMemberAccessor? FindProperty(MemberExpression? memberExpression)
    {
        if (memberExpression == null)
            throw new ArgumentException("The expression is not a member access expression.", nameof(memberExpression));

        var property = memberExpression.Member as PropertyInfo;
        if (property == null)
            throw new ArgumentException("The member access expression does not access a property.", nameof(memberExpression));

        // find by name because we can't trust the PropertyInfo here as it could be from an interface or inherited class
        return FindProperty(property.Name);
    }

    private static PropertyInfo? FindProperty(Type type, string name, BindingFlags flags)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (name == null)
            throw new ArgumentNullException(nameof(name));

        var typeInfo = type.GetTypeInfo();

        // first try GetProperty
        var property = typeInfo.GetProperty(name, flags);
        if (property != null)
            return property;

        // if not found, search while ignoring case
        property = typeInfo
            .GetProperties(flags)
            .FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        return property;
    }

    private static PropertyAccessor? CreatePropertyAccessor(PropertyInfo? propertyInfo)
    {
        return propertyInfo == null ? null : new PropertyAccessor(propertyInfo);
    }
    #endregion

    #region Field
    /// <summary>
    /// Searches for the field with the specified name.
    /// </summary>
    /// <param name="name">The name of the field to find.</param>
    /// <returns>An <see cref="IMemberAccessor"/> instance for the field if found; otherwise <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method searches for both public and non-public instance fields by default.
    /// The search supports case-insensitive matching as a fallback if an exact case match is not found.
    /// </remarks>
    public IMemberAccessor? FindField(string name)
    {
        return FindField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }

    /// <summary>
    /// Searches for the specified field, using the specified binding constraints.
    /// </summary>
    /// <param name="name">The name of the field to find.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>An <see cref="IMemberAccessor"/> instance for the field if found; otherwise <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method first attempts an exact case-sensitive match, then falls back to case-insensitive matching
    /// if no exact match is found. The result is cached for performance.
    /// </remarks>
    public IMemberAccessor? FindField(string name, BindingFlags flags)
    {
        return _memberCache.GetOrAdd(name, n => CreateFieldAccessor(n, flags));
    }

    private FieldAccessor? CreateFieldAccessor(string name, BindingFlags flags)
    {
        var info = FindField(Type, name, flags);
        return info == null ? null : CreateFieldAccessor(info);
    }

    private static FieldInfo? FindField(Type type, string name, BindingFlags flags)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (name == null)
            throw new ArgumentNullException(nameof(name));

        // first try GetField
        var typeInfo = type.GetTypeInfo();
        var field = typeInfo.GetField(name, flags);
        if (field != null)
            return field;

        // if not found, search while ignoring case
        return typeInfo
            .GetFields(flags)
            .FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private static FieldAccessor? CreateFieldAccessor(FieldInfo? fieldInfo)
    {
        return fieldInfo == null ? null : new FieldAccessor(fieldInfo);
    }
    #endregion


    /// <summary>
    /// Gets the <see cref="TypeAccessor"/> for the specified generic type.
    /// </summary>
    /// <typeparam name="T">The type to get the accessor for.</typeparam>
    /// <returns>A <see cref="TypeAccessor"/> instance for the specified type.</returns>
    /// <remarks>
    /// This method provides a strongly-typed way to get a type accessor. The result is cached
    /// in a static dictionary for optimal performance on repeated calls.
    /// </remarks>
    public static TypeAccessor GetAccessor<T>()
    {
        return GetAccessor(typeof(T));
    }

    /// <summary>
    /// Gets the <see cref="TypeAccessor"/> for the specified type.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to get the accessor for.</param>
    /// <returns>A <see cref="TypeAccessor"/> instance for the specified type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method uses a static cache to ensure only one <see cref="TypeAccessor"/> instance exists per type,
    /// providing optimal performance for repeated access operations.
    /// </remarks>
    public static TypeAccessor GetAccessor(Type type)
    {
        return _typeCache.GetOrAdd(type, t => new TypeAccessor(t));
    }
}
