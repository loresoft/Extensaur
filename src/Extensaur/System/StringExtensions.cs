#pragma warning disable IDE0130 // Namespace does not match folder structure

#nullable enable

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System;

/// <summary>
/// Extension methods for <see cref="string"/> to provide additional string manipulation and formatting utilities.
/// </summary>
[ExcludeFromCodeCoverage]
#if PUBLIC_EXTENSIONS
public
#endif
 static class StringExtensions
{
    /// <summary>
    /// Determines whether the specified string is <c>null</c> or an empty string ("").
    /// </summary>
    /// <param name="item">The string to check.</param>
    /// <returns>
    /// <c>true</c> if the value parameter is <c>null</c> or an empty string (""); otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? item)
    {
        return string.IsNullOrEmpty(item);
    }

    /// <summary>
    /// Determines whether a string is <c>null</c>, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="item">The string to check.</param>
    /// <returns>
    /// <c>true</c> if the value parameter is <c>null</c> or <see cref="string.Empty"/>, or if value consists exclusively of white-space characters; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? item)
    {
        if (item == null)
            return true;

        for (int i = 0; i < item.Length; i++)
            if (!char.IsWhiteSpace(item[i]))
                return false;

        return true;
    }

    /// <summary>
    /// Determines whether the specified string is not <c>null</c> or empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>
    /// <c>true</c> if the value parameter is not <c>null</c> or empty; otherwise, <c>false</c>.
    /// </returns>
    public static bool HasValue([NotNullWhen(true)] this string? value)
    {
        return !string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Formats the string using the specified arguments, similar to <see cref="string.Format(string, object?[])"/>.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to format.</param>
    /// <returns>
    /// A copy of <paramref name="format"/> in which the format items have been replaced by the string representation of the corresponding objects in <paramref name="args"/>.
    /// </returns>
    public static string FormatWith([StringSyntax(StringSyntaxAttribute.CompositeFormat)] this string format, params object?[] args)
    {
        return string.Format(format, args);
    }

    /// <summary>
    /// Formats the string using the specified format provider and arguments, similar to <see cref="string.Format(IFormatProvider, string, object?[])"/>.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="args">An array of objects to format.</param>
    /// <returns>
    /// A copy of <paramref name="format"/> in which the format items have been replaced by the string representation of the corresponding objects in <paramref name="args"/>.
    /// </returns>
    public static string FormatWith([StringSyntax(StringSyntaxAttribute.CompositeFormat)] this string format, IFormatProvider? provider, params object?[] args)
    {
        return string.Format(provider, format, args);
    }


    /// <summary>
    /// Truncates the specified string to a maximum length, optionally appending an ellipsis or custom suffix if truncation occurs.
    /// </summary>
    /// <param name="text">The string to truncate.</param>
    /// <param name="keep">The number of characters to keep (including the ellipsis, if used).</param>
    /// <param name="ellipsis">The string to append if truncation occurs. Defaults to "..." if not specified.</param>
    /// <returns>
    /// The truncated string with the ellipsis (or custom suffix) appended if truncation occurred; otherwise, the original string.
    /// Returns <c>null</c> if <paramref name="text"/> is <c>null</c>.
    /// </returns>
    [return: NotNullIfNotNull(nameof(text))]
    public static string? Truncate(this string? text, int keep, string? ellipsis = "...")
    {
        if (string.IsNullOrEmpty(text) || text.Length <= keep)
            return text;

        ellipsis ??= string.Empty;

        int ellipsisLength = ellipsis.Length;

        // If there's no room for ellipsis, just return truncated prefix
        if (keep <= ellipsisLength)
            return text[..keep];

        int prefixLength = keep - ellipsisLength;
        int totalLength = prefixLength + ellipsisLength;

        // Use stack allocation for short strings, or rent from the pool for longer ones
        char[]? rentedArray = null;
        Span<char> buffer = totalLength <= 256
            ? stackalloc char[totalLength]
            : (rentedArray = ArrayPool<char>.Shared.Rent(totalLength));

        try
        {
            text.AsSpan(0, prefixLength).CopyTo(buffer);
            ellipsis.AsSpan().CopyTo(buffer[prefixLength..]);

            return new string(buffer[..totalLength]);
        }
        finally
        {
            // Return rented array to the pool to avoid memory leaks
            if (rentedArray != null)
                ArrayPool<char>.Shared.Return(rentedArray);
        }
    }

    /// <summary>
    /// Converts a string to a human-readable title case, inserting spaces at word boundaries.
    /// Word boundaries are detected at transitions between lowercase and uppercase letters, between letters and digits, and at non-alphanumeric characters.
    /// Non-alphanumeric characters are treated as word separators and are replaced with spaces.
    /// The first letter of each word is capitalized.
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <returns>
    /// A title-cased string with spaces inserted at word boundaries, or <c>null</c> if <paramref name="input"/> is <c>null</c>.
    /// If <paramref name="input"/> is empty, returns an empty string.
    /// </returns>
    [return: NotNullIfNotNull(nameof(input))]
    public static string? ToTitle(this string? input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        ReadOnlySpan<char> span = input.AsSpan();

        // Estimate output size with padding for added spaces
        int estimatedSize = span.Length + (span.Length / 4);

        // Use stack allocation for short strings, or rent from the pool for longer ones
        char[]? rentedArray = null;
        Span<char> buffer = estimatedSize <= 256
            ? stackalloc char[estimatedSize]
            : (rentedArray = ArrayPool<char>.Shared.Rent(estimatedSize));

        try
        {
            int j = 0;                 // Buffer write index
            bool atWordStart = true;   // Track if next character starts a new word

            for (int i = 0; i < span.Length; i++)
            {
                char current = span[i];

                // Treat any non-alphanumeric character as a word separator (convert to space)
                if (!char.IsLetterOrDigit(current))
                {
                    if (j > 0 && buffer[j - 1] != ' ')
                    {
                        buffer[j++] = ' ';
                        atWordStart = true;
                    }
                    continue;
                }

                if (i > 0)
                {
                    char prev = span[i - 1];

                    bool isUpper = char.IsUpper(current);
                    bool wasLower = char.IsLower(prev);
                    bool wasUpper = char.IsUpper(prev);
                    bool isDigit = char.IsDigit(current);
                    bool wasLetter = char.IsLetter(prev);
                    bool wasDigit = char.IsDigit(prev);

                    // Determine if a space should be inserted before the current character
                    bool insertSpace =
                        // Case 1: lowercase letter followed by uppercase (camelCase transition) "firstName" → "First Name"
                        (isUpper && wasLower) ||
                        // Case 2: consecutive uppercase letters followed by a lowercase letter (e.g., "PDFName" → "PDF Name")
                        (isUpper && wasUpper && i + 1 < span.Length && char.IsLower(span[i + 1])) ||
                        // Case 3: letter followed by digit (e.g., "Version2" → "Version 2")
                        (isDigit && wasLetter) ||
                        // Case 4: digit followed by letter (e.g., "IP4Address" → "IP 4 Address")
                        (char.IsLetter(current) && wasDigit);

                    if (insertSpace)
                    {
                        buffer[j++] = ' ';
                        atWordStart = true;
                    }
                }

                if (atWordStart)
                {
                    // Capitalize the first letter of each word
                    buffer[j++] = char.ToUpperInvariant(current);
                    atWordStart = false;
                }
                else
                {
                    buffer[j++] = current;
                }
            }

            return new string(buffer.Slice(0, j));
        }
        finally
        {
            // Return rented array to the pool to avoid memory leaks
            if (rentedArray != null)
                ArrayPool<char>.Shared.Return(rentedArray);
        }
    }

    /// <summary>
    /// Converts a string to PascalCase, removing non-alphanumeric characters and capitalizing the first letter of each word.
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <returns>
    /// The PascalCase version of the input string, or <c>null</c> if <paramref name="input"/> is <c>null</c> or whitespace.
    /// </returns>
    [return: NotNullIfNotNull(nameof(input))]
    public static string? ToPascalCase(this string? input)
    {
        return ToNameCase(input, upperFirst: true);
    }

    /// <summary>
    /// Converts a string to camelCase, removing non-alphanumeric characters and capitalizing the first letter of each word except the first.
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <returns>
    /// The camelCase version of the input string, or <c>null</c> if <paramref name="input"/> is <c>null</c> or whitespace.
    /// </returns>
    [return: NotNullIfNotNull(nameof(input))]
    public static string? ToCamelCase(this string? input)
    {
        return ToNameCase(input, upperFirst: false);
    }

    private static string? ToNameCase(string? input, bool upperFirst)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        ReadOnlySpan<char> span = input.AsSpan();

        // Estimate output size (input length is a safe upper bound)
        int estimatedSize = span.Length;

        // Use stack allocation for short strings, or rent from the pool for longer ones
        char[]? rentedArray = null;
        Span<char> buffer = estimatedSize <= 256
            ? stackalloc char[estimatedSize]
            : (rentedArray = ArrayPool<char>.Shared.Rent(estimatedSize));

        try
        {
            int j = 0;                          // Output buffer index
            bool capitalizeNext = upperFirst;   // Indicates if next valid char should be capitalized

            // Iterate over each character in the input
            for (int i = 0; i < span.Length; i++)
            {
                char c = span[i];

                // Treat non-alphanumeric characters as delimiters that cause capitalization of next char
                if (!char.IsLetterOrDigit(c))
                {
                    capitalizeNext = true; // Next valid char should be capitalized
                    continue;              // Skip delimiter chars
                }

                // Capitalize or lowercase character depending on position and flag
                if (j == 0)
                {
                    // First character: uppercase for PascalCase, lowercase for camelCase
                    buffer[j++] = upperFirst
                        ? char.ToUpperInvariant(c)
                        : char.ToLowerInvariant(c);
                }
                else
                {
                    // Subsequent characters: capitalize if capitalizeNext is true, else lowercase
                    buffer[j++] = capitalizeNext
                        ? char.ToUpperInvariant(c)
                        : char.ToLowerInvariant(c);
                }

                capitalizeNext = false; // Reset flag after processing a valid char
            }

            // Handle edge case: if input had no delimiters (no non-alphanumeric chars)
            // and length matches output length,
            // assume input was already PascalCase or camelCase and preserve the original casing
            if (j == span.Length && input.All(c => char.IsLetterOrDigit(c)))
            {
                // Force the first character case according to upperFirst flag
                buffer[0] = upperFirst
                    ? char.ToUpperInvariant(buffer[0])
                    : char.ToLowerInvariant(buffer[0]);

                // Copy the rest of the characters from the original input to preserve casing
                for (int i = 1; i < j; i++)
                    buffer[i] = input[i];
            }

            // Create and return a new string from the populated span slice
            return new string(buffer.Slice(0, j));
        }
        finally
        {
            // Return rented array to the pool to avoid memory leaks
            if (rentedArray != null)
                ArrayPool<char>.Shared.Return(rentedArray);
        }
    }

