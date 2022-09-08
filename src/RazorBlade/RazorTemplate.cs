using System.IO;
using System.Threading.Tasks;

namespace RazorBlade;

public abstract class RazorTemplate
{
    public TextWriter Output { get; set; } = new StringWriter();

    public virtual Task ExecuteAsync()
        => Task.CompletedTask; // The IDE complains when this method is abstract :(

    public override string ToString()
        => Output.ToString() ?? string.Empty;
}
