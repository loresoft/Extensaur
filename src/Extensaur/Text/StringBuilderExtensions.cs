#pragma warning disable IDE0130 // Namespace does not match folder structure

#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Text;

/// <summary>
/// Provides extension methods for <see cref="StringBuilder"/> to enhance string building operations with conditional appending and formatting capabilities.
/// </summary>
[ExcludeFromCodeCoverage]
#if PUBLIC_EXTENSIONS
public
#endif
static class StringBuilderExtensions
{
    /// <summary>
    /// Appends a formatted string followed by the default line terminator to the end of the <see cref="StringBuilder"/> object.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder"/> instance to append to.</param>
    /// <param name="format">A composite format string containing formatting specifications.</param>
    /// <param name="args">An array of objects to format and append.</param>
    /// <returns>A reference to the <see cref="StringBuilder"/> instance after the append operation has completed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    /// <exception cref="FormatException">Thrown when the format string is invalid or the arguments don't match the format specifications.</exception>
    public static StringBuilder AppendLine(this StringBuilder builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object[] args)
    {
        builder.AppendFormat(format, args);
        builder.AppendLine();
        return builder;
    }

    /// <summary>
    /// Conditionally appends a copy of the specified string to the <see cref="StringBuilder"/> based on the provided condition.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder"/> instance to append to.</param>
    /// <param name="text">The string to append if the condition is met.</param>
    /// <param name="condition">The condition delegate to evaluate. If null, defaults to checking if the text is not null or whitespace.</param>
    /// <returns>A reference to the <see cref="StringBuilder"/> instance after the conditional append operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static StringBuilder AppendIf(this StringBuilder builder, string? text, Func<string?, bool>? condition = null)
    {
        var c = condition ?? (s => !string.IsNullOrWhiteSpace(s));

        if (c(text))
            builder.Append(text);

        return builder;
    }

    /// <summary>
    /// Conditionally appends a copy of the specified string to the <see cref="StringBuilder"/> based on the provided boolean condition.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder"/> instance to append to.</param>
    /// <param name="text">The string to append if the condition is true.</param>
    /// <param name="condition">The boolean condition that determines whether to append the text.</param>
    /// <returns>A reference to the <see cref="StringBuilder"/> instance after the conditional append operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static StringBuilder AppendIf(this StringBuilder builder, string? text, bool condition)
    {
        if (condition)
            builder.Append(text);

        return builder;
    }

    /// <summary>
    /// Conditionally appends a copy of the specified string followed by the default line terminator to the <see cref="StringBuilder"/> based on the provided condition.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder"/> instance to append to.</param>
    /// <param name="text">The string to append if the condition is met.</param>
    /// <param name="condition">The condition delegate to evaluate. If null, defaults to checking if the text is not null or whitespace.</param>
    /// <returns>A reference to the <see cref="StringBuilder"/> instance after the conditional append operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static StringBuilder AppendLineIf(this StringBuilder builder, string? text, Func<string?, bool>? condition = null)
    {
        var c = condition ?? (s => !string.IsNullOrWhiteSpace(s));

        if (c(text))
            builder.AppendLine(text);

        return builder;
    }

    /// <summary>
    /// Conditionally appends a copy of the specified string followed by the default line terminator to the <see cref="StringBuilder"/> based on the provided boolean condition.
    /// </summary>
    /// <param name="builder">The <see cref="StringBuilder"/> instance to append to.</param>
    /// <param name="text">The string to append if the condition is true.</param>
    /// <param name="condition">The boolean condition that determines whether to append the text with a line terminator.</param>
    /// <returns>A reference to the <see cref="StringBuilder"/> instance after the conditional append operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static StringBuilder AppendLineIf(this StringBuilder builder, string? text, bool condition)
    {
        if (condition)
            builder.AppendLine(text);

        return builder;
    }

    /// <summary>
    /// Concatenates and appends the string representations of the elements in a collection, using the specified separator between each element.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="builder">The <see cref="StringBuilder"/> instance to append to.</param>
    /// <param name="separator">The string to use as a separator between elements. If null, an empty string is used.</param>
    /// <param name="values">The collection containing the elements to concatenate and append.</param>
    /// <returns>A reference to the <see cref="StringBuilder"/> instance after the append operation has completed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="values"/> is null.</exception>
    public static StringBuilder AppendJoin<T>(this StringBuilder builder, string? separator, IEnumerable<T?> values)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));
        if (values is null)
            throw new ArgumentNullException(nameof(values));

        separator ??= string.Empty;

        var wroteValue = false;

        foreach (var value in values)
        {
            if (wroteValue)
                builder.Append(separator);

            builder.Append(value);
            wroteValue = true;
        }

        return builder;
    }
}