    /// <summary>
    /// Converts a string to kebab-case, inserting hyphens at word boundaries and converting all characters to lowercase.
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <returns>
    /// The kebab-case version of the input string, or <c>null</c> if <paramref name="input"/> is <c>null</c> or whitespace.
    /// </returns>
    [return: NotNullIfNotNull(nameof(input))]
    public static string? ToKebabCase(this string? input)
    {
        return ToDelimitedCase(input, '-');
    }

    /// <summary>
    /// Converts a string to snake_case, inserting underscores at word boundaries and converting all characters to lowercase.
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <returns>
    /// The snake_case version of the input string, or <c>null</c> if <paramref name="input"/> is <c>null</c> or whitespace.
    /// </returns>
    [return: NotNullIfNotNull(nameof(input))]
    public static string? ToSnakeCase(this string? input)
    {
        return ToDelimitedCase(input, '_');
    }

    private static string? ToDelimitedCase(string? input, char delimiter)
    {
        // Return early if input is null, empty, or whitespace
        if (string.IsNullOrWhiteSpace(input))
            return input;

        ReadOnlySpan<char> span = input.AsSpan();
        // Estimate max output size as twice the input length (to accommodate delimiters)
        int estimatedSize = input.Length * 2;

        // Use stackalloc for small inputs, rent from ArrayPool for larger inputs
        char[]? rentedArray = null;
        Span<char> result = estimatedSize <= 512
            ? stackalloc char[estimatedSize]
            : (rentedArray = ArrayPool<char>.Shared.Rent(estimatedSize));

        try
        {
            int resultIndex = 0;

            // Tracks whether the last appended character was a delimiter
            bool lastWasDelimiter = false;

            for (int i = 0; i < span.Length; i++)
            {
                char current = span[i];

                // If the character is NOT a letter or digit:
                if (!char.IsLetterOrDigit(current))
                {
                    // Insert delimiter only if:
                    // - This is not the first character in result (resultIndex > 0)
                    // - The last appended character was not already a delimiter (avoid duplicates)
                    if (resultIndex > 0 && !lastWasDelimiter)
                    {
                        result[resultIndex++] = delimiter;
                        lastWasDelimiter = true;
                    }
                    // Skip adding multiple delimiters for consecutive non-alphanumeric chars
                    continue;
                }

                // Get previous and next characters for boundary checks (if they exist)
                char? prev = i > 0 ? span[i - 1] : null;
                char? next = i < span.Length - 1 ? span[i + 1] : null;

                bool isCurrentUpper = char.IsUpper(current);
                bool isCurrentDigit = char.IsDigit(current);

                bool addDelimiter = false;

                // Determine if a delimiter should be inserted before the current character,
                // but only if the last appended character was not a delimiter (avoid duplicates)
                if (resultIndex > 0 && !lastWasDelimiter)
                {
                    // Insert delimiter before digits if preceded by a letter
                    if (isCurrentDigit && prev.HasValue && char.IsLetter(prev.Value))
                    {
                        addDelimiter = true;
                    }
                    // Insert delimiter before uppercase letters in these cases:
                    // 1) Previous char is lowercase or digit (camelCase boundary)
                    // 2) Next char is lowercase (splitting acronyms like XMLHttp)
                    else if (isCurrentUpper)
                    {
                        if (prev.HasValue && (char.IsLower(prev.Value) || char.IsDigit(prev.Value)))
                        {
                            addDelimiter = true;
                        }
                        else if (next.HasValue && char.IsLower(next.Value))
                        {
                            addDelimiter = true;
                        }
                    }
                }

                // Append delimiter if determined needed
                if (addDelimiter)
                {
                    result[resultIndex++] = delimiter;
                }

                // Append lowercase version of the current character
                result[resultIndex++] = char.ToLowerInvariant(current);

                // Reset delimiter tracker since we appended a non-delimiter character
                lastWasDelimiter = false;
            }

            // If no valid characters found, return null
            if (resultIndex == 0)
                return null;

            // Remove trailing delimiter if present (to clean up output)
            if (result[resultIndex - 1] == delimiter)
                resultIndex--;

            // Create final string from the filled portion of the span
            return new string(result.Slice(0, resultIndex));
        }
        finally
        {
            // Return rented array back to the pool to avoid memory leaks
            if (rentedArray != null)
                ArrayPool<char>.Shared.Return(rentedArray);
        }
    }

