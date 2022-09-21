using System.IO;
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
        value.ToString().ShouldEqual("&<>");
    }

    [Test]
    public void should_write_html()
    {
        var value = new HtmlString("&<>");
        var writer = new StringWriter();
        ((IEncodedContent)value).WriteTo(writer);
        writer.ToString().ShouldEqual("&<>");
    }
}
