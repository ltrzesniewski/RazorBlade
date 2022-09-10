# RazorBlade                       <a href="#"><img src="icon.png" align="right" alt="Logo" /></a>

Compile Razor templates at build-time without a dependency on ASP.NET.

**This is a work-in-progress.**

## Usage

This package will generate a template class for every `.cshtml` file in your project.

- Add the following directive to your Razor files: `@inherits RazorBlade.HtmlTemplate`
- Use a `@functions { ... }` block to define the values your template will render.
