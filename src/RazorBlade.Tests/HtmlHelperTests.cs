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
}