    /// <summary>
    /// Combines two strings with the specified separator, ensuring only a single separator character is present between them.
    /// </summary>
    /// <param name="first">The first string.</param>
    /// <param name="second">The second string.</param>
    /// <param name="separator">The separator character to use. Defaults to '/'.</param>
    /// <returns>
    /// A string combining <paramref name="first"/> and <paramref name="second"/> with the <paramref name="separator"/> between them.
    /// If either string is <c>null</c> or empty, returns the other string.
    /// </returns>
    [return: NotNullIfNotNull(nameof(first))]
    [return: NotNullIfNotNull(nameof(second))]
    public static string? Combine(this string? first, string? second, char separator = '/')
    {
        if (string.IsNullOrEmpty(first))
            return second;

        if (string.IsNullOrEmpty(second))
            return first;

        bool firstEndsWith = first![^1] == separator;
        bool secondStartsWith = second![0] == separator;

        int firstLength = first.Length;
        int secondLength = second.Length;

        if (firstEndsWith && !secondStartsWith || !firstEndsWith && secondStartsWith)
        {
            // No separator adjustment needed
            var totalLength = firstLength + secondLength;
            return string.Create(totalLength, (first, second), static (span, state) =>
            {
                state.first.AsSpan().CopyTo(span);
                state.second.AsSpan().CopyTo(span[state.first.Length..]);
            });
        }

        if (firstEndsWith && secondStartsWith)
        {
            // Remove one separator to avoid duplication
            var totalLength = firstLength + secondLength - 1;
            return string.Create(totalLength, (first, second), static (span, state) =>
            {
                state.first.AsSpan().CopyTo(span);
                state.second.AsSpan(1).CopyTo(span[state.first.Length..]);
            });
        }

        // Need to insert a separator
        var total = firstLength + 1 + secondLength;
        return string.Create(total, (first, second, separator), static (span, state) =>
        {
            state.first.AsSpan().CopyTo(span);
            span[state.first.Length] = state.separator;
            state.second.AsSpan().CopyTo(span[(state.first.Length + 1)..]);
        });
    }

