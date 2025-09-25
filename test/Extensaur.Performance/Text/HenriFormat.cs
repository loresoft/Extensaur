using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Extensaur.Performance.Text;

public static class HenriFormat
{
    public static string Format(string format, object source)
    {
        if (format == null)
            throw new ArgumentNullException(nameof(format));

        if (source == null)
            return format;

        var result = new StringBuilder(format.Length * 2);

        using (var reader = new StringReader(format))
        {
            var expression = new StringBuilder();

            State state = State.OutsideExpression;
            do
            {
                int c = -1;
                switch (state)
                {
                    case State.OutsideExpression:
                        c = reader.Read();
                        switch (c)
                        {
                            case -1:
                                state = State.End;
                                break;
                            case '{':
                                state = State.OnOpenBracket;
                                break;
                            case '}':
                                state = State.OnCloseBracket;
                                break;
                            default:
                                result.Append((char)c);
                                break;
                        }
                        break;
                    case State.OnOpenBracket:
                        c = reader.Read();
                        switch (c)
                        {
                            case -1:
                                throw new FormatException();
                            case '{':
                                result.Append('{');
                                state = State.OutsideExpression;
                                break;
                            default:
                                expression.Append((char)c);
                                state = State.InsideExpression;
                                break;
                        }
                        break;
                    case State.InsideExpression:
                        c = reader.Read();
                        switch (c)
                        {
                            case -1:
                                throw new FormatException();
                            case '}':
                                string value = Evaluate(source, expression.ToString());
                                result.Append(value);
                                expression.Length = 0;
                                state = State.OutsideExpression;
                                break;
                            default:
                                expression.Append((char)c);
                                break;
                        }
                        break;
                    case State.OnCloseBracket:
                        c = reader.Read();
                        switch (c)
                        {
                            case '}':
                                result.Append('}');
                                state = State.OutsideExpression;
                                break;
                            default:
                                throw new FormatException();
                        }
                        break;
                    default:
                        throw new InvalidOperationException("Invalid state.");
                }
            } while (state != State.End);
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

    private enum State
    {
        OutsideExpression,
        OnOpenBracket,
        InsideExpression,
        OnCloseBracket,
        End
    }

}
