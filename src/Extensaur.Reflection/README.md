# Reflection Extensions

Source only package for Reflection - High-performance compiled expression-based reflection utilities for .NET

This package provides a comprehensive set of classes for high-performance reflection operations using compiled expressions instead of traditional reflection calls. The package is optimized for scenarios where reflection operations need to be performed repeatedly, such as ORM scenarios, object mapping, and dynamic property access.

## Key Features

- **High Performance**: Uses compiled expressions instead of traditional reflection for significantly better performance
- **Mapping Support**: Built-in support for Data Annotations attributes
- **Type Safety**: Strongly-typed interfaces and generic methods where possible  
- **Caching**: Intelligent caching of compiled expressions and member information
- **Late Binding**: Dynamic member access with runtime type resolution
- **Comprehensive Coverage**: Support for properties, fields, methods, and constructors

## Core Classes

### TypeAccessor

The main entry point for accessing type information and members. Provides high-performance reflection-based access to type members through compiled expressions.

```csharp
// Get a type accessor
var accessor = TypeAccessor.GetAccessor<Person>();
var accessor = TypeAccessor.GetAccessor(typeof(Person));

// Create instances
var person = accessor.Create();

// Access properties
var nameAccessor = accessor.FindProperty("Name");
nameAccessor.SetValue(person, "John Doe");
var name = nameAccessor.GetValue(person);

// Access by database column name
var columnAccessor = accessor.FindColumn("first_name");

// Get all properties
var properties = accessor.GetProperties();

// Find methods
var methodAccessor = accessor.FindMethod("ToString");
var result = methodAccessor.Invoke(person);
```

**Key Features:**

- Static caching of accessors per type for optimal performance
- Support for `TableAttribute` for database table mapping
- Expression-based property finding with compile-time safety
- Intelligent method overload resolution
- Case-insensitive member searching as fallback

### LateBinder

Static utility class providing simplified late-binding operations for dynamic member access.

```csharp
// Set property/field values
LateBinder.SetProperty(person, "Name", "John Doe");
LateBinder.SetField(person, "_id", 123);

// Tries property first, then field
LateBinder.Set(person, "Email", "john@example.com"); 

// Get property/field values  
var name = LateBinder.GetProperty(person, "Name");
var id = LateBinder.GetField(person, "_id");

// Tries property first, then field
var email = LateBinder.Get(person, "Email");

// Create instances
var instance = LateBinder.CreateInstance(typeof(Person));

// Invoke methods
var result = LateBinder.InvokeMethod(person, "ToString");

// Static method
var result = LateBinder.InvokeMethod(typeof(Math), null, "Abs", -5);

// Find members
var propertyAccessor = LateBinder.FindProperty(typeof(Person), "Name");
var methodAccessor = LateBinder.FindMethod(typeof(Person), "ToString");
```

**Key Features:**

- Unified API for common reflection operations
- Support for nested property access with dot notation
- Automatic type resolution and method overload matching
- Both public and non-public member access options
- Static and instance method support

### IMemberAccessor Interface

Provides a unified interface for accessing different types of members (properties and fields).

```csharp
public interface IMemberAccessor : IMemberInformation
{
    object? GetValue(object? instance);
    void SetValue(object? instance, object? value);
}
```

**Implementations:**

- `PropertyAccessor` - For property access
- `FieldAccessor` - For field access

### PropertyAccessor

Specialized accessor for properties with high-performance compiled expression-based access.

```csharp
var typeAccessor = TypeAccessor.GetAccessor<Person>();
var accessor = typeAccessor.FindProperty("Name");

// High-performance property access
accessor.SetValue(person, "John Doe");
var name = accessor.GetValue(person);
```

**Key Features:**

- Lazy-loaded compiled expressions for optimal performance
- Support for both readable and writable properties
- Handles static and instance properties
- Efficient null handling and type casting

### FieldAccessor

Specialized accessor for fields with high-performance compiled expression-based access.

```csharp
var typeAccessor = TypeAccessor.GetAccessor<Person>();
var accessor = typeAccessor.FindField("_name");

// High-performance field access
accessor.SetValue(person, "John Doe");
var name = accessor.GetValue(person);
```

**Key Features:**

- Handles readonly and const field restrictions
- Support for both public and private fields
- Efficient expression compilation and caching
- Static and instance field support

### IMethodAccessor Interface & MethodAccessor

High-performance method invocation through compiled expressions.

```csharp
public interface IMethodAccessor
{
    MethodInfo MethodInfo { get; }
    string Name { get; }
    object? Invoke(object? instance, params object?[] arguments);
}

var methodInfo = typeof(Person).GetMethod("ToString");
var accessor = new MethodAccessor(methodInfo);

// High-performance method invocation
var result = accessor.Invoke(person);
var result = accessor.Invoke(person, arg1, arg2); // With arguments
```

**Key Features:**

- Compiled expressions for fast method invocation
- Automatic parameter validation and type conversion
- Support for void and non-void return types
- Static and instance method support
- Intelligent method overload resolution

### IMemberInformation Interface

Comprehensive interface providing member metadata including database mapping information.

```csharp
public interface IMemberInformation
{
    Type MemberType { get; }
    MemberInfo MemberInfo { get; }
    string Name { get; }
    string Column { get; }
    string? ColumnType { get; }
    int? ColumnOrder { get; }
    bool IsKey { get; }
    bool IsNotMapped { get; }
    bool IsDatabaseGenerated { get; }
    bool IsConcurrencyCheck { get; }
    string? ForeignKey { get; }
    bool HasGetter { get; }
    bool HasSetter { get; }
}
```

