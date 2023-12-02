# RazorBlade                       <picture><source media="(prefers-color-scheme: dark)" srcset="icon-dark.png"><img src="icon.png" align="right" alt="Logo"></picture>

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

For HTML templates, specify one of the following base classes with an `@inherits` directive:

- `RazorBlade.HtmlTemplate`
- `RazorBlade.HtmlTemplate<TModel>`
- `RazorBlade.HtmlLayout` (for layouts only)

If you'd like to write a plain text template (which never escapes HTML), the following classes are available:

- `RazorBlade.PlainTextTemplate`
- `RazorBlade.PlainTextTemplate<TModel>`

They all derive from `RazorBlade.RazorTemplate`, which provides the base functionality.

You can also write your own base classes. Marking a constructor with `[TemplateConstructor]` will forward it to the generated template class. 

### Writing templates

HTML escaping can be avoided by using the `@Html.Raw(value)` method, just like in ASP.NET. The `IEncodedContent` interface represents content which does not need to be escaped. The `HtmlString` class is a simple implementation of this interface.

Templates can be included in other templates by evaluating them, since they implement `IEncodedContent`. For instance, a `Footer` template can be included by writing `@(new Footer())`. Remember to always create a new instance of the template to include, even if it doesn't contain custom code, as templates are stateful and not thread-safe.

The namespace of the generated class can be customized with the `@namespace` directive. The default value is deduced from the file location.

### Layouts

Layout templates may be written by inheriting from the `RazorBlade.HtmlLayout` class, which provides the relevant methods such as `RenderBody` and `RenderSection`. It inherits from `RazorBlade.HtmlTemplate`.

The layout to use can be specified through the `Layout` property of `RazorBlade.HtmlTemplate`. Given that all Razor templates are stateful and not thread-safe, always create a new instance of the layout page to use:

```Razor
@{
    Layout = new LayoutToUse();
}
```

Layout pages can be nested, and can use sections. Unlike in ASP.NET, RazorBlade does not verify if the body and all sections have been used. Sections may also be executed multiple times.

### Executing templates

The `RazorTemplate` base class provides `Render` and `RenderAsync` methods to execute the template.

Templates are stateful and not thread-safe, so it is advised to always create new instances of the templates to render.

### MSBuild

The source generator will process `RazorBlade` MSBuild items which have the `.cshtml` file extension.

By default, all `.cshtml` files are included, unless one of the `EnableDefaultRazorBladeItems` or `EnableDefaultItems` properties are set to `false`. You can also manually customize this set.

### Removing the dependency on RazorBlade

RazorBlade makes it possible to remove the dependency on its runtime assembly. This could be useful for library projects which should be self-contained, with no dependencies on external packages.

This mode is enabled by default when the `PackageReference` of RazorBlade has the `PrivateAssets="all"` attribute. In order to avoid compilation warnings, the assembly reference also needs to be explicitly excluded with `ExcludeAssets="compile;runtime"`.

```XML
<PackageReference Include="RazorBlade" Version="..." ExcludeAssets="compile;runtime" PrivateAssets="all" />
```

A source generator will then embed an `internal` version of the RazorBlade library in the target project. This behavior can also be controlled by setting the `RazorBladeEmbeddedLibrary` MSBuild property to `true` or `false`.
