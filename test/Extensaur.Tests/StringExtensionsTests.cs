#nullable enable

namespace Extensaur.Tests;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("Hello World", 5, "...", "He...")]
    [InlineData("Hello", 10, "...", "Hello")]
    [InlineData(null, 5, "...", null)]
    [InlineData("", 5, "...", "")]
    [InlineData("Hello", 2, "...", "He")]
    [InlineData("Hello", 3, null, "Hel")]
    [InlineData("Hello", 5, "...", "Hello")]
    [InlineData("HelloWorld", 5, "...", "He...")]
    [InlineData("HelloWorld", 10, "...", "HelloWorld")]
    [InlineData("HelloWorld", 8, "...", "Hello...")]
    [InlineData("HelloWorld", 2, "...", "He")]
    [InlineData("HelloWorld", 0, "...", "")]
    [InlineData("HelloWorld", 3, null, "Hel")]
    [InlineData("HelloWorld", 8, "--", "HelloW--")]
    [InlineData("Hello", 3, "...", "Hel")]
    [InlineData("Short", 10, "...", "Short")]
    [InlineData("This is a long string of words.", 10, "...", "This is...")]
    public void TruncateTests(string? input, int keep, string? ellipsis, string? expected)
    {
        var result = StringExtensions.Truncate(input, keep, ellipsis);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData(" ", false)]
    [InlineData("abc", false)]
    public void IsNullOrEmptyTests(string? input, bool expected)
    {
        Assert.Equal(expected, StringExtensions.IsNullOrEmpty(input));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData("abc", false)]
    [InlineData(" a ", false)]
    public void IsNullOrWhiteSpaceTests(string? input, bool expected)
    {
        Assert.Equal(expected, StringExtensions.IsNullOrWhiteSpace(input));
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("abc", true)]
    public void HasValueTests(string? input, bool expected)
    {
        Assert.Equal(expected, StringExtensions.HasValue(input));
    }

    [Fact]
    public void FormatWithoutProvider()
    {
        var result = "{0} {1}".FormatWith("Hello", "World");
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void FormatWithProvider()
    {
        var result = "{0:C}".FormatWith(System.Globalization.CultureInfo.GetCultureInfo("en-US"), 12.5);
        Assert.Equal("$12.50", result);
    }

    [Theory]
    [InlineData("NameIdentifier", "Name Identifier")]
    [InlineData("name-identifier", "Name Identifier")]
    [InlineData("name--identifier", "Name Identifier")]
    [InlineData("Name1Identifier", "Name 1 Identifier")]
    [InlineData("Name123Identifier", "Name 123 Identifier")]
    [InlineData("Name_identifier", "Name Identifier")]
    [InlineData("XMLHttpRequest2", "XML Http Request 2")]
    [InlineData("XML_HttpRequest2", "XML Http Request 2")]
    [InlineData("PDFName", "PDF Name")]
    [InlineData("IP4Address", "IP 4 Address")]
    [InlineData("A", "A")]
    [InlineData("", "")]
    [InlineData(null, null)]
    public void ToTitleTests(string? input, string? expected)
    {
        var result = StringExtensions.ToTitle(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("hello world", "HelloWorld")]
    [InlineData("Hello World", "HelloWorld")]
    [InlineData("hello_world", "HelloWorld")]
    [InlineData("hello-world", "HelloWorld")]
    [InlineData("helloWorld", "HelloWorld")]
    [InlineData("XML_http_request", "XmlHttpRequest")]
    [InlineData("name123 identifier", "Name123Identifier")]
    [InlineData("alreadyPascalCase", "AlreadyPascalCase")]
    [InlineData("a", "A")]
    public void ToPascalCaseTests(string? input, string? expected)
    {
        var result = StringExtensions.ToPascalCase(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("hello", "hello")]
    [InlineData("Hello", "hello")]
    [InlineData("HelloWorld", "helloWorld")]
    [InlineData("helloWorld", "helloWorld")]
    [InlineData("hello world", "helloWorld")]
    [InlineData("Hello World", "helloWorld")]
    [InlineData("hello_world", "helloWorld")]
    [InlineData("hello-world", "helloWorld")]
    [InlineData("name123 identifier", "name123Identifier")]
    [InlineData("alreadyCamelCase", "alreadyCamelCase")]
    [InlineData("A", "a")]
    [InlineData("test123", "test123")]
    [InlineData("Test123", "test123")]
    public void ToCamelCaseTests(string? input, string? expected)
    {
        var result = StringExtensions.ToCamelCase(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("hello", "hello")]
    [InlineData("Hello", "hello")]
    [InlineData("HelloWorld", "hello-world")]
    [InlineData("helloWorld", "hello-world")]
    [InlineData("hello world", "hello-world")]
    [InlineData("Hello World", "hello-world")]
    [InlineData("hello_world", "hello-world")]
    [InlineData("hello-world", "hello-world")]
    [InlineData("name123 identifier", "name-123-identifier")]
    [InlineData("alreadyCamelCase", "already-camel-case")]
    [InlineData("XML_http_request", "xml-http-request")]
    [InlineData("XMLHttpRequest2", "xml-http-request-2")]
    [InlineData("A", "a")]
    [InlineData("test123", "test-123")]
    [InlineData("Test123", "test-123")]
    public void ToKebabCaseTests(string? input, string? expected)
    {
        var result = StringExtensions.ToKebabCase(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("hello", "hello")]
    [InlineData("Hello", "hello")]
    [InlineData("HelloWorld", "hello_world")]
    [InlineData("helloWorld", "hello_world")]
    [InlineData("hello world", "hello_world")]
    [InlineData("Hello World", "hello_world")]
    [InlineData("hello_world", "hello_world")]
    [InlineData("hello-world", "hello_world")]
    [InlineData("name123 identifier", "name_123_identifier")]
    [InlineData("alreadyCamelCase", "already_camel_case")]
    [InlineData("XML_http_request", "xml_http_request")]
    [InlineData("XMLHttpRequest2", "xml_http_request_2")]
    [InlineData("A", "a")]
    [InlineData("test123", "test_123")]
    [InlineData("Test123", "test_123")]
    public void ToSnakeCaseTests(string? input, string? expected)
    {
        var result = StringExtensions.ToSnakeCase(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, null, '/', null)]
    [InlineData("foo", null, '/', "foo")]
    [InlineData(null, "bar", '/', "bar")]
    [InlineData("foo", "bar", '/', "foo/bar")]
    [InlineData("foo/", "bar", '/', "foo/bar")]
    [InlineData("foo", "/bar", '/', "foo/bar")]
    [InlineData("foo/", "/bar", '/', "foo/bar")]
    [InlineData("foo", "bar", '-', "foo-bar")]
    public void CombineTests(string? first, string? second, char separator, string? expected)
    {
        var result = StringExtensions.Combine(first, second, separator);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("SensitiveData1234", 2, 4, '*', null, "Se***********1234")]
    [InlineData("CreditCardNumber", 0, 4, '#', null, "############mber")]
    [InlineData("abcdef", 1, 1, '*', null, "a****f")]
    [InlineData("abcdef", 0, 0, '*', null, "******")]
    [InlineData("abcdef", 3, 0, '*', null, "abc***")]
    [InlineData("abcdef", 0, 3, '*', null, "***def")]
    [InlineData("abcdef", 6, 0, '*', null, "abcdef")]
    [InlineData("abcdef", 0, 6, '*', null, "abcdef")]
    [InlineData("", 0, 0, '*', null, "")]
    [InlineData(null, 0, 0, '*', null, null)]
    // Tests with maskedCount specified
    [InlineData("SensitiveData1234", 2, 4, '*', 5, "Se*****1234")]
    [InlineData("CreditCardNumber", 0, 4, '#', 5, "#####mber")]
    [InlineData("abcdef", 1, 1, '*', 2, "a**f")]
    [InlineData("abcdef", 1, 1, '*', 0, "af")]
    [InlineData("abcdef", 1, 1, '*', 10, "a**********f")]
    [InlineData("abcdef", 0, 0, '*', 3, "***")]
    [InlineData("abcdef", 3, 0, '*', 1, "abc*")]
    [InlineData("abcdef", 0, 3, '*', 2, "**def")]
    [InlineData("abcdef", 6, 0, '*', 5, "abcdef*****")]
    [InlineData("abcdef", 0, 6, '*', 5, "*****abcdef")]
    [InlineData("", 0, 0, '*', 2, "")]
    [InlineData(null, 0, 0, '*', 2, null)]
    public void MaskTests(string? input, int unmaskedStart, int unmaskedEnd, char maskChar, int? maskedCount, string? expected)
    {
        var result = maskedCount.HasValue
            ? StringExtensions.Mask(input, unmaskedStart, unmaskedEnd, maskChar, maskedCount.Value)
            : StringExtensions.Mask(input, unmaskedStart, unmaskedEnd, maskChar);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Mask_Throws_When_Unmasked_Exceeds_Length()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => StringExtensions.Mask("abc", 2, 2));
    }

    [Fact]
    public void Mask_Throws_When_Negative_Unmasked_Start()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => StringExtensions.Mask("abc", -1, 1));
    }

    [Fact]
    public void Mask_Standard_Values()
    {
        // mask credit card number
        string maskedCreditCard = "4111111111111111".Mask(4, 4);
        Assert.Equal("4111********1111", maskedCreditCard);

        // mask a social security number
        var maskedSocial = "123-45-6789".Mask(0, 4, '*', 5);
        Assert.Equal("*****6789", maskedSocial);

        // mask a password
        var maskedPassword = "P@ssw0rd123".Mask(0, 0, '*', 8);
        Assert.Equal("********", maskedPassword);
    }
}
