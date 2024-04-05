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

        var result = await template.RenderAsync();

        result.ShouldEqual("foo & bar < baz > foobar");
    }

    [Test]
    public void should_render_to_local_output()
    {
        var template = new Template(t => t.WriteLiteral("foo"));
        template.Output.ShouldEqual(TextWriter.Null);

        template.Render().ShouldEqual("foo");

        template.Output.ShouldEqual(TextWriter.Null);
    }

    [Test]
    public void should_render_to_text_writer()
    {
        var template = new Template(t => t.WriteLiteral("foo"));
        template.Output.ShouldEqual(TextWriter.Null);

        var output = new StringWriter();
        template.Render(output);
        output.ToString().ShouldEqual("foo");

        template.Output.ShouldEqual(TextWriter.Null);
    }

    [Test]
    public async Task should_render_async_to_local_output()
    {
        var template = new Template(t => t.WriteLiteral("foo"));
        template.Output.ShouldEqual(TextWriter.Null);

        (await template.RenderAsync()).ShouldEqual("foo");

        template.Output.ShouldEqual(TextWriter.Null);
    }

    [Test]
    public async Task should_render_async_to_text_writer()
    {
        var template = new Template(t => t.WriteLiteral("foo"));
        template.Output.ShouldEqual(TextWriter.Null);

        var output = new StringWriter();
        await template.RenderAsync(output);
        output.ToString().ShouldEqual("foo");

        template.Output.ShouldEqual(TextWriter.Null);
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

    [Test]
#if NETFRAMEWORK
    [Timeout(10_000)]
#endif
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

    [Test]
    public async Task should_flush_output()
    {
        var output = new StringWriter();
        var stepSemaphore = new StepSemaphore();

        var template = new Template(async t =>
        {
            await stepSemaphore.WaitForNextStepAsync();

            t.WriteLiteral("foo");
            await t.FlushAsync();

            await stepSemaphore.WaitForNextStepAsync();

            t.WriteLiteral(" bar");
            await t.FlushAsync();

            await stepSemaphore.WaitForNextStepAsync();

            t.WriteLiteral(" baz");
            await t.FlushAsync();

            await stepSemaphore.NotifyEndOfLastStepAsync();
        });

        var task = template.RenderAsync(output);

        await stepSemaphore.StartNextStepAndWaitForResultAsync();
        output.ToString().ShouldEqual("foo");
        template.Output.ShouldBe<StringWriter>().ToString().ShouldEqual(string.Empty);

        await stepSemaphore.StartNextStepAndWaitForResultAsync();
        output.ToString().ShouldEqual("foo bar");
        template.Output.ShouldBe<StringWriter>().ToString().ShouldEqual(string.Empty);

        await stepSemaphore.StartNextStepAndWaitForResultAsync();
        output.ToString().ShouldEqual("foo bar baz");

        await task;
        output.ToString().ShouldEqual("foo bar baz");
        template.Output.ShouldEqual(TextWriter.Null);
    }

    [Test]
    public async Task should_buffer_output_until_flushed()
    {
        var output = new StringWriter();
        var stepSemaphore = new StepSemaphore();

        var template = new Template(async t =>
        {
            await stepSemaphore.WaitForNextStepAsync();

            t.WriteLiteral("foo");
            await t.Output.FlushAsync();

            await stepSemaphore.WaitForNextStepAsync();

            t.WriteLiteral(" bar");
            await t.Output.FlushAsync();

            await stepSemaphore.WaitForNextStepAsync();

            t.WriteLiteral(" baz");
            await t.Output.FlushAsync();

            await stepSemaphore.NotifyEndOfLastStepAsync();
        });

        var task = template.RenderAsync(output);

        await stepSemaphore.StartNextStepAndWaitForResultAsync();
        output.ToString().ShouldEqual(string.Empty);
        template.Output.ShouldBe<StringWriter>().ToString().ShouldEqual("foo");

        await stepSemaphore.StartNextStepAndWaitForResultAsync();
        output.ToString().ShouldEqual(string.Empty);
        template.Output.ShouldBe<StringWriter>().ToString().ShouldEqual("foo bar");

        await stepSemaphore.StartNextStepAndWaitForResultAsync();
        await task;
        output.ToString().ShouldEqual("foo bar baz");
    }

    private class Template(Func<Template, Task> executeAction) : RazorTemplate
    {
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
            await executeAction(this);
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
