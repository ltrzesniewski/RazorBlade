using NUnit.Framework;
using RazorBlade.Tests.Support;

namespace RazorBlade.Tests;

[TestFixture]
public class HtmlHelperTests
{
    [Test]
    public void should_return_html_string()
    {
        var result = HtmlHelper.Instance.Raw("&<>");
        result.ToHtmlString().ShouldEqual("&<>");
    }

    [Test]
    [TestCase("", "")]
    [TestCase("foo", "foo")]
    [TestCase("&foo", "&amp;foo")]
    [TestCase("foo&", "foo&amp;")]
    [TestCase("foo & bar < baz > foobar", "foo &amp; bar &lt; baz &gt; foobar")]
    public void should_html_encode_string(string input, string expectedOutput)
    {
        HtmlHelper.Instance.Encode(input).ShouldEqual(expectedOutput);
    }
}
