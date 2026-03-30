using Microsoft.CodeAnalysis;

namespace ResourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class ReswResourceGenerator :
    IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(
        IncrementalGeneratorInitializationContext context)
    {
    }
}
