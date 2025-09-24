using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LoreSoft.Extensions.Performance.Text;

public static class MarisicFormat
{
    public static string Format(this string format, object source)
    {
        if (format == null)
            throw new ArgumentNullException(nameof(format));
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
                while (true)
                {
                    if (!e.MoveNext())
                        throw new FormatException();

                    ch = e.Current;
                    if (ch == '}')
                    {
                        string value = Evaluate(source, expression.ToString());
                        result.Append(value);
                        expression.Length = 0;
                        break;
                    }
                    if (ch == '{')
                    {
                        result.Append(ch);
                        break;
                    }
                    expression.Append(ch);
                }
            }
            else if (ch == '}')
            {
                if (!e.MoveNext() || e.Current != '}')
                    throw new FormatException();

                result.Append('}');
            }
            else
            {
                result.Append(ch);
            }
        }

        return result.ToString();
    }

    private static string Evaluate(object source, string expression)
    {
        string format = "";

        int colonIndex = expression.IndexOf(':');
        if (colonIndex > 0)
        {
            format = expression[(colonIndex + 1)..];
            expression = expression[..colonIndex];
        }


        if (source is IDictionary<string, string> stringDictionary)
        {
            stringDictionary.TryGetValue(expression, out var value);
            return FormatValue(format, value);
        }
        else if (source is IDictionary<string, object> dictionary)
        {
            dictionary.TryGetValue(expression, out var value);
            return FormatValue(format, value);
        }
        else
        {
            var value = LateBinder.GetProperty(source, expression);
            return FormatValue(format, value);
        }
    }

    private static string FormatValue<T>(string format, T value)
    {
        if (value == null)
            return string.Empty;

        var formatted = string.IsNullOrEmpty(format)
          ? value.ToString()
          : string.Format("{0:" + format + "}", value);

        return formatted ?? string.Empty;
    }
}
