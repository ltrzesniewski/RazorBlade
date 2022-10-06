# RazorBlade                       <a href="#"><img src="icon.png" align="right" alt="Logo" /></a>

[![Build](https://github.com/ltrzesniewski/RazorBlade/workflows/Build/badge.svg)](https://github.com/ltrzesniewski/RazorBlade/actions?query=workflow%3ABuild)
[![NuGet package](https://img.shields.io/nuget/v/RazorBlade.svg?logo=NuGet)](https://www.nuget.org/packages/RazorBlade)
[![GitHub release](https://img.shields.io/github/release/ltrzesniewski/RazorBlade.svg?logo=GitHub)](https://github.com/ltrzesniewski/RazorBlade/releases)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/ltrzesniewski/RazorBlade/blob/master/LICENSE)

*The sharpest part of the razor.*

Compile Razor templates at build-time without a dependency on ASP.NET.

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

Further [documentation](#Documentation) is provided below.

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

## Documentation

### Base template classes

Use one of the following base classes for HTML templates: 

- `RazorBlade.HtmlTemplate`
- `RazorBlade.HtmlTemplate<TModel>`

If you'd like to write a plain text template (which never escapes HTML), the following classes are available:

- `RazorBlade.PlainTextTemplate`
- `RazorBlade.PlainTextTemplate<TModel>`

They all derive from `RazorBlade.RazorTemplate`, which provides the base functionality.

You can also write your own base classes. Marking a constructor with `[TemplateConstructor]` will forward it to the generated template class. 

### Writing templates

HTML escaping can be avoided by using the `@Html.Raw(value)` method, just like in ASP.NET. The `IEncodedContent` interface represents content which does not need to be escaped. The `HtmlString` class is a simple implementation of this interface.

Templates can be included in other templates by evaluating them, since they implement `IEncodedContent`. For instance, a `Footer` template can be included by writing `@(new Footer())`. Remember to always create a new instance of the template to include, even if they don't contain custom code, as they are not thread-safe.

The namespace of the generated class can be customized with the `@namespace` directive. The default value is deduced from the file location.

### Executing templates

The `RazorTemplate` base class provides `Render` and `RenderAsync` methods to execute the template.

Templates are stateful and not thread-safe, so it is advised to always create new instances of the templates to render.

### MSBuild

The source generator will process `RazorBlade` MSBuild items which have the `.cshtml` file extension.

By default, all `.cshtml` files are included, unless one of the `EnableDefaultRazorBladeItems` or `EnableDefaultItems` properties are set to `false`. You can also manually customize this set.