**Supported Data Annotations:**

- `[Column]` - Database column mapping
- `[Key]` - Primary key identification  
- `[NotMapped]` - Exclude from database mapping
- `[DatabaseGenerated]` - Database-generated values
- `[ConcurrencyCheck]` - Optimistic concurrency control
- `[ForeignKey]` - Foreign key relationships
- `[Table]` - Table name and schema mapping

### MemberAccessor (Abstract Base Class)

Abstract base class providing common functionality for member accessors including lazy-loaded Data Annotations attribute access.

**Key Features:**

- Lazy-loaded attribute caching for performance
- Base implementation of `IMemberAccessor` and `IMemberInformation`
- Equality comparison based on underlying `MemberInfo`
- Debugger-friendly display attributes

### ExpressionFactory (Internal)

Internal factory class that creates compiled expressions for high-performance member access. This class handles the complex expression tree generation and compilation.

**Capabilities:**

- Method invocation delegates with parameter validation
- Property and field getter/setter delegates  
- Constructor delegates for object creation
- Safe type casting and null handling
- Optimized boxing for value types
- Parameter count validation with helpful error messages

## Usage Examples

### Basic Property Access

```csharp
// Get type accessor
var accessor = TypeAccessor.GetAccessor<Person>();

// Find and use property accessor  
var nameAccessor = accessor.FindProperty("Name");
nameAccessor.SetValue(person, "John Doe");
var name = nameAccessor.GetValue(person);
```

### Database Mapping Scenarios

```csharp
[Table("People", Schema = "dbo")]
public class Person
{
    [Key]
    public int Id { get; set; }
    
    [Column("first_name")]
    public string FirstName { get; set; }
    
    [Column("last_name")]  
    public string LastName { get; set; }
    
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
    
    [ConcurrencyCheck]
    public byte[] RowVersion { get; set; }
}

var accessor = TypeAccessor.GetAccessor<Person>();

// Access by database column name
var firstNameAccessor = accessor.FindColumn("first_name");
firstNameAccessor.SetValue(person, "John");

// Get table information
Console.WriteLine($"Table: {accessor.TableName}");    // "People"  
Console.WriteLine($"Schema: {accessor.TableSchema}"); // "dbo"

// Check member attributes
var properties = accessor.GetProperties();
foreach (var prop in properties)
{
    Console.WriteLine($"{prop.Name}");
}
```

### Expression-Based Property Access

```csharp
var accessor = TypeAccessor.GetAccessor<Person>();

// Type-safe property access using expressions
var nameAccessor = accessor.FindProperty<Person, string>(p => p.Name);
nameAccessor.SetValue(person, "John Doe");
```

### Method Invocation

```csharp  
var accessor = TypeAccessor.GetAccessor<Person>();

// Find and invoke method
var toStringMethod = accessor.FindMethod("ToString");
var result = toStringMethod.Invoke(person);

// Method with parameters
var equalsMethod = accessor.FindMethod("Equals", [typeof(object)]);  
var isEqual = equalsMethod.Invoke(person, otherPerson);
```

### Late Binding Operations

```csharp
// Simple property access
LateBinder.SetProperty(person, "Name", "John Doe");
var name = LateBinder.GetProperty(person, "Name");

// Unified member access (tries property first, then field)
LateBinder.Set(person, "Email", "john@example.com");
var email = LateBinder.Get(person, "Email");

// Method invocation with automatic overload resolution
var result = LateBinder.InvokeMethod(person, "CompareTo", otherPerson);
```

## Compiler Configuration

### PUBLIC_EXTENSIONS Flag

By default, the classes in this package are marked as `internal` to avoid namespace pollution in consuming projects. If you need direct access to classes from outside the assembly, you can make them `public` by defining the `PUBLIC_EXTENSIONS` compiler constant.

Add the following to your project file (`.csproj`) to make the extension classes public:

```xml
<PropertyGroup>
  <DefineConstants>$(DefineConstants);PUBLIC_EXTENSIONS</DefineConstants>
</PropertyGroup>
```

**When to use PUBLIC_EXTENSIONS:**

- You need direct access to extension classes from outside the assembly
- You're building libraries that extend or wrap this functionality
- You need to expose reflection utilities in your public API

**Default behavior (without PUBLIC_EXTENSIONS):**

- All extension classes remain `internal`
- Main functionality is still accessible through the public interfaces
- Cleaner public API surface for most use cases

## Performance Benefits

This package provides significant performance improvements over traditional reflection:

- **Compiled Expressions**: Method calls are compiled to optimized delegates
- **Caching**: Type accessors and member accessors are cached for reuse
- **Lazy Loading**: Expensive operations are deferred until first use
- **Optimized Type Conversion**: Smart handling of boxing/unboxing and type casting

## Thread Safety

All classes in this package are designed to be thread-safe:

- Static caches use `ConcurrentDictionary` for safe concurrent access
- Lazy initialization ensures thread-safe single initialization
- Compiled delegates are inherently thread-safe once created

## Integration with ORMs

This package is designed to work well with Object-Relational Mapping scenarios:

- Full support for Data Annotations
- Database column name mapping and resolution  
- Primary key and foreign key relationship support
- Concurrency token and database-generated value handling
- Table and schema name resolution

The comprehensive metadata provided by `IMemberInformation` makes it ideal for building ORM tools, object mappers, serializers, and other data access libraries that need efficient member access combined with rich metadata
