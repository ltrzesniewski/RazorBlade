using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using RazorBlade.Tests.Support;

namespace RazorBlade.Tests;

[TestFixture]
public class PlainTextTemplateTests
{
    [Test]
    public void should_not_escape_html_characters()
    {
        var template = new Template(t => t.Write("foo & bar < baz > foobar"));
        template.Render().ShouldEqual("foo & bar < baz > foobar");
    }

    [Test]
    public void should_write_IEncodedContent()
    {
        var template = new Template(_ => { });
        var encodedContent = new TestEncodedContent();
        template.Write(encodedContent);
        template.Output.ToString().ShouldEqual("<br>");
    }

    [Test]
    public void should_write_IEncodedContent_as_object()
    {
        var template = new Template(_ => { });
        var encodedContent = new TestEncodedContent();
        template.Write((object)encodedContent);
        template.Output.ToString().ShouldEqual("<br>");
    }

    private class Template : PlainTextTemplate
    {
        private readonly Action<Template> _executeAction;

        public Template(Action<Template> executeAction)
        {
            _executeAction = executeAction;
            Output = new StringWriter();
        }

        protected internal override Task ExecuteAsync()
        {
            _executeAction(this);
            return base.ExecuteAsync();
        }
    }

    private class TestEncodedContent : IEncodedContent
    {
        public void WriteTo(TextWriter textWriter)
            => textWriter.Write("<br>");
    }
}
