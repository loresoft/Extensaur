# System Extensions

Source only package for System extension methods for core .NET types

This package provides a comprehensive set of extension methods for working with fundamental .NET types, including `string`, `DateTime`, `DateTimeOffset`, `DateOnly`, `Type`, and safe type conversion utilities. These extensions simplify common operations such as string formatting, date/time manipulation, type inspection, and safe data type conversions.

## Core Classes

### ConvertExtensions

Provides safe string-to-type conversion methods that return null for nullable types when conversion fails, instead of throwing exceptions.

```csharp
using System;

string value = "123";

// Safe conversion to nullable types - returns null instead of throwing
int? number = value.ToInt32(); // Returns 123
int? invalid = "abc".ToInt32(); // Returns null instead of throwing

// Extended boolean parsing supports multiple formats
bool result1 = "yes".ToBoolean(); // true
bool result2 = "1".ToBoolean();   // true  
bool result3 = "on".ToBoolean();  // true

// Culture-aware conversions
decimal? price = "123.45".ToDecimal(CultureInfo.InvariantCulture);
DateTime? date = "2023-12-25".ToDateTime();
```

**Key Features:**

- **Safe Conversions**: Returns null instead of throwing exceptions for failed conversions
- **Extended Boolean Parsing**: Supports "t", "y", "yes", "1", "x", "on" as true values
- **Culture-Aware**: Optional `IFormatProvider` parameter for locale-specific parsing
- **Comprehensive Type Support**: All primitive .NET types plus `DateTime`, `DateTimeOffset`, `TimeSpan`, `Guid`, `DateOnly`, `TimeOnly`
- **Generic Conversion**: `SafeConvert` and `ConvertValue<T>` for dynamic type conversion

### DateTimeExtensions

Enhances `DateTime` and `DateTimeOffset` with timezone conversion and calendar operations.

```csharp
using System;

var dateTime = DateTime.Now;
var dateTimeOffset = DateTimeOffset.Now;

// Timezone conversion with proper handling of DateTimeKind
var easternTime = dateTime.ToTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
var utcTime = dateTimeOffset.ToTimeZone(TimeZoneInfo.Utc);

// Week number calculation with customizable rules
int weekNumber = dateTime.WeekNumber(); // Default: Sunday start, first four-day week rule
int isoWeek = dateTime.WeekNumber(DayOfWeek.Monday, CalendarWeekRule.FirstFourDayWeek);

// Works with DateTimeOffset too
int offsetWeek = dateTimeOffset.WeekNumber(DayOfWeek.Sunday);
```

**Key Features:**

- **Smart Timezone Conversion**: Handles `DateTimeKind.Local`, `DateTimeKind.Utc`, and `DateTimeKind.Unspecified` appropriately
- **Flexible Week Calculation**: Customizable first day of week and calendar week rules
- **Consistent API**: Same methods work for both `DateTime` and `DateTimeOffset`

### DateOnlyExtensions (.NET 6+)

Provides conversion utilities for `DateOnly` with timezone-aware operations.

```csharp
#if NET6_0_OR_GREATER
using System;

var date = new DateOnly(2023, 12, 25);

// Convert to DateTimeOffset with specific time and timezone
var christmas = date.ToDateTimeOffset(); // Midnight in local timezone
var christmasEvening = date.ToDateTimeOffset(new TimeOnly(18, 30)); // 6:30 PM local time
var christmasUtc = date.ToDateTimeOffset(TimeOnly.MinValue, TimeZoneInfo.Utc); // Midnight UTC

// Convert DateTimeOffset back to DateOnly in specific timezone
var nowUtc = DateTimeOffset.UtcNow;
var todayInPacific = nowUtc.ToDateOnly(TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));

// Week number calculation
int week = date.WeekNumber(DayOfWeek.Monday, CalendarWeekRule.FirstFourDayWeek);
#endif
```

**Key Features:**

- **Timezone-Aware Conversions**: Proper handling of time zones when converting between `DateOnly` and `DateTimeOffset`
- **Flexible Time Components**: Optional time specification, defaults to midnight
- **Week Number Support**: Consistent with `DateTime` and `DateTimeOffset` implementations

