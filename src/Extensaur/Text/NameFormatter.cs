using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Extensaur.Text;

/// <summary>
/// Named string formatter.
/// </summary>
public static class NameFormatter
{
    /// <summary>
    /// Replaces each named format item in a specified string with the text equivalent of a corresponding object's property value.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="source">The object to format.</param>
    /// <returns>A copy of format in which any named format items are replaced by the string representation.</returns>
    /// <example>
    /// <code>
    /// var o = new { First = "John", Last = "Doe" };
    /// string result = NameFormatter.Format("Full Name: {First} {Last}", o);
    /// </code>
    /// </example>
    public static string FormatName(this string format, object source)
    {
        if (format == null)
            throw new ArgumentNullException(nameof(format));

        if (format.Length == 0)
            return string.Empty;

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

    private static string Evaluate(object source, string expression)
    {
        if (source is null || string.IsNullOrEmpty(expression))
            return string.Empty;

        string format = null;

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

    private static object GetValue(object target, string name)
    {
        var currentType = target.GetType();
        var currentTarget = target;

        PropertyInfo property = null;

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
                currentType = property.PropertyType;
            }

            property = currentType.GetRuntimeProperty(part);
        }

        // return last property
        return property?.GetValue(currentTarget);
    }

    private static string FormatValue<T>(string format, T value)
    {
        if (value == null)
            return string.Empty;

        return string.IsNullOrEmpty(format)
          ? value.ToString()
          : string.Format("{0:" + format + "}", value);
    }
}