    /// <summary>
    /// Masks a string by replacing characters with a specified mask character,
    /// preserving a certain number of characters at the start and end.
    /// Optionally, you can specify a fixed number of masked characters in the middle.
    /// </summary>
    /// <param name="input">The input string to mask.</param>
    /// <param name="unmaskedStart">Number of characters to leave unmasked at the start.</param>
    /// <param name="unmaskedEnd">Number of characters to leave unmasked at the end.</param>
    /// <param name="maskChar">The character to use for masking. Default is '*'.</param>
    /// <param name="maskedCount">
    /// Optional. If specified, the number of mask characters to use in the masked section.
    /// If not specified, the masked section will fill the space between the unmasked start and end.
    /// </param>
    /// <returns>The masked string.</returns>
    /// <example>
    /// <code>
    /// mask credit card number
    /// string maskedCreditCard = "4111111111111111".Mask(4, 4);
    /// Assert.Equal("4111********1111", maskedCreditCard);
    ///
    /// // mask a social security number
    /// var maskedSocial = "123-45-6789".Mask(0, 4, '*', 5);
    /// Assert.Equal("*****6789", maskedSocial);
    ///
    /// // mask a password
    /// var maskedPassword = "P@ssw0rd123".Mask(0, 0, '*', 8);
    /// Assert.Equal("********", maskedPassword);
    /// </code>
    /// </example>
    [return: NotNullIfNotNull(nameof(input))]
    public static string? Mask(
        this string? input,
        int unmaskedStart,
        int unmaskedEnd,
        char maskChar = '*',
        int? maskedCount = null)
    {
        if (input.IsNullOrEmpty())
            return input;

        int length = input.Length;

        if (unmaskedStart + unmaskedEnd > length)
            throw new ArgumentOutOfRangeException(nameof(input), "Unmasked prefix and suffix exceed input length.");

        if (length == 0 || (unmaskedStart == 0 && unmaskedEnd == 0 && maskChar == '\0'))
            return string.Empty;

        int maskedSectionLength = maskedCount ?? (length - unmaskedStart - unmaskedEnd);
        if (maskedSectionLength < 0)
            maskedSectionLength = 0;

        int totalLength = unmaskedStart + maskedSectionLength + unmaskedEnd;

        Span<char> buffer = totalLength <= 256
            ? stackalloc char[totalLength]
            : ArrayPool<char>.Shared.Rent(totalLength);

        try
        {
            int pos = 0;

            // Copy prefix
            if (unmaskedStart > 0)
            {
                input.AsSpan(0, unmaskedStart).CopyTo(buffer.Slice(pos, unmaskedStart));
                pos += unmaskedStart;
            }

            // Fill middle with maskChar
            if (maskedSectionLength > 0)
            {
                buffer.Slice(pos, maskedSectionLength).Fill(maskChar);
                pos += maskedSectionLength;
            }

            // Copy suffix
            if (unmaskedEnd > 0)
            {
                input.AsSpan(length - unmaskedEnd, unmaskedEnd).CopyTo(buffer.Slice(pos, unmaskedEnd));
                pos += unmaskedEnd;
            }

            return new string(buffer.Slice(0, totalLength));
        }
        finally
        {
            if (totalLength > 256)
                ArrayPool<char>.Shared.Return(buffer.ToArray(), clearArray: true);
        }
    }
}
