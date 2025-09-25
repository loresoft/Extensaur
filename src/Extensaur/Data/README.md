# Data Extensions

Source only package for Data extension methods for .NET data access types

This package provides extension methods for working with ADO.NET data access interfaces, specifically `IDataRecord`. These utilities simplify data retrieval operations by providing null-safe access, type conversion, and streamlined column access patterns commonly used in database operations.

## Key Features

- **Null-Safe Access**: Automatic handling of `DBNull` values with proper null conversion
- **Type Conversion**: Generic methods with automatic type casting and conversion
- **Column Access**: Access data by column name or ordinal position
- **Performance Optimized**: Uses `DbDataReader.GetFieldValue<T>` when available for better performance
- **Exception Safety**: Comprehensive error handling with descriptive exception messages

## Core Classes

### DataRecordExtensions

Provides enhanced data access methods for `IDataRecord` implementations, commonly used with `DataReader`, `DataRow`, and other ADO.NET data access objects.

```csharp
using System.Data;
using System.Data.SqlClient;

// Example with SqlDataReader
using var connection = new SqlConnection(connectionString);
using var command = new SqlCommand("SELECT Id, Name, Email, CreatedDate FROM Users", connection);
connection.Open();

using var reader = command.ExecuteReader();
while (reader.Read())
{
    // Basic null-safe value access
    var name = reader.GetValue("Name"); // Returns null if DBNull
    
    // Strongly-typed access with automatic conversion
    var id = reader.GetValue<int>("Id");
    var email = reader.GetValue<string>("Email");
    var createdDate = reader.GetValue<DateTime?>("CreatedDate");
    
    // Access by ordinal position
    var nameByOrdinal = reader.GetValue<string>(1);
    
    // Check for null values
    bool hasEmail = !reader.IsDBNull("Email");
}
```

**Key Features:**

- **Null Handling**: Automatically converts `DBNull` to null or default values
- **Type Safety**: Generic methods with compile-time type checking
- **Flexible Access**: Support for both column name and ordinal access patterns
- **Performance**: Optimized for `DbDataReader` with `GetFieldValue<T>` when available

### Method Overview

#### GetValue Methods

```csharp
// Get value as object, null if DBNull
object? value = dataRecord.GetValue("ColumnName");

// Get strongly-typed value, default(T) if DBNull
int id = dataRecord.GetValue<int>("Id");
string? name = dataRecord.GetValue<string>("Name");
DateTime? date = dataRecord.GetValue<DateTime?>("CreatedDate");

// Access by ordinal position
string email = dataRecord.GetValue<string>(2);
```

#### Null Checking

```csharp
// Check if column contains DBNull
bool isNull = dataRecord.IsDBNull("ColumnName");

// Safe processing pattern
if (!dataRecord.IsDBNull("Email"))
{
    var email = dataRecord.GetValue<string>("Email");
    ProcessEmail(email);
}
```

## Performance Notes

- Uses `DbDataReader.GetFieldValue<T>()` when the data record is a `DbDataReader` for optimal performance
- Falls back to standard `IDataRecord.GetValue()` with casting for other implementations
- Column ordinal lookup is cached by ADO.NET for repeated access
- Minimal overhead for null checks and type conversion

## Compiler Configuration

### PUBLIC_EXTENSIONS

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
