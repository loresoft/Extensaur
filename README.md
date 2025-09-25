# 🦖 Extensaur

**Extensaur** is a collection of high-performance, thoughtfully crafted extension methods designed to enhance your .NET development experience. It provides both a comprehensive package and focused source-only packages for specific functionality areas.

## 📦 Installation

### Main Package (Runtime Library)

The main `Extensaur` package contains all extensions as a runtime library:

```bash
Install-Package Extensaur
```

### Source-Only Packages

For developers who prefer source integration or need specific functionality, individual source-only packages are available:

```bash
# Collections extensions (ICollection, IDictionary, IEnumerable)
Install-Package Extensaur.Collections

# Data access extensions (IDataRecord, database utilities)
Install-Package Extensaur.Data

# Reflection utilities (TypeAccessor, high-performance member access)
Install-Package Extensaur.Reflection

# System extensions (String, DateTime, Type utilities)
Install-Package Extensaur.System

# Text processing (NameFormatter, StringBuilder extensions)
Install-Package Extensaur.Text
```

## 🎯 Key Features

- **High Performance**: Leverages compiled expressions and optimized algorithms
- **Comprehensive Coverage**: Extensions for collections, data access, reflection, system types, and text processing
- **Source Packages**: Zero-dependency source-only packages for specific functionality
- **Production Ready**: Extensively tested with comprehensive documentation
- **Modern .NET**: Targets .NET 8.0 and .NET 9.0 with nullable reference types
