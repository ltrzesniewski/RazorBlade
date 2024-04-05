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
        var template = new Template(t => t.Write(new TestEncodedContent()));
        template.Render().ShouldEqual("<br>");
    }

    [Test]
    public void should_write_IEncodedContent_as_object()
    {
        var template = new Template(t => t.Write((object)new TestEncodedContent()));
        template.Render().ShouldEqual("<br>");
    }

    [Test]
    public void should_write_attributes()
    {
        var template = new Template(t =>
        {
            t.BeginWriteAttribute("name", "attr_prefix ", 0, " attr_suffix", 0, 2);
            t.WriteAttributeValue("foo_prefix ", 0, "<", 0, 0, false);
            t.WriteAttributeValue(" bar_prefix ", 0, ">", 0, 0, true);
            t.EndWriteAttribute();
        });

        template.Render().ShouldEqual("attr_prefix foo_prefix < bar_prefix > attr_suffix");
    }

    [Test]
    [TestCase(null)]
    [TestCase(false)]
    [TestCase(true)]
    [TestCase("bar")]
    public void should_not_special_case_attributes_by_value(object value)
    {
        var template = new Template(t =>
        {
            t.BeginWriteAttribute("foo", "foo=\"", 0, "\"", 0, 1);
            t.WriteAttributeValue("", 0, value, 0, 0, false);
            t.EndWriteAttribute();
        });

        template.Render().ShouldEqual($"foo=\"{value}\"");
    }

    [Test]
    public void should_not_skip_prefixes_of_null_attribute_values()
    {
        var template = new Template(t =>
        {
            t.BeginWriteAttribute("foo", "foo=\"", 0, "\"", 0, 5);
            t.WriteAttributeValue(" a ", 0, true, 0, 0, false);
            t.WriteAttributeValue(" b ", 0, false, 0, 0, false);
            t.WriteAttributeValue(" c ", 0, 42, 0, 0, false);
            t.WriteAttributeValue(" d ", 0, null, 0, 0, false);
            t.WriteAttributeValue(" e ", 0, "bar", 0, 0, false);
            t.EndWriteAttribute();
        });

        template.Render().ShouldEqual("foo=\" a True b False c 42 d  e bar\"");
    }

    private class Template(Action<Template> executeAction) : PlainTextTemplate
    {
        protected internal override Task ExecuteAsync()
        {
            executeAction(this);
            return base.ExecuteAsync();
        }
    }

    private class TestEncodedContent : IEncodedContent
    {
        public void WriteTo(TextWriter textWriter)
            => textWriter.Write("<br>");
    }
}
