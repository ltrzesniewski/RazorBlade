using NUnit.Framework;
using RazorBlade.Tests.Support;

namespace RazorBlade.Tests;

[TestFixture]
public class HtmlStringTests
{
    [Test]
    public void should_not_escape_html()
    {
        var value = new HtmlString("&<>");
        value.ToHtmlString().ShouldEqual("&<>");
    }
}
