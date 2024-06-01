using Snork.JsonCommentStripper.Model;
using Xunit;

namespace Snork.JsonCommentStripper.Tests;

public class UnitTests
{
    private const string Esc1 = "{\"\\\\\":\"https://foobar.com\"}";
    private const string Esc2 = "{\"foo\\\"\":\"https://foobar.com\"}";

    private const string SingleQuotedString = "'Hello, \"Bob\"! Here\\'s stuff in {curlybraces} and [straight braces]! '";
    private const string DoubleQuotedString = "\"Hello, 'Sam'! Here's stuff in {curlybraces} and [straight braces]! \"";

    [Theory]
    [InlineData("//comment\n{\"a\":\"b\"}", "         \n{\"a\":\"b\"}")]
    [InlineData("/*//comment*/{\"a\":\"b\"}", "             {\"a\":\"b\"}")]
    [InlineData("{\"a\":\"b\"//comment\n}", "{\"a\":\"b\"         \n}")]
    [InlineData("{\"a\":\"b\"/*comment*/}", "{\"a\":\"b\"           }")]
    [InlineData("{\"a\"/*\n\n\ncomment\r\n*/:\"b\"}", "{\"a\"  \n\n\n       \r\n  :\"b\"}")]
    [InlineData("/*!\n * comment\n */\n{\"a\":\"b\"}", "   \n          \n   \n{\"a\":\"b\"}")]
    [InlineData("{/*comment*/\"a\":\"b\"}", "{           \"a\":\"b\"}")]
    [InlineData("//comment\n{'a':'b'}", "         \n{'a':'b'}")]
    [InlineData("/*//comment*/{'a':'b'}", "             {'a':'b'}")]
    [InlineData("{'a':'b'//comment\n}", "{'a':'b'         \n}")]
    [InlineData("{'a':'b'/*comment*/}", "{'a':'b'           }")]
    [InlineData("{'a'/*\n\n\ncomment\r\n*/:'b'}", "{'a'  \n\n\n       \r\n  :'b'}")]
    [InlineData("/*!\n * comment\n */\n{'a':'b'}", "   \n          \n   \n{'a':'b'}")]
    [InlineData("{/*comment*/'a':'b'}", "{           'a':'b'}")]
    public void TestReplaceCommentsWithWhiteSpace(string input, string expectedOutput)
    {
        var test = Stripper.Execute(input);
        Assert.Equal(expectedOutput, test);
    }

    [Theory]
    [InlineData("//comment\n{\"a\":\"b\"}", "\n{\"a\":\"b\"}")]
    [InlineData("/*//comment*/{\"a\":\"b\"}", "{\"a\":\"b\"}")]
    [InlineData("{\"a\":\"b\"//comment\n}", "{\"a\":\"b\"\n}")]
    [InlineData("{\"a\":\"b\"/*comment*/}", "{\"a\":\"b\"}")]
    [InlineData("{\"a\"/*\n\n\ncomment\r\n*/:\"b\"}", "{\"a\":\"b\"}")]
    [InlineData("/*!\n * comment\n */\n{\"a\":\"b\"}", "\n{\"a\":\"b\"}")]
    [InlineData("{/*comment*/\"a\":\"b\"}", "{\"a\":\"b\"}")]
    [InlineData("//comment\n{'a':'b'}", "\n{'a':'b'}")]
    [InlineData("/*//comment*/{'a':'b'}", "{'a':'b'}")]
    [InlineData("{'a':'b'//comment\n}", "{'a':'b'\n}")]
    [InlineData("{'a':'b'/*comment*/}", "{'a':'b'}")]
    [InlineData("{'a'/*\n\n\ncomment\r\n*/:'b'}", "{'a':'b'}")]
    [InlineData("/*!\n * comment\n */\n{'a':'b'}", "\n{'a':'b'}")]
    [InlineData("{/*comment*/'a':'b'}", "{'a':'b'}")]
    public void TestRemoveComments(string input, string expectedOutput)
    {
        var test = Stripper.Execute(input, new StripperOptions { ReplaceWithWhiteSpace = false });
        Assert.Equal(expectedOutput, test);
    }


