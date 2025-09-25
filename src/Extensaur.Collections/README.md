# Collections Extensions

Source only package for Collections extension methods for .NET collection types

This package provides a comprehensive set of extension methods for working with common .NET collection interfaces, including `ICollection<T>`, `IDictionary<TKey, TValue>`, and `IEnumerable<T>`. These utilities simplify common patterns such as conditional addition, lazy initialization, indexed access, and string formatting.

## Core Classes

### CollectionExtensions

Provides advanced conditional addition methods for `ICollection<T>` implementations.

```csharp
using System.Collections.Generic;

var items = new List<Item>();

// Find first matching item or create and add a new one
var item = items.FirstOrAdd(x => x.Id == 123, () => new Item { Id = 123, Name = "New Item" });

// Useful for caching scenarios
var cache = new List<CacheEntry>();
var entry = cache.FirstOrAdd(e => e.Key == "user:123", () => LoadUserData("123"));
```

**Key Features:**

- Thread-safe lazy initialization pattern
- Combines search and creation in a single operation
- Supports any predicate condition for matching
- Factory function only called when needed

### DictionaryExtensions

Simplifies value retrieval and lazy creation for `IDictionary<TKey, TValue>` implementations.

```csharp
using System.Collections.Generic;

var cache = new Dictionary<string, ExpensiveObject>();

// Get existing value or create new one with factory
var obj = cache.GetOrAdd("key1", key => new ExpensiveObject(key));

// Factory receives the key as parameter
var userCache = new Dictionary<int, User>();
var user = userCache.GetOrAdd(userId, id => LoadUserFromDatabase(id));
```

**Key Features:**

- Two overloads: factory function or pre-computed value
- Factory function receives key as parameter for key-dependent creation
- Atomic check-and-add operation
- Compatible with any `IDictionary<TKey, TValue>` implementation

### EnumerableExtensions

Adds indexed access and string formatting utilities for `IEnumerable<T>` and non-generic `IEnumerable`.

```csharp
using System.Collections;
using System.Collections.Generic;

// Indexed access for non-generic IEnumerable
ArrayList list = new ArrayList { "a", "b", "c" };
var item = list.ElementAt(1); // Returns "b"

// Safe indexed access
var safeItem = list.ElementAtOrDefault(10); // Returns null instead of throwing

// String formatting with custom delimiter
var numbers = new[] { 1, 2, 3, 4, 5 };
var csv = numbers.ToDelimitedString(","); // "1,2,3,4,5"
var pipe = numbers.ToDelimitedString(" | "); // "1 | 2 | 3 | 4 | 5"

// Works with strings too
var words = new[] { "apple", "banana", "cherry" };
var sentence = words.ToDelimitedString(" and "); // "apple and banana and cherry"

// Handles null values gracefully
var mixed = new string[] { "a", null, "c" };
var result = mixed.ToDelimitedString(); // "a,,c"
```

**Key Features:**

- **Performance Optimized**: Direct array/list access when possible, falls back to enumeration
- **Safe Access**: `ElementAtOrDefault` returns null instead of throwing exceptions
- **Flexible Formatting**: Customizable delimiters for string concatenation
- **Null Handling**: Graceful handling of null elements in sequences
- **Type Support**: Works with any `IEnumerable<T>` and non-generic `IEnumerable`

## Performance Notes

- `ElementAt` and `ElementAtOrDefault` are optimized for arrays and `IList` implementations
- `GetOrAdd` performs single dictionary lookup to minimize overhead
- `FirstOrAdd` uses LINQ's `Where` for efficient predicate matching
- All methods include comprehensive null checks with minimal performance impact

## Common Use Cases

### Caching Patterns

```csharp
// Method result caching
private readonly Dictionary<string, string> _cache = new();

public string GetProcessedData(string key)
{
    return _cache.GetOrAdd(key, k => ExpensiveOperation(k));
}

// Collection-based caching
private readonly List<ConfigItem> _configs = new();

public ConfigItem GetConfig(string name)
{
    return _configs.FirstOrAdd(c => c.Name == name, () => LoadConfig(name));
}
```

### Data Processing

```csharp
// Safe enumeration of mixed collections
ArrayList legacy = GetLegacyData();
for (int i = 0; i < 10; i++)
{
    var item = legacy.ElementAtOrDefault(i);
    if (item != null)
        ProcessItem(item);
}

// String formatting for reports
var metrics = GetMetrics();
var report = $"Values: {metrics.ToDelimitedString(" | ")}";
```

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
