# RazorBlade                       <a href="#"><img src="icon.png" align="right" alt="Logo" /></a>

[![Build](https://github.com/ltrzesniewski/RazorBlade/workflows/Build/badge.svg)](https://github.com/ltrzesniewski/RazorBlade/actions?query=workflow%3ABuild)
[![NuGet package](https://img.shields.io/nuget/v/RazorBlade.svg?logo=NuGet)](https://www.nuget.org/packages/RazorBlade)
[![GitHub release](https://img.shields.io/github/release/ltrzesniewski/RazorBlade.svg?logo=GitHub)](https://github.com/ltrzesniewski/RazorBlade/releases)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/ltrzesniewski/RazorBlade/blob/master/LICENSE)

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