    [Theory]
    [InlineData("{\"a\":\"b//c\"}", "{\"a\":\"b//c\"}")]
    [InlineData("{\"a\":\"b/*c*/\"}", "{\"a\":\"b/*c*/\"}")]
    [InlineData("{\"/*a\":\"b\"}", "{\"/*a\":\"b\"}")]
    [InlineData("{\"\\\"/*a\":\"b\"}", "{\"\\\"/*a\":\"b\"}")]
    [InlineData("{'a':'b//c'}", "{'a':'b//c'}")]
    [InlineData("{'a':'b/*c*/'}", "{'a':'b/*c*/'}")]
    [InlineData("{'/*a':'b'}", "{'/*a':'b'}")]
    [InlineData("{'\\'/*a':'b'}", "{'\\'/*a':'b'}")]
    public void TestDontStripCommentsInsideStrings(string input, string expectedOutput)
    {
        var test = Stripper.Execute(input);
        Assert.Equal(expectedOutput, test);
    }

    [Theory]
    [InlineData(Esc1)]
    [InlineData(Esc2)]
    public void TestConsiderEscapedSlashesWhenCheckingforEscapedStringQuote(string input)
    {
        var test = Stripper.Execute(input);
        Assert.Equal(input, test);
    }

    [Theory]
    [InlineData("{\"a\":\"b\"\n}", "{\"a\":\"b\"\n}")]
    [InlineData("{\"a\":\"b\"\r\n}", "{\"a\":\"b\"\r\n}")]
    [InlineData("{'a':'b'\n}", "{'a':'b'\n}")]
    [InlineData("{'a':'b'\r\n}", "{'a':'b'\r\n}")]
    public void TestLineEndingsNoComments(string input, string expectedOutput)
    {
        var test = Stripper.Execute(input);
        Assert.Equal(expectedOutput, test);
    }

    [Theory]
    [InlineData("{\"a\":\"b\"//c\n}", "{\"a\":\"b\"   \n}")]
    [InlineData("{\"a\":\"b\"//c\r\n}", "{\"a\":\"b\"   \r\n}")]
    [InlineData("{'a':'b'//c\n}", "{'a':'b'   \n}")]
    [InlineData("{'a':'b'//c\r\n}", "{'a':'b'   \r\n}")]
    public void TestLineEndingsSingleLineComment(string input, string expectedOutput)
    {
        var test = Stripper.Execute(input);
        Assert.Equal(expectedOutput, test);
    }

    [Theory]
    [InlineData("{\"a\":\"b\"/*c*/\n}", "{\"a\":\"b\"     \n}")]
    [InlineData("{\"a\":\"b\"/*c*/\r\n}", "{\"a\":\"b\"     \r\n}")]
    [InlineData("{'a':'b'/*c*/\n}", "{'a':'b'     \n}")]
    [InlineData("{'a':'b'/*c*/\r\n}", "{'a':'b'     \r\n}")]
    public void TestLineEndingsSingleLineBlockComment(string input, string expectedOutput)
    {
        var test = Stripper.Execute(input);
        Assert.Equal(expectedOutput, test);
    }

    [Theory]
    [InlineData("{\"a\":\"b\",/*c\nc2*/\"x\":\"y\"\n}", "{\"a\":\"b\",   \n    \"x\":\"y\"\n}")]
    [InlineData("{\"a\":\"b\",/*c\r\nc2*/\"x\":\"y\"\r\n}", "{\"a\":\"b\",   \r\n    \"x\":\"y\"\r\n}")]
    [InlineData("{'a':'b',/*c\nc2*/'x':'y'\n}", "{'a':'b',   \n    'x':'y'\n}")]
    [InlineData("{'a':'b',/*c\r\nc2*/'x':'y'\r\n}", "{'a':'b',   \r\n    'x':'y'\r\n}")]
    public void TestLineEndingsMultiLineBlockComment(string input, string expectedOutput)
    {
        var test = Stripper.Execute(input);
        Assert.Equal(expectedOutput, test);
    }

