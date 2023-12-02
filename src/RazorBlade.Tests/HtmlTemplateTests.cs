using System;
using System.IO;
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
    [TestCase("foo's", "foo&#x27;s")]
    [TestCase("foo & bar < baz > foo\"bar", "foo &amp; bar &lt; baz &gt; foo&quot;bar")]
    public void should_escape_special_characters(string input, string expectedOutput)
    {
        var template = new Template(t => t.Write(input));
        template.Render().ShouldEqual(expectedOutput);
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
            t.WriteAttributeValue("foo_prefix ", 0, "&", 0, 0, false);
            t.WriteAttributeValue(" bar_prefix ", 0, "&", 0, 0, true);
            t.EndWriteAttribute();
        });

        template.Render().ShouldEqual("attr_prefix foo_prefix &amp; bar_prefix & attr_suffix");
    }

    [Test]
    [TestCase(null)]
    [TestCase(false)]
    public void should_skip_attribute_when_value_is_null_or_false(object value)
    {
        var template = new Template(t =>
        {
            t.BeginWriteAttribute("foo", "foo=\"", 0, "\"", 0, 1);
            t.WriteAttributeValue("", 0, value, 0, 0, false);
            t.EndWriteAttribute();
        });

        template.Render().ShouldEqual(string.Empty);
    }

    [Test]
    public void should_write_attribute_name_when_value_is_true()
    {
        var template = new Template(t =>
        {
            t.BeginWriteAttribute("foo", "foo=\"", 0, "\"", 0, 1);
            t.WriteAttributeValue("", 0, true, 0, 0, false);
            t.EndWriteAttribute();
        });

        template.Render().ShouldEqual("foo=\"foo\"");
    }

    [Test]
    public void should_skip_prefixes_of_null_attribute_values()
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

        template.Render().ShouldEqual("foo=\" a True b False c 42 e bar\"");
    }

    [Test]
    public void should_write_raw_string()
    {
        var template = new Template(t => t.Write(t.Raw("&<>")));

        template.Render().ShouldEqual("&<>");
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
    }

    private class TestEncodedContent : IEncodedContent
    {
        public void WriteTo(TextWriter textWriter)
            => textWriter.Write("<br>");
    }
}
