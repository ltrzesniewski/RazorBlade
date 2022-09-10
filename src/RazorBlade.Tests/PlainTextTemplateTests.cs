using System;
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

    private class Template : PlainTextTemplate
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
}
