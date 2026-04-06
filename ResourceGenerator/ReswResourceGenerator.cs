using System;
using System.IO;

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
        var priFilesProvider = context.AdditionalTextsProvider
            .Where(static x => IsReswFile(x.Path))
            .Select(static (file, cancellationToken) =>
            {
                var text = file.GetText(cancellationToken);
                return (Path: file.Path, Text: text?.ToString());
            })
            .Collect()
            .Select(static (input, cancellationToken) =>
            {

            });

        context.RegisterSourceOutput(
            priFilesProvider,
            static (context, input) =>
            {
                Console.WriteLine(input.Path);
            });
    }

    private static bool IsReswFile(string path)
    {
        return string.Equals(Path.GetExtension(path), ".resw", StringComparison.OrdinalIgnoreCase);
    }
}
