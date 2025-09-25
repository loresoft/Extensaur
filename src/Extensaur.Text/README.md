# Text Extensions

Source only package for Text extension methods and text building utilities for .NET

This package provides advanced text manipulation utilities including named placeholder formatting, enhanced StringBuilder operations, and high-performance string building with minimal heap allocations. These tools are designed for scenarios requiring efficient string composition, template processing, and conditional text generation.

## Core Classes

### NameFormatter

Provides string formatting functionality using named placeholders that are replaced with object property values. Supports nested property access, dictionary-based sources, and custom format strings.

```csharp
using System.Text;

var person = new { 
    Name = "John Doe", 
    Age = 30, 
    Address = new { City = "Seattle", State = "WA" },
    Salary = 75000.50m
};

// Basic named placeholder formatting
string result = "Hello {Name}, you are {Age} years old!".FormatName(person);
// Result: "Hello John Doe, you are 30 years old!"

// Nested property access with dot notation
string location = "You live in {Address.City}, {Address.State}".FormatName(person);
// Result: "You live in Seattle, WA"

// Custom formatting with colon syntax
string formatted = "Salary: {Salary:C}".FormatName(person);
// Result: "Salary: $75,000.50"

// Works with dictionaries too
var data = new Dictionary<string, object>
{
    ["ProductName"] = "Laptop",
    ["Price"] = 999.99,
    ["Stock"] = 15
};

string inventory = "{ProductName}: ${Price} (Stock: {Stock})".FormatName(data);
// Result: "Laptop: $999.99 (Stock: 15)"

// Complex template processing
string template = @"
Order Summary for {Customer.Name}:
- Product: {Product.Name} (${Product.Price:F2})
- Quantity: {Quantity}
- Total: {Total:C}
- Ship to: {Customer.Address.Street}, {Customer.Address.City}
";
string orderSummary = template.FormatName(orderData);
```

**Key Features:**

- **Named Placeholders**: Use property names instead of numeric indexes for clearer templates
- **Nested Property Access**: Support for deep object traversal using dot notation
- **Custom Formatting**: Standard .NET format strings with colon syntax (e.g., `{Price:C}`)
- **Dictionary Support**: Works with both objects and dictionary data sources
- **Null Safety**: Graceful handling of null values at any level of nesting
- **Performance Optimized**: Efficient parsing with minimal allocations

### StringBuilderExtensions

Enhances `StringBuilder` with conditional appending, formatted line operations, and collection joining capabilities.

```csharp
using System.Text;

var builder = new StringBuilder();

// Formatted line appending (combines AppendFormat + AppendLine)
builder.AppendLine("Hello {0}, today is {1:yyyy-MM-dd}", "John", DateTime.Today);

// Conditional appending with custom conditions
bool includeOptional = true;
builder.AppendIf("Optional content", includeOptional);
builder.AppendIf("   ", text => !string.IsNullOrWhiteSpace(text)); // Won't append whitespace

// Conditional line appending with lambda expressions
string errorMessage = GetErrorMessage();
builder.AppendLineIf(errorMessage, msg => !string.IsNullOrEmpty(msg));

// Function-based conditional appending (default checks for non-whitespace)
builder.AppendLineIf("Debug info"); // Only appends if string has content

// Collection joining similar to string.Join but for StringBuilder
var items = new[] { "apple", "banana", "cherry" };
builder.AppendJoin(", ", items); // "apple, banana, cherry"

// Works with any IEnumerable<T>
var numbers = Enumerable.Range(1, 5);
builder.AppendJoin(" | ", numbers); // "1 | 2 | 3 | 4 | 5"

// Chaining operations for fluent syntax
var result = new StringBuilder()
    .AppendLine("Report Generated: {0}", DateTime.Now)
    .AppendLineIf("=".PadLeft(50, '='))
    .AppendIf("Optional Header", includeHeader)
    .AppendJoin(Environment.NewLine, reportLines)
    .ToString();
```

**Key Features:**

- **Formatted Line Operations**: Combine formatting and line termination in single call
- **Flexible Conditional Logic**: Boolean conditions or custom predicate functions
- **Default Whitespace Filtering**: Built-in logic to skip null/whitespace content
- **Collection Joining**: Efficient joining of enumerable collections
- **Fluent Interface**: All methods return `StringBuilder` for method chaining
- **Type Safety**: Generic support for any collection type

### ValueStringBuilder

A high-performance, stack-friendly string builder that minimizes heap allocations by using stack-allocated or pooled buffers. Ideal for performance-critical scenarios with predictable string sizes.

```csharp
using System.Text;

// Stack-allocated buffer for small strings
Span<char> buffer = stackalloc char[256];
using var builder = new ValueStringBuilder(buffer);

builder.Append("Processing items: ");
for (int i = 0; i < items.Length; i++)
{
    if (i > 0) builder.Append(", ");
    builder.Append(items[i].ToString());
}

string result = builder.ToString(); // Efficient conversion to string

// Pool-based allocation for larger strings
using var largeBuilder = new ValueStringBuilder(1024);
largeBuilder.Append("Large content...");

// Insert operations
largeBuilder.Insert(0, "Prefix: ");
largeBuilder.Insert(largeBuilder.Length, " :Suffix");

// Span-based operations for maximum efficiency
ReadOnlySpan<char> content = largeBuilder.AsSpan();
bool success = largeBuilder.TryCopyTo(destinationBuffer, out int written);

// Character-level access
for (int i = 0; i < builder.Length; i++)
{
    ref char c = ref builder[i]; // Direct reference to buffer
    if (c == 'a') c = 'A'; // In-place modification
}

// Advanced buffer management
builder.EnsureCapacity(500); // Pre-allocate capacity
Span<char> appendBuffer = builder.AppendSpan(10); // Get span for direct writing
// Write directly to appendBuffer...

// Null-terminated strings for interop
ref char pinned = ref builder.GetPinnableReference(terminate: true);
```

**Key Features:**

- **Zero Heap Allocation**: Uses stack buffers or pooled arrays to avoid GC pressure
- **High Performance**: Direct buffer manipulation with minimal overhead
- **Flexible Initialization**: Stack-allocated span or pooled array backing
- **Direct Buffer Access**: Reference-based character access for in-place modifications
- **Span Integration**: Full integration with `Span<char>` and `ReadOnlySpan<char>`
- **Memory Pool Integration**: Automatic return of rented arrays via `Dispose()`
- **Capacity Management**: Manual capacity control for optimal performance

## Performance Notes

- **NameFormatter**: Optimized property reflection with minimal overhead for simple properties; nested properties use efficient dot-notation parsing
- **StringBuilderExtensions**: All operations maintain StringBuilder's performance characteristics while adding convenience
- **ValueStringBuilder**: Designed for high-performance scenarios:
  - Stack allocation eliminates heap allocations for small strings (< 256 chars typically)
  - Pool allocation reduces GC pressure for larger strings
  - Direct buffer access enables in-place modifications without string copies
  - Span-based operations provide zero-copy string processing

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
