#pragma warning disable IDE0130 // Namespace does not match folder structure

#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace System.Text;

/// <summary>
/// Provides string formatting functionality using named placeholders that are replaced with object property values.
/// Supports nested property access, dictionary-based sources, and custom format strings.
/// </summary>
[ExcludeFromCodeCoverage]
#if PUBLIC_EXTENSIONS
public
#endif
static class NameFormatter
{
    /// <summary>
    /// Replaces each named format item in a specified string with the text equivalent of a corresponding object's property value.
    /// Supports nested property access using dot notation, dictionary sources, and custom format strings using colon syntax.
    /// </summary>
    /// <param name="format">A composite format string containing named placeholders enclosed in braces (e.g., "{PropertyName}" or "{Property:format}").</param>
    /// <param name="source">The object containing the properties to use for replacement. Can be an object with properties, or a dictionary.</param>
    /// <returns>
    /// A copy of <paramref name="format"/> in which any named format items are replaced by the string representation
    /// of the corresponding property values from <paramref name="source"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="format"/> is null.</exception>
    /// <exception cref="FormatException">Thrown when the format string contains unmatched braces or invalid format syntax.</exception>
    public static string FormatName(this string format, object? source)
    {
        if (format == null)
            throw new ArgumentNullException(nameof(format));

        if (format.Length == 0)
            return string.Empty;

        if (source == null)
            return format;

        var result = new StringBuilder(format.Length * 2);
        var expression = new StringBuilder();

        var e = format.GetEnumerator();
        while (e.MoveNext())
        {
            var ch = e.Current;
            if (ch == '{')
            {
                // start expression block, continue till closing char
                while (true)
                {
                    // end of format string without closing expression
                    if (!e.MoveNext())
                        throw new FormatException();

                    ch = e.Current;
                    if (ch == '}')
                    {
                        // close expression block, evaluate expression and add to result
                        string value = Evaluate(source, expression.ToString());
                        result.Append(value);

                        // reset expression buffer
                        expression.Length = 0;
                        break;
                    }
                    if (ch == '{')
                    {
                        // double expression start, add to result
                        result.Append(ch);
                        break;
                    }

                    // add to expression buffer
                    expression.Append(ch);
                }
            }
            else if (ch == '}')
            {
                // close expression char without having started one
                if (!e.MoveNext() || e.Current != '}')
                    throw new FormatException();

                // double expression close, add to result
                result.Append('}');
            }
            else
            {
                // normal char, add to result
                result.Append(ch);
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Evaluates a named expression against the source object and returns the formatted string representation.
    /// Handles dictionary sources and object properties, with support for nested property access and custom formatting.
    /// </summary>
    /// <param name="source">The source object to evaluate the expression against.</param>
    /// <param name="expression">The property name or expression to evaluate, optionally with format specifier after a colon.</param>
    /// <returns>The formatted string representation of the evaluated expression, or an empty string if the expression cannot be resolved.</returns>
    private static string Evaluate(object? source, string expression)
    {
        if (source is null || string.IsNullOrEmpty(expression))
            return string.Empty;

        string? format = null;

        // support format string {0:d}
        int colonIndex = expression.IndexOf(':');
        if (colonIndex > 0)
        {
            format = expression.Substring(colonIndex + 1);
            expression = expression.Substring(0, colonIndex);
        }

        // better way to support more dictionary generics?
        if (source is IDictionary<string, string> stringDictionary)
        {
            stringDictionary.TryGetValue(expression, out var value);
            return FormatValue(format, value);
        }
        else if (source is IDictionary<string, object> objectDictionary)
        {
            objectDictionary.TryGetValue(expression, out var value);
            return FormatValue(format, value);
        }
        else if (source is System.Collections.IDictionary dictionary)
        {
            var value = dictionary[expression];
            return FormatValue(format, value);
        }
        else
        {
            var value = GetValue(source, expression);
            return FormatValue(format, value);
        }
    }

    /// <summary>
    /// Gets the value of a property from the target object, supporting nested property access using dot notation.
    /// </summary>
    /// <param name="target">The object to retrieve the property value from.</param>
    /// <param name="name">The property name, which can include nested properties separated by dots (e.g., "Address.City").</param>
    /// <returns>The value of the specified property, or null if the property is not found or is null at any level.</returns>
    private static object? GetValue(object target, string name)
    {
        var currentType = target.GetType();
        object? currentTarget = target;

        PropertyInfo? property = null;

        // optimization if no nested property
        if (!name.Contains('.'))
        {
            property = currentType.GetRuntimeProperty(name);
            return property?.GetValue(currentTarget);
        }

        // support nested property
        foreach (var part in name.Split('.'))
        {
            if (property != null)
            {
                // pending property, get value and type
                currentTarget = property.GetValue(currentTarget);
                if (currentTarget is null)
                    return null;

                currentType = property.PropertyType;
            }

            property = currentType.GetRuntimeProperty(part);
            if (property is null)
                return null;
        }

        // return last property
        return property?.GetValue(currentTarget);
    }

    /// <summary>
    /// Formats a value using the specified format string, handling null values appropriately.
    /// </summary>
    /// <typeparam name="T">The type of the value to format.</typeparam>
    /// <param name="format">The format string to apply, or null/empty for default formatting.</param>
    /// <param name="value">The value to format.</param>
    /// <returns>
    /// The formatted string representation of <paramref name="value"/>, or an empty string if <paramref name="value"/> is null.
    /// </returns>
    private static string FormatValue<T>(string? format, T? value)
    {
        if (value == null)
            return string.Empty;

        return string.IsNullOrEmpty(format)
          ? value.ToString() ?? string.Empty
          : string.Format("{0:" + format + "}", value);
    }
}
