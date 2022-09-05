using System.Text;
using System.Threading.Tasks;

namespace RazorBlade;

public abstract class TextTemplate
{
    private readonly StringBuilder _sb = new();

    public abstract Task ExecuteAsync();

    public override string ToString() =>
        _sb.ToString();

    protected void WriteLiteral(string value)
        => _sb.Append(value);
}
