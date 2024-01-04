using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using MyApplication;

namespace RazorBlade.IntegrationTest.Examples
{
    [SuppressMessage("ReSharper", "UnusedVariable")]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal class Examples
    {
        public static void ExampleTemplateUsage()
        {
            #region ExampleTemplate.Usage

            var template = new ExampleTemplate
            {
                Name = "World"
            };

            var result = template.Render();

            #endregion
        }

        public static void TemplateWithModelUsage()
        {
            #region TemplateWithModel.Usage

            var model = new GreetingModel { Name = "World" };
            var template = new TemplateWithModel(model);
            var result = template.Render();

            #endregion
        }
    }
}

namespace MyApplication
{
    public class GreetingModel
    {
        public required string Name { get; init; }
    }

    public class ModelType;
}
