# RazorBlade                       <a href="#"><img src="icon.png" align="right" alt="Logo" /></a>

[![Build](https://github.com/ltrzesniewski/RazorBlade/workflows/Build/badge.svg)](https://github.com/ltrzesniewski/RazorBlade/actions?query=workflow%3ABuild)
[![NuGet package](https://img.shields.io/nuget/v/RazorBlade.svg?logo=NuGet)](https://www.nuget.org/packages/RazorBlade)
[![GitHub release](https://img.shields.io/github/release/ltrzesniewski/RazorBlade.svg?logo=GitHub)](https://github.com/ltrzesniewski/RazorBlade/releases)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/ltrzesniewski/RazorBlade/blob/master/LICENSE)

*The sharpest part of the razor.*

Compile Razor templates at build-time without a dependency on ASP.NET.

This is a work-in-progress. Feedback is welcome.

## Usage

This package will generate a template class for every `.cshtml` file in your project.

The generated classes will inherit from `RazorBlade.HtmlTemplate` by default, though it is advised to specify the base class explicitly to get the best IDE experience:

````Razor
@inherits RazorBlade.HtmlTemplate
````

A version with a model is also available for convenience. The following will add a `Model` property and a constructor with a `TModel` parameter:

```Razor
@inherits RazorBlade.HtmlTemplate<TModel>
```

Generated templates are *not* thread-safe. Always use new instances.

## Example

The following template, in the `TestTemplate.cshtml` file:

```Razor
@inherits RazorBlade.HtmlTemplate

Hello, <i>@Name</i>!

@functions
{
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

### With a model

A similar template with a model would be:

```Razor
@using MyApplication.Models
@inherits RazorBlade.HtmlTemplate<GreetingModel>

Hello, <i>@Model.Name</i>!
```

Instantiating the generated class requires a model argument:

```C#
var model = new GreetingModel { Name = "World" };
var template = new TestTemplate(model);
var result = template.Render();
```

## Additional Features

- The namespace of generated classes is deduced from the file location. This can be overriden with the `@namespace` directive.
- Templates can be composed by writing them as values: `@(new Footer())` for instance.
- A rudimentary HTML helper is provided mostly for `@Html.Raw`
- You can write custom base classes. Constructors decorated with `[TemplateConstructor]` will be available through the generated class.
