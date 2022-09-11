using System;
using System.Threading.Tasks;
using NUnit.Framework;
using RazorBlade.Tests.Support;

namespace RazorBlade.Tests;

[TestFixture]
public class HtmlTemplateTests
{
    [Test]
    [TestCase("", "")]
    [TestCase("foo", "foo")]
    [TestCase("&foo", "&amp;foo")]
    [TestCase("foo&", "foo&amp;")]
    [TestCase("foo & bar < baz > foobar", "foo &amp; bar &lt; baz &gt; foobar")]
    public void should_escape_special_characters(string input, string expectedOutput)
    {
        var template = new Template(t => t.Write(input));
        template.Render().ShouldEqual(expectedOutput);
    }

    [Test]
    public void should_not_escape_IHtmlString()
    {
        var template = new Template(_ => { });
        var htmlString = new TestHtmlString();
        template.Write(htmlString);
        template.Output.ToString().ShouldEqual(htmlString.ToHtmlString());
    }

    private class Template : HtmlTemplate
    {
        private readonly Action<Template> _executeAction;

        public Template(Action<Template> executeAction)
        {
            _executeAction = executeAction;
        }

        public override Task ExecuteAsync()
        {
            _executeAction(this);
            return base.ExecuteAsync();
        }
    }

    private class TestHtmlString : IHtmlString
    {
        public string ToHtmlString()
            => "<br>";
    }
}