### StringExtensions

Comprehensive string manipulation utilities including formatting, casing, truncation, and validation.

```csharp
using System;

string text = "Hello World";

// Null and whitespace checking with better semantics
bool isEmpty = text.IsNullOrEmpty();     // false
bool hasContent = text.HasValue();       // true (opposite of IsNullOrEmpty)
bool isBlank = "   ".IsNullOrWhiteSpace(); // true

// String formatting
string formatted = "Hello {0}!".FormatWith("World"); // "Hello World!"
string localized = "Price: {0:C}".FormatWith(CultureInfo.GetCulture("fr-FR"), 123.45);

// Truncation with ellipsis
string truncated = "Very long text here".Truncate(10); // "Very lo..."
string customEllipsis = "Long text".Truncate(5, ".."); // "Lon.."

// Case conversion utilities
string title = "hello world program".ToTitle();        // "Hello World Program"
string pascal = "hello_world_program".ToPascalCase();  // "HelloWorldProgram"
string camel = "HelloWorldProgram".ToCamelCase();       // "helloWorldProgram"
string kebab = "HelloWorldProgram".ToKebabCase();       // "hello-world-program"
string snake = "HelloWorldProgram".ToSnakeCase();       // "hello_world_program"

// Path-like string combination
string path = "api".Combine("users", '/'); // "api/users"
string url = "https://api.com".Combine("v1/users"); // "https://api.com/v1/users"

// String masking for sensitive data
string masked = "4111111111111111".Mask(4, 4, '*'); // "4111********1111"
string email = "user@domain.com".Mask(2, 4, '*', '@'); // "us***@domain.com"
```

**Key Features:**

- **Enhanced Null Checking**: `HasValue()` provides semantic opposite of `IsNullOrEmpty()`
- **Flexible Formatting**: Extension method syntax for `string.Format`
- **Smart Truncation**: Customizable ellipsis with length-aware truncation
- **Multiple Case Conversions**: Support for Title, Pascal, Camel, Kebab, and Snake case
- **Path Combination**: Intelligent path/URL joining with separator handling
- **Data Masking**: Flexible masking for sensitive information with customizable patterns

### TypeExtensions

Enhances `Type` reflection capabilities with utilities for nullable types and interface checking.

```csharp
using System;

Type stringType = typeof(string);
Type intType = typeof(int);
Type nullableIntType = typeof(int?);

// Get underlying type from nullable types
Type underlying = nullableIntType.GetUnderlyingType(); // Returns typeof(int)
Type notNullable = intType.GetUnderlyingType();        // Returns typeof(int) (unchanged)

// Check if type can accept null values
bool canBeNull1 = stringType.IsNullable();      // true (reference type)
bool canBeNull2 = intType.IsNullable();         // false (value type)
bool canBeNull3 = nullableIntType.IsNullable(); // true (nullable value type)

// Interface implementation checking
bool implementsList = typeof(List<int>).Implements<IList<int>>(); // true
bool implementsEnum = typeof(string).Implements<IEnumerable>();   // true

// Generic interface checking
bool implementsGeneric = typeof(Dictionary<string, int>).Implements<IDictionary<string, int>>(); // true
```

**Key Features:**

- **Nullable Type Handling**: Safe extraction of underlying types from `Nullable<T>`
- **Nullability Detection**: Determines if a type can accept null values (reference types and nullable value types)
- **Interface Validation**: Type-safe interface implementation checking with compile-time validation
- **Generic Support**: Works with both generic and non-generic interface types

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

### Language Version

This package uses modern C# language features and requires C# 11.0 or later. If your project doesn't build or you encounter compiler errors, ensure your project is configured to use the latest C# language version:

```xml
<PropertyGroup>
  <LangVersion>latest</LangVersion>
</PropertyGroup>
```

**Why this is required:**

- The package uses modern C# features like file-scoped namespaces, global using statements, and improved pattern matching
- Older C# language versions may not recognize these syntax features
- Using `latest` ensures compatibility with the newest language features
