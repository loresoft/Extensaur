using System.Text;

namespace Extensaur.Tests;


public class NameFormatterTest
{
    [Fact]
    public void StringFormat_WithMultipleExpressions()
    {
        var o = new { First = "John", Last = "Doe" };
        string result = NameFormatter.FormatName("Full Name: {First} {Last}", o);

        //assert
        Assert.Equal("Full Name: John Doe", result);
    }

    [Fact]
    public void StringFormat_WithMultipleExpressions_NullProperty()
    {
        //arrange
        var o = new { foo = 123.45, bar = 42, baz = (string)null };

        //act
        string result = NameFormatter.FormatName("{foo} {foo} {bar}{baz}", o);

        //assert
        Assert.Equal("123.45 123.45 42", result);
    }

    [Fact]
    public void StringFormat_WithMultipleExpressions_FormatsThemAll()
    {
        //arrange
        var o = new { foo = 123.45, bar = 42, baz = "hello" };

        //act
        string result = NameFormatter.FormatName("{foo} {foo} {bar}{baz}", o);

        //assert
        Assert.Equal("123.45 123.45 42hello", result);
    }

    [Fact]
    public void StringFormat_WithDoubleEscapedCurlyBraces_DoesNotFormatString()
    {
        //arrange
        var o = new { foo = 123.45 };

        //act
        string result = NameFormatter.FormatName("{{{{foo}}}}", o);

        //assert
        Assert.Equal("{{foo}}", result);
    }

    [Fact]
    public void StringFormat_WithFormatSurroundedByDoubleEscapedBraces_FormatsString()
    {
        //arrange
        var o = new { foo = 123.45 };

        //act
        string result = NameFormatter.FormatName("{{{{{foo}}}}}", o);

        //assert
        Assert.Equal("{{123.45}}", result);
    }

    [Fact]
    public void Format_WithEscapeSequence_EscapesInnerCurlyBraces()
    {
        var o = new { foo = 123.45 };

        //act
        string result = NameFormatter.FormatName("{{{foo}}}", o);

        //assert
        Assert.Equal("{123.45}", result);
    }

    [Fact]
    public void Format_WithEmptyString_ReturnsEmptyString()
    {
        var o = new { foo = 123.45 };

        //act
        string result = NameFormatter.FormatName(string.Empty, o);

        //assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Format_WithNoFormats_ReturnsFormatStringAsIs()
    {
        var o = new { foo = 123.45 };

        //act
        string result = NameFormatter.FormatName("a b c", o);

        //assert
        Assert.Equal("a b c", result);
    }

    [Fact]
    public void Format_WithFormatType_ReturnsFormattedExpression()
    {
        var o = new { foo = 123.45 };

        //act
        string result = NameFormatter.FormatName("{foo:#.#}", o);

        //assert
        Assert.Equal("123.5", result);
    }

    [Fact]
    public void Format_WithSubProperty_ReturnsValueOfSubProperty()
    {
        var o = new { foo = new { bar = 123.45 } };

        //act
        string result = NameFormatter.FormatName("{foo.bar:#.#}ms", o);

        //assert
        Assert.Equal("123.5ms", result);
    }

    [Fact]
    public void Format_WithFormatNameNotInObject()
    {
        //arrange
        var o = new { foo = 123.45 };

        //act
        string result = NameFormatter.FormatName("This Value:{bar}", o);

        //assert
        Assert.Equal("This Value:", result);
    }

    [Fact]
    public void Format_WithNoEndFormatBrace_ThrowsFormatException()
    {
        //arrange
        var o = new { foo = 123.45 };

        //act, assert
        Assert.Throws<FormatException>(() => NameFormatter.FormatName("{bar", o));
    }

    [Fact]
    public void Format_WithEscapedEndFormatBrace_ThrowsFormatException()
    {
        //arrange
        var o = new { foo = 123.45 };


        //act, assert
        Assert.Throws<FormatException>(() => NameFormatter.FormatName("{foo}}", o));
    }

    [Fact]
    public void Format_WithDoubleEscapedEndFormatBrace_ThrowsFormatException()
    {
        //arrange
        var o = new { foo = 123.45 };

        //act, assert
        Assert.Throws<FormatException>(() => NameFormatter.FormatName("{foo}}}}bar", o));
    }

    [Fact]
    public void Format_WithDoubleEscapedEndFormatBraceWhichTerminatesString_ThrowsFormatException()
    {
        //arrange
        var o = new { foo = 123.45 };

        //act, assert
        Assert.Throws<FormatException>(() => NameFormatter.FormatName("{foo}}}}", o));
    }

    [Fact]
    public void Format_WithEndBraceFollowedByEscapedEndFormatBraceWhichTerminatesString_FormatsCorrectly()
    {
        var o = new { foo = 123.45 };

        //act
        string result = NameFormatter.FormatName("{foo}}}", o);

        //assert
        Assert.Equal("123.45}", result);
    }

    [Fact]
    public void Format_WithEndBraceFollowedByEscapedEndFormatBrace_FormatsCorrectly()
    {
        var o = new { foo = 123.45 };

        //act
        string result = NameFormatter.FormatName("{foo}}}bar", o);

        //assert
        Assert.Equal("123.45}bar", result);
    }

    [Fact]
    public void Format_WithEndBraceFollowedByDoubleEscapedEndFormatBrace_FormatsCorrectly()
    {
        var o = new { foo = 123.45 };

        //act
        string result = NameFormatter.FormatName("{foo}}}}}bar", o);

        //assert
        Assert.Equal("123.45}}bar", result);
    }

    [Fact]
    public void Format_WithNullFormatString_ThrowsArgumentNullException()
    {
        //arrange, act, assert
        Assert.Throws<ArgumentNullException>(() => NameFormatter.FormatName(null, 123));
    }



    [Fact]
    public void StringFormat_WithDictionaryMultipleExpressions()
    {
        var o = new Dictionary<string, string> { ["First"] = "John", ["Last"] = "Doe" };
        string result = NameFormatter.FormatName("Full Name: {First} {Last}", o);

        //assert
        Assert.Equal("Full Name: John Doe", result);
    }

    [Fact]
    public void StringFormat_WithDictionaryMultipleExpressions_NullProperty()
    {
        //arrange
        var o = new Dictionary<string, object> { ["foo"] = 123.45, ["bar"] = 42, ["baz"] = null };

        //act
        string result = NameFormatter.FormatName("{foo} {foo} {bar}{baz}", o);

        //assert
        Assert.Equal("123.45 123.45 42", result);
    }


    [Fact]
    public void Format_WithDictionaryFormatType_ReturnsFormattedExpression()
    {
        var o = new { foo = 123.45 };

        //act
        string result = NameFormatter.FormatName("{foo:#.#}", o);

        //assert
        Assert.Equal("123.5", result);
    }

}
