using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
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

    [Test]
    public async Task should_compose_templates()
    {
        var templateFoo = new Template(t => t.WriteLiteral("foo"));
        var templateBar = new Template(t =>
        {
            t.Write(templateFoo);
            t.WriteLiteral("bar");
        });

        var result = await templateBar.RenderAsync();
        result.ShouldEqual("foobar");
    }

    [Test]
    public void should_write_encoded_content()
    {
        var template = new Template(t => t.WriteLiteral("foo & bar"));
        var writer = new StringWriter();
        ((IEncodedContent)template).WriteTo(writer);
        writer.ToString().ShouldEqual("foo & bar");
    }

    [Test, Timeout(10_000)]
    public async Task should_support_cancellation()
    {
        var cts = new CancellationTokenSource();
        var semaphore = new SemaphoreSlim(0);

        var template = new Template(async t =>
        {
            semaphore.Release();
            await Task.Delay(Timeout.InfiniteTimeSpan, t.CancellationToken);
        });

        var task = template.RenderAsync(cts.Token);

        // Make sure we enter the template execution before we cancel
        await semaphore.WaitAsync(CancellationToken.None);
        cts.Cancel();

        var exception = Assert.CatchAsync<OperationCanceledException>(async () => await task);
        task.IsCanceled.ShouldBeTrue();
        exception.ShouldNotBeNull().CancellationToken.ShouldEqual(cts.Token);
    }

    [Test]
    public void should_not_execute_template_when_already_cancelled()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var semaphore = new SemaphoreSlim(0);
        var template = new Template(_ => semaphore.Release());

        Assert.Catch<OperationCanceledException>(() => template.Render(cts.Token));
        Assert.Catch<OperationCanceledException>(() => template.Render(StreamWriter.Null, cts.Token));
        Assert.CatchAsync<OperationCanceledException>(() => template.RenderAsync(cts.Token));
        Assert.CatchAsync<OperationCanceledException>(() => template.RenderAsync(StreamWriter.Null, cts.Token));

        semaphore.CurrentCount.ShouldEqual(0);
    }

    private class Template : RazorTemplate
    {
        private readonly Func<Template, Task> _executeAction;

        public Template(Func<Template, Task> executeAction)
        {
            _executeAction = executeAction;
            Output = new StringWriter();
        }

        public Template(Action<Template> executeAction)
            : this(t =>
            {
                executeAction(t);
                return Task.CompletedTask;
            })
        {
        }

        protected internal override async Task ExecuteAsync()
        {
            await _executeAction(this);
            await base.ExecuteAsync();
        }

        protected internal override void Write(object? value)
        {
        }

        protected internal override void BeginWriteAttribute(string name, string prefix, int prefixOffset, string suffix, int suffixOffset, int attributeValuesCount)
        {
        }

        protected internal override void WriteAttributeValue(string prefix, int prefixOffset, object? value, int valueOffset, int valueLength, bool isLiteral)
        {
        }

        protected internal override void EndWriteAttribute()
        {
        }
    }
}