    [Theory]
    [InlineData("{\r\n\t\"a\":\"b\"\r\n} //EOF", "{\r\n\t\"a\":\"b\"\r\n}      ", true)]
    [InlineData("{\r\n\t\"a\":\"b\"\r\n} //EOF", "{\r\n\t\"a\":\"b\"\r\n} ", false)]
    [InlineData("{\r\n\t'a':'b'\r\n} //EOF", "{\r\n\t'a':'b'\r\n}      ", true)]
    [InlineData("{\r\n\t'a':'b'\r\n} //EOF", "{\r\n\t'a':'b'\r\n} ", false)]
    [InlineData($"{SingleQuotedString} //EOF", $"{SingleQuotedString}      ", true)]
    [InlineData($"{DoubleQuotedString} //EOF", $"{DoubleQuotedString} ", false)]
    public void TestLineEndingsWorksAtEOF(string input, string expectedOutput, bool replaceWithWhiteSpace)
    {
        var test = Stripper.Execute(input, new StripperOptions { ReplaceWithWhiteSpace = replaceWithWhiteSpace });
        Assert.Equal(expectedOutput, test);
    }

    [Theory]
    [InlineData(
        "{\"x\":\"x \\\"sed -e \\\\\\\"s/^.\\\\\\\\{46\\\\\\\\}T//\\\\\\\" -e \\\\\\\"s/#033/\\\\\\\\x1b/g\\\\\\\"\\\"\"}")]
    [InlineData(
        "{'x':'x \\'sed -e \\\\\\'s/^.\\\\\\\\{46\\\\\\\\}T//\\\\\\' -e \\\\\\'s/#033/\\\\\\\\x1b/g\\\\\\'\\''}")]
    public void TestHandlesWeirdEscaping(string input)
    {
        var test = Stripper.Execute(input);
        Assert.Equal(input, test);
    }

    [Theory]
    [InlineData("{\"x\":true,}", "{\"x\":true }", true, true)]
    [InlineData("{\"x\":true,}", "{\"x\":true}", true, false)]
    [InlineData("{\"x\":true,\n  }", "{\"x\":true \n  }", true, true)]
    [InlineData("[true, false,]", "[true, false ]", true, true)]
    [InlineData("[true, false,]", "[true, false]", true, false)]
    [InlineData("{\n  \"array\": [\n    true,\n    false,\n  ],\n}", "{\n  \"array\": [\n    true,\n    false\n  ]\n}",
        true, false)]
    [InlineData("{\n  \"array\": [\n    true,\n    false /* comment */ ,\n /*comment*/ ],\n}",
        "{\n  \"array\": [\n    true,\n    false  \n  ]\n}", true, false)]
    [InlineData("{'x':true,}", "{'x':true }", true, true)]
    [InlineData("{'x':true,}", "{'x':true}", true, false)]
    [InlineData("{'x':true,\n  }", "{'x':true \n  }", true, true)]
    [InlineData("[true, false,]", "[true, false ]", true, true)]
    [InlineData("[true, false,]", "[true, false]", true, false)]
    [InlineData("{\n  'array': [\n    true,\n    false,\n  ],\n}", "{\n  'array': [\n    true,\n    false\n  ]\n}",
        true, false)]
    [InlineData("{\n  'array': [\n    true,\n    false /* comment */ ,\n /*comment*/ ],\n}",
        "{\n  'array': [\n    true,\n    false  \n  ]\n}", true, false)]
    public void TestStripsTrailingCommas(string input, string expectedOutput, bool trailingCommas,
        bool replaceWithWhiteSpace)
    {
        var test = Stripper.Execute(input,
            new StripperOptions
                { ReplaceWithWhiteSpace = replaceWithWhiteSpace, StripTrailingCommas = trailingCommas });
        Assert.Equal(expectedOutput, test);
    }

    [Theory]
    [InlineData("[] */", "[] */")]
    public void TestHandlesMalformedBlockComments(string input, string expectedOutput)
    {
        var test = Stripper.Execute(input);
        Assert.Equal(expectedOutput, test);
    }

    [Theory]
    [InlineData("[] /*", "[]   ")]
    [InlineData("{a:1} /*", "{a:1}   ")]
    [InlineData($"{SingleQuotedString} /*", $"{SingleQuotedString}   ")]
    [InlineData($"{DoubleQuotedString} /*", $"{DoubleQuotedString}   ")]
    public void TestOpenedMultiblockOnEndGetsRemoved(string input, string expectedOutput)
    {
        var test = Stripper.Execute(input);
        Assert.Equal(expectedOutput, test);
    }
}