# RazorBlade                       <picture><source media="(prefers-color-scheme: dark)" srcset="icon-dark.png"><img src="icon.png" align="right" alt="Logo"></picture>

[![Build](https://github.com/ltrzesniewski/RazorBlade/workflows/Build/badge.svg)](https://github.com/ltrzesniewski/RazorBlade/actions?query=workflow%3ABuild)
[![NuGet package](https://img.shields.io/nuget/v/RazorBlade.svg?logo=NuGet)](https://www.nuget.org/packages/RazorBlade)
[![GitHub release](https://img.shields.io/github/release/ltrzesniewski/RazorBlade.svg?logo=GitHub)](https://github.com/ltrzesniewski/RazorBlade/releases)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/ltrzesniewski/RazorBlade/blob/master/LICENSE)

**Compile Razor templates at build-time without a dependency on ASP.NET.**

RazorBlade is meant to be *lightweight* and *self-contained*: cshtml files are compiled into C# classes at build-time with a Roslyn source generator. No reference to ASP.NET is required.

A simple base class library is provided by default, but it can also be embedded into the target project, or even replaced by your own implementation.

## Usage

This package will generate a template class for every `.cshtml` file in your project.

The generated classes will inherit from `RazorBlade.HtmlTemplate` by default, though it is advised to specify the base class explicitly to get the best IDE experience:

<!-- snippet: EmptyTemplate.cshtml -->
<a id='snippet-EmptyTemplate.cshtml'></a>
```cshtml
@inherits RazorBlade.HtmlTemplate
```
<sup><a href='/src/RazorBlade.IntegrationTest/Examples/EmptyTemplate.cshtml#L1-L1' title='Snippet source file'>snippet source</a> | <a href='#snippet-EmptyTemplate.cshtml' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

A version with a model is also available for convenience. The following will add a `Model` property and a constructor with a `ModelType` parameter:

<!-- snippet: EmptyTemplateWithModel.cshtml -->
<a id='snippet-EmptyTemplateWithModel.cshtml'></a>
```cshtml
@inherits RazorBlade.HtmlTemplate<MyApplication.ModelType>
```
<sup><a href='/src/RazorBlade.IntegrationTest/Examples/EmptyTemplateWithModel.cshtml#L1-L1' title='Snippet source file'>snippet source</a> | <a href='#snippet-EmptyTemplateWithModel.cshtml' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Please note that this will cause a constructor with a `ModelType` parameter to be added to the generated class, which may cause false errors to be shown in some IDEs.

Further [documentation](#Documentation) is provided below.

## Example

The following template, in the `ExampleTemplate.cshtml` file:

<!-- snippet: ExampleTemplate.cshtml -->
<a id='snippet-ExampleTemplate.cshtml'></a>
```cshtml
@inherits RazorBlade.HtmlTemplate

Hello, <i>@Name</i>!

@functions
{
    public string? Name { get; init; }
}
```
<sup><a href='/src/RazorBlade.IntegrationTest/Examples/ExampleTemplate.cshtml#L1-L8' title='Snippet source file'>snippet source</a> | <a href='#snippet-ExampleTemplate.cshtml' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Will generate the following class in your project:

```cs
internal partial class ExampleTemplate : RazorBlade.HtmlTemplate
{
    // ...
    public string? Name { get; init; }
    // ...
}
```

That you can use like the following:

<!-- snippet: ExampleTemplate.Usage -->
<a id='snippet-ExampleTemplate.Usage'></a>
```cs
var template = new ExampleTemplate
{
    Name = "World"
};

var result = template.Render();
```
<sup><a href='/src/RazorBlade.IntegrationTest/Examples/Examples.cs#L13-L22' title='Snippet source file'>snippet source</a> | <a href='#snippet-ExampleTemplate.Usage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### With a model

A similar template with a model would be:

<!-- snippet: TemplateWithModel.cshtml -->
<a id='snippet-TemplateWithModel.cshtml'></a>
```cshtml
@using MyApplication
@inherits RazorBlade.HtmlTemplate<GreetingModel>

Hello, <i>@Model.Name</i>!
```
<sup><a href='/src/RazorBlade.IntegrationTest/Examples/TemplateWithModel.cshtml#L1-L4' title='Snippet source file'>snippet source</a> | <a href='#snippet-TemplateWithModel.cshtml' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Instantiating the generated class requires a model argument:

<!-- snippet: TemplateWithModel.Usage -->
<a id='snippet-TemplateWithModel.Usage'></a>
```cs
var model = new GreetingModel { Name = "World" };
var template = new TemplateWithModel(model);
var result = template.Render();
```
<sup><a href='/src/RazorBlade.IntegrationTest/Examples/Examples.cs#L27-L33' title='Snippet source file'>snippet source</a> | <a href='#snippet-TemplateWithModel.Usage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Since this generates a constructor with a `GreetingModel` parameter in the `TemplateWithModel` class, it may cause false errors to be shown in some IDEs, as they don't recognize this constructor signature.

### With a manual model property

Another way of implementing a template with a model is to add a `Model` property in the template and mark it as `required`. This will work around false errors which can be shown in some IDEs.

<!-- snippet: TemplateWithManualModel.cshtml -->
<a id='snippet-TemplateWithManualModel.cshtml'></a>
```cshtml
@using MyApplication
@inherits RazorBlade.HtmlTemplate

Hello, <i>@Model.Name</i>!

@functions
{
    public required GreetingModel Model { get; init; }
}
```
<sup><a href='/src/RazorBlade.IntegrationTest/Examples/TemplateWithManualModel.cshtml#L1-L9' title='Snippet source file'>snippet source</a> | <a href='#snippet-TemplateWithManualModel.cshtml' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Instantiating the generated class is done similarly to the previous example:

<!-- snippet: TemplateWithManualModel.Usage -->
<a id='snippet-TemplateWithManualModel.Usage'></a>
```cs
var model = new GreetingModel { Name = "World" };
var template = new TemplateWithManualModel { Model = model };
var result = template.Render();
```
<sup><a href='/src/RazorBlade.IntegrationTest/Examples/Examples.cs#L38-L44' title='Snippet source file'>snippet source</a> | <a href='#snippet-TemplateWithManualModel.Usage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Documentation

### Base template classes

For HTML templates, specify one of the following base classes with an `@inherits` directive:

- `RazorBlade.HtmlTemplate`
- `RazorBlade.HtmlTemplate<TModel>`
- `RazorBlade.HtmlTemplateWithLayout<TLayout>` (automatically applies the given layout)
- `RazorBlade.HtmlLayout` (for layouts only)

If you'd like to write a plain text template (which never escapes HTML), the following classes are available:

- `RazorBlade.PlainTextTemplate`
- `RazorBlade.PlainTextTemplate<TModel>`

They all derive from `RazorBlade.RazorTemplate`, which provides the base functionality.

You can also write your own base classes. Marking a constructor with `[TemplateConstructor]` will forward it to the generated template class. 

### Writing templates

HTML escaping can be avoided by using the `@Html.Raw(value)` method, just like in ASP.NET. The `IEncodedContent` interface represents content which does not need to be escaped. The `HtmlString` class is a simple implementation of this interface.

The namespace of the generated class can be customized with the `@namespace` directive. The default value is deduced from the file location.

### Partials

Templates can be included in other templates by evaluating them, as they implement `IEncodedContent`. For instance, a `Footer` template can be included by writing `@(new Footer())`.

Since templates are stateful and not thread-safe, the simplest way to avoid race conditions is to always create a new instance of a partial to render:

<!-- snippet: TemplateWithPartials.Simple -->
<a id='snippet-TemplateWithPartials.Simple'></a>
```cshtml
<ul>
    @foreach (var item in ListItems)
    {
        @(new MyPartial { Value = item })
    }
</ul>
```
<sup><a href='/src/RazorBlade.IntegrationTest/Examples/TemplateWithPartials.cshtml#L4-L11' title='Snippet source file'>snippet source</a> | <a href='#snippet-TemplateWithPartials.Simple' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

But you may also reuse a single partial instance as long as it is only accessible to the template instance which uses it:

<!-- snippet: TemplateWithPartials.SingleInstance -->
<a id='snippet-TemplateWithPartials.SingleInstance'></a>
```cshtml
@{ var partialInstance = new MyPartial(); }
<ul>
    @foreach (var item in ListItems)
    {
        partialInstance.Value = item;
        @partialInstance
    }
</ul>
```
<sup><a href='/src/RazorBlade.IntegrationTest/Examples/TemplateWithPartials.cshtml#L13-L22' title='Snippet source file'>snippet source</a> | <a href='#snippet-TemplateWithPartials.SingleInstance' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

### Layouts

Layout templates may be written by inheriting from the `RazorBlade.HtmlLayout` class, which provides the relevant methods such as `RenderBody` and `RenderSection`. It inherits from `RazorBlade.HtmlTemplate`.

The layout to use can be specified by overriding the `CreateLayout` method of `RazorBlade.HtmlTemplate`. Given that all Razor templates are stateful and not thread-safe, always create a new instance of the layout page to use:

<!-- snippet: TemplateWithLayout.Usage -->
<a id='snippet-TemplateWithLayout.Usage'></a>
```cshtml
@functions
{
    protected override HtmlLayout? CreateLayout()
        => new LayoutToUse();
}
```
<sup><a href='/src/RazorBlade.IntegrationTest/Examples/TemplateWithLayout.cshtml#L2-L8' title='Snippet source file'>snippet source</a> | <a href='#snippet-TemplateWithLayout.Usage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

This can be simplified by using the `HtmlTemplateWithLayout<TLayout>` class, which can be useful in `_ViewImports.cshtml` files:

<!-- snippet: TemplateWithLayoutFromBase.cshtml -->
<a id='snippet-TemplateWithLayoutFromBase.cshtml'></a>
```cshtml
@inherits RazorBlade.HtmlTemplateWithLayout<LayoutToUse>
```
<sup><a href='/src/RazorBlade.IntegrationTest/Examples/TemplateWithLayoutFromBase.cshtml#L1-L1' title='Snippet source file'>snippet source</a> | <a href='#snippet-TemplateWithLayoutFromBase.cshtml' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Layout pages can be nested, and can use sections. Unlike in ASP.NET, RazorBlade does not verify if the body and all sections have been used. Sections may also be executed multiple times.

> [!NOTE]  
> Layout usage is not compatible with direct output to the provided `TextWriter` and will cause buffering.  
> You may work around it by replacing layouts with partial templates such as:
> 
> ```cshtml
> @(new Header())
> Your content
> @(new Footer())
> ```

### Import files: `_ViewImports.cshtml`

RazorBlade automatically applies the [`_ViewImports.cshtml` files](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/layout#importing-shared-directives) from the project root to the given template directory.

These files can define default values for `@using`, `@inherits` and `@namespace`. For instance, an `@inherits RazorBlade.HtmlTemplateWithLayout<LayoutToUse>` directive in a `_ViewImports.cshtml` file will set a common default layout type for the templates in its directory and subdirectories.

### Executing templates

The `RazorTemplate` base class provides `Render` and `RenderAsync` methods to execute the template.

Templates are stateful and not thread-safe, so it is advised to always create new instances of the templates to render.

### Supported directives

The following [Razor directives](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor#directives) are supported:

- `@attribute`
- `@functions`
- `@implements`
- `@inherits`
- `@namespace`
- `@section`
- `@typeparam`

The following ones are rejected:

- `@model` - Use a base class such as `HtmlTemplate<TModel>` instead.
- `@addTagHelper`, `@removeTagHelper`, `@tagHelperPrefix` - [Tag helpers](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/intro) would require ASP.NET.

### MSBuild

The source generator will process `RazorBlade` MSBuild items which have the `.cshtml` file extension.

By default, all `.cshtml` files are included, unless one of the `EnableDefaultRazorBladeItems` or `EnableDefaultItems` properties are set to `false`. You can also manually customize this set.

**Available property settings:**

- `EnableDefaultRazorBladeItems`: Whether to automatically include all `.cshtml` files in the project. Default is `true`.
- `RazorBladeDefaultAccessibility`: The default accessibility of the generated classes (`internal` or `public`). Default is `internal`.
- `RazorBladeEmbeddedLibrary`: Whether to embed the RazorBlade library in the target project (see below). Default is `false`.

**Available item metadata settings:**

- `Accessibility`: The accessibility of the generated class (`internal` or `public`). Default is `$(RazorBladeDefaultAccessibility)`.

### Removing the dependency on RazorBlade

RazorBlade makes it possible to remove the dependency on its runtime assembly. This could be useful for library projects which should be self-contained, with no dependencies on external packages.

This mode is enabled by default when the `PackageReference` of RazorBlade has the `PrivateAssets="all"` attribute. In order to avoid compilation warnings, the assembly reference also needs to be explicitly excluded with `ExcludeAssets="compile;runtime"`.

```XML
<PackageReference Include="RazorBlade" Version="..." ExcludeAssets="compile;runtime" PrivateAssets="all" />
```

A source generator will then embed an `internal` version of the RazorBlade library in the target project. This behavior can also be controlled by setting the `RazorBladeEmbeddedLibrary` MSBuild property to `true` or `false`.
