using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using RazorBlade.Tests.Support;

namespace RazorBlade.Tests;

[TestFixture]
public class RazorTemplateTests
{
    [Test]
    public async Task should_write_literal()
    {
        var template = new Template(t => t.WriteLiteral("foo & bar < baz > foobar"));
        await template.ExecuteAsync();
        template.Output.ToString().ShouldEqual("foo & bar < baz > foobar");
    }

    [Test]
    public void should_render_to_local_output()
    {
        var template = new Template(t => t.WriteLiteral("foo"));
        template.Output.Write("bar");

        template.Render().ShouldEqual("foo");
        template.Output.ToString().ShouldEqual("bar");
    }

    [Test]
    public void should_render_to_text_writer()
    {
        var template = new Template(t => t.WriteLiteral("foo"));
        template.Output.Write("bar");

        var output = new StringWriter();
        template.Render(output);
        output.ToString().ShouldEqual("foo");

        template.Output.ToString().ShouldEqual("bar");
    }

    [Test]
    public async Task should_render_async_to_local_output()
    {
        var template = new Template(t => t.WriteLiteral("foo"));
        await template.Output.WriteAsync("bar");

        (await template.RenderAsync()).ShouldEqual("foo");
        template.Output.ToString().ShouldEqual("bar");
    }

    [Test]
    public async Task should_render_async_to_text_writer()
    {
        var template = new Template(t => t.WriteLiteral("foo"));
        await template.Output.WriteAsync("bar");

        var output = new StringWriter();
        await template.RenderAsync(output);
        output.ToString().ShouldEqual("foo");

        template.Output.ToString().ShouldEqual("bar");
    }

    private class Template : RazorTemplate
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
}
