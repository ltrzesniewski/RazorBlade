using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RazorBlade.Tests.Support;

namespace RazorBlade.Library.Tests;

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
    public void should_render_to_string()
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
    public async Task should_render_async_to_string()
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

        var executionCount = 0;
        var template = new Template(_ => Interlocked.Increment(ref executionCount));

        Assert.Catch<OperationCanceledException>(() => template.Render(cts.Token));
        Assert.Catch<OperationCanceledException>(() => template.Render(StreamWriter.Null, cts.Token));
        Assert.CatchAsync<OperationCanceledException>(() => template.RenderAsync(cts.Token));
        Assert.CatchAsync<OperationCanceledException>(() => template.RenderAsync(StreamWriter.Null, cts.Token));

        executionCount.ShouldEqual(0);
    }

    [Test]
    public async Task should_flush_output()
    {
        var output = new StringWriter();
        var stepSemaphore = new StepSemaphore();

        var template = new Template(async t =>
        {
            using var worker = stepSemaphore.CreateWorker();
            await worker.WaitForNextStepAsync();

            t.WriteLiteral("foo");
            await t.FlushAsync();

            await worker.WaitForNextStepAsync();

            t.WriteLiteral(" bar");
            await t.FlushAsync();

            await worker.WaitForNextStepAsync();

            t.WriteLiteral(" baz");
            await t.FlushAsync();

            await worker.WaitForNextStepAsync();

            t.WriteLiteral(" hello"); // No flush after here

            await worker.WaitForNextStepAsync();

            t.WriteLiteral(" world");
        });

        var task = template.RenderAsync(output);
        var controller = stepSemaphore.CreateController();

        await controller.StartNextStepAndWaitForResultAsync();
        output.ToString().ShouldEqual("foo");
        template.Output.ShouldBe<StringWriter>().ToString().ShouldEqual(string.Empty);

        await controller.StartNextStepAndWaitForResultAsync();
        output.ToString().ShouldEqual("foo bar");
        template.Output.ShouldBe<StringWriter>().ToString().ShouldEqual(string.Empty);

        await controller.StartNextStepAndWaitForResultAsync();
        output.ToString().ShouldEqual("foo bar baz");
        template.Output.ShouldBe<StringWriter>().ToString().ShouldEqual(string.Empty);

        await controller.StartNextStepAndWaitForResultAsync();
        output.ToString().ShouldEqual("foo bar baz");
        template.Output.ShouldBe<StringWriter>().ToString().ShouldEqual(" hello");

        await controller.StartNextStepAndWaitForResultAsync();
        await task;
        output.ToString().ShouldEqual("foo bar baz hello world");
        template.Output.ShouldEqual(TextWriter.Null);
    }

    [Test]
    public async Task should_buffer_output_until_flushed()
    {
        var output = new StringWriter();
        var stepSemaphore = new StepSemaphore();

        var template = new Template(async t =>
        {
            using var worker = stepSemaphore.CreateWorker();
            await worker.WaitForNextStepAsync();

            t.WriteLiteral("foo");
            await t.Output.FlushAsync();

            await worker.WaitForNextStepAsync();

            t.WriteLiteral(" bar");
            await t.Output.FlushAsync();

            await worker.WaitForNextStepAsync();

            t.WriteLiteral(" baz");
            await t.Output.FlushAsync();
        });

        var task = template.RenderAsync(output);
        var controller = stepSemaphore.CreateController();

        await controller.StartNextStepAndWaitForResultAsync();
        output.ToString().ShouldEqual(string.Empty);
        template.Output.ShouldBe<StringWriter>().ToString().ShouldEqual("foo");

        await controller.StartNextStepAndWaitForResultAsync();
        output.ToString().ShouldEqual(string.Empty);
        template.Output.ShouldBe<StringWriter>().ToString().ShouldEqual("foo bar");

        await controller.StartNextStepAndWaitForResultAsync();
        await task;
        output.ToString().ShouldEqual("foo bar baz");
    }

    [Test]
    public void should_push_pop_writers()
    {
        var writerA = new StringWriter();
        var writerB = new StringWriter();

        var template = new Template(t =>
        {
            t.Write("A");
            t.PushWriter(writerA);
            t.Write("B");
            t.PushWriter(writerB);
            t.Write("C");
            t.PopWriter();
            t.Write("D");
            t.PopWriter();
            t.Write("E");
        });

        template.Render().ShouldEqual("AE");
        writerA.ToString().ShouldEqual("BD");
        writerB.ToString().ShouldEqual("C");
    }

    [Test]
    [SuppressMessage("ReSharper", "ConvertToLocalFunction")]
    public void should_execute_templated_delegate()
    {
        var template = new Template(t =>
        {
            Func<object, object> bold = item => new RazorTemplate.HelperResult(writer =>
                {
                    t.PushWriter(writer);
                    t.WriteLiteral("<b>");
                    t.Write(item);
                    t.WriteLiteral("</b>");
                    t.PopWriter();
                    return Task.CompletedTask;
                }
            );

            t.Write(bold("Bold text"));
            t.WriteLiteral(" - ");
            t.Write(bold("Other bold text"));
        });

        template.Render().ShouldEqual("<b>Bold text</b> - <b>Other bold text</b>");
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
            if (value is IEncodedContent encodedContent)
                encodedContent.WriteTo(Output);
            else
                WriteLiteral(value?.ToString());
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
