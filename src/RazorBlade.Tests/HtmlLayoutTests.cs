using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RazorBlade.Tests.Support;

namespace RazorBlade.Tests;

[TestFixture]
public class HtmlLayoutTests
{
    [Test]
    public void should_render_layout()
    {
        var layout = new Layout(t =>
        {
            t.Write("before ");
            t.Write(t.RenderBody());
            t.Write(" after");
        });

        var page = new Template(t =>
        {
            t.Write("body");
            t.Layout = layout;
        });

        page.Render().ShouldEqual("before body after");
    }

    [Test]
    public void should_render_sections()
    {
        var layout = new Layout(t =>
        {
            t.Write("before ");
            t.Write(t.RenderSection("foo"));
            t.Write(" after");
        });

        var page = new Template(t =>
        {
            t.Layout = layout;
            t.SetSection("foo", "foo section");
        });

        page.Render().ShouldEqual("before foo section after");
    }

    [Test]
    public void should_render_nested_layouts()
    {
        var outerLayout = new Layout(t =>
        {
            t.Write("beforeA ");
            t.Write(t.RenderBody());
            t.Write(" afterA");
        });

        var innerLayout = new Layout(t =>
        {
            t.Write("beforeB ");
            t.Write(t.RenderBody());
            t.Write(" afterB");
            t.Layout = outerLayout;
        });

        var page = new Template(t =>
        {
            t.Write("body");
            t.Layout = innerLayout;
        });

        page.Render().ShouldEqual("beforeA beforeB body afterB afterA");
    }

    [Test]
    public void should_render_nested_layout_sections()
    {
        var outerLayout = new Layout(t =>
        {
            t.Write(t.RenderSection("inner"));
            t.Write(t.RenderSection("inner2", false));
            t.SetSection("outer", "outerSection ");
            t.Write(t.RenderSection("outer", false));
            t.Write(t.RenderBody());
        });

        var innerLayout = new Layout(t =>
        {
            t.Write(t.RenderSectionAsync("page").GetAwaiter().GetResult());
            t.Write(t.RenderSection("page2", false));
            t.SetSection("inner", "innerSection ");
            t.Write(t.RenderSection("inner", false));
            t.Write(t.RenderBody());
            t.Layout = outerLayout;
        });

        var page = new Template(t =>
        {
            t.SetSection("page", "pageSection");
            t.Layout = innerLayout;
        });

        page.Render().ShouldEqual("innerSection pageSection");
    }

    [Test]
    public void should_keep_the_layout_in_sections()
    {
        var sectionRendered = false;

        var layout = new Layout(t => t.Write(t.RenderSection("section")));

        var page = new Template(t =>
        {
            t.Layout = layout;
            t.DefineSection(
                "section",
                () =>
                {
                    t.Layout.ShouldBeTheSameAs(layout);
                    sectionRendered = true;
                    return Task.CompletedTask;
                }
            );
        });

        page.Render();
        sectionRendered.ShouldBeTrue();
    }

    [Test]
    public void should_throw_on_undefined_sections()
    {
        var layout = new Layout(t => t.Write(t.RenderSection("foo")));

        var page = new Template(t => t.Layout = layout);

        Assert.Throws<InvalidOperationException>(() => page.Render());
    }

    [Test]
    public void should_indicate_if_section_is_defined()
    {
        var layout = new Layout(t =>
        {
            t.IsSectionDefined("foo").ShouldBeTrue();
            t.IsSectionDefined("bar").ShouldBeFalse();
        });

        var page = new Template(t =>
        {
            t.Layout = layout;
            t.SetSection("foo", "foo");
        });

        page.Render();
        layout.WasExecuted.ShouldBeTrue();
    }

    [Test]
    public void should_throw_when_duplicate_section_is_defined()
    {
        var page = new Template(t =>
        {
            t.SetSection("foo", "foo");
            t.SetSection("foo", "foo");
        });

        Assert.Throws<InvalidOperationException>(() => page.Render());
    }

    [Test, Obsolete]
    public void should_throw_on_render()
    {
        var layout = new Layout(_ => { });

        Assert.Throws<InvalidOperationException>(() => layout.Render(CancellationToken.None));
        Assert.Throws<InvalidOperationException>(() => layout.Render(TextWriter.Null, CancellationToken.None));
        Assert.Throws<InvalidOperationException>(() => layout.RenderAsync(CancellationToken.None).GetAwaiter().GetResult());
        Assert.Throws<InvalidOperationException>(() => layout.RenderAsync(TextWriter.Null, CancellationToken.None).GetAwaiter().GetResult());
        Assert.Throws<InvalidOperationException>(() => ((RazorTemplate)layout).Render(CancellationToken.None));
    }

    private class Template : HtmlTemplate
    {
        private readonly Action<Template> _executeAction;

        public Template(Action<Template> executeAction)
            => _executeAction = executeAction;

        protected internal override Task ExecuteAsync()
        {
            _executeAction(this);
            return base.ExecuteAsync();
        }

        public void SetSection(string name, string content)
        {
            DefineSection(
                name,
                () =>
                {
                    Write(content);
                    return Task.CompletedTask;
                }
            );
        }
    }

    private class Layout : HtmlLayout
    {
        private readonly Action<Layout> _executeAction;

        public bool WasExecuted { get; private set; }

        public Layout(Action<Layout> executeAction)
            => _executeAction = executeAction;

        protected internal override Task ExecuteAsync()
        {
            _executeAction(this);
            WasExecuted = true;
            return base.ExecuteAsync();
        }

        public void SetSection(string name, string content)
        {
            DefineSection(
                name,
                () =>
                {
                    Write(content);
                    return Task.CompletedTask;
                }
            );
        }
    }
}
