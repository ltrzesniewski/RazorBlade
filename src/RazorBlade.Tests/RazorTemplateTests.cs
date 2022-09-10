using System;
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
    public async Task should_render_async_to_local_output()
    {
        var template = new Template(t => t.WriteLiteral("foo"));
        await template.Output.WriteAsync("bar");

        (await template.RenderAsync()).ShouldEqual("foo");
        template.Output.ToString().ShouldEqual("bar");
    }

    [Test]
    public void should_return_output_as_string()
    {
        var template = new Template(_ => { });
        template.Output.Write("foo");
        template.ToString().ShouldEqual("foo");
    }

    private class Template : RazorTemplate
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
