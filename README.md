# RazorBlade                       <a href="#"><img src="icon.png" align="right" alt="Logo" /></a>

*The sharpest part of the razor.*

Compile Razor templates at build-time without a dependency on ASP.NET.

**This is a work-in-progress.**

## Usage

This package will generate a template class for every `.cshtml` file in your project.

You can use a `@functions { ... }` block to add properties to your template (instead of a model), which the IDE will see.

The generated template class will inherit from `RazorBlade.HtmlTemplate` by default, but you can customize this with an `@inherits` directive. Specifying the base class explicitly will give you access to its members in the IDE.
