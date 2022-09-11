# RazorBlade                       <a href="#"><img src="icon.png" align="right" alt="Logo" /></a>

*The sharpest part of the razor.*

Compile Razor templates at build-time without a dependency on ASP.NET.

:warning: **This is a work-in-progress.** The API is unstable and will probably change in the future. More features will be added over time.

## Usage

This package will generate a template class for every `.cshtml` file in your project.

You can use a `@functions { ... }` block to add properties to your template (instead of a model), which the IDE will see.

The generated template class will inherit from `RazorBlade.HtmlTemplate` by default, but you can customize this with an `@inherits` directive. Specifying the base class explicitly will give you access to its members in the IDE.

## Example

The following template, in the `TestTemplate.cshtml` file:

```Razor
@inherits RazorBlade.HtmlTemplate

Hello, @Name!

@functions {
    public string? Name { get; set; }
}
```

Will generate the following class in your project:

```C#
internal partial class TestTemplate : RazorBlade.HtmlTemplate
{
    // ...
    public string? Name { get; set; }
    // ...
}
```

That you can use like the following:

```C#
var template = new TestTemplate
{
    Name = "World"
};

var result = template.Render();
```
