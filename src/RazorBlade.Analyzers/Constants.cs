namespace RazorBlade.Analyzers;

internal static class Constants
{
    public static class GlobalOptions
    {
        private const string _prefix = "build_property";

        public const string DefaultAccessibility = $"{_prefix}.RazorBladeDefaultAccessibility";
        public const string EmbeddedLibrary = $"{_prefix}.RazorBladeEmbeddedLibrary";
    }

    public static class FileOptions
    {
        private const string _prefix = "build_metadata.AdditionalFiles";

        public const string IsRazorBlade = $"{_prefix}.IsRazorBlade";
        public const string HintNamespace = $"{_prefix}.HintNamespace";
        public const string Accessibility = $"{_prefix}.Accessibility";
    }
}
