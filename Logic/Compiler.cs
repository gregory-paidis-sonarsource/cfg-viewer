using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.CFG;
using SonarAnalyzer.CFG.Roslyn;

namespace VisualCfg.Logic;

public static class Compiler
{
    public static Either<string, List<Diagnostic>> CompileCfg(string snippet, OutputKind outputKind)
    {
        var tree = CSharpSyntaxTree.ParseText(snippet);
        var compilation = Compile(tree, outputKind);
        var diagnostics = compilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error).ToList();
        if (diagnostics.Any())
        {
            return new(diagnostics);
        }
        var node = GetNode(tree);
        var model = compilation.GetSemanticModel(tree, true);
        var cfg = ControlFlowGraph.Create(node, model, CancellationToken.None);

        var serialized = CfgSerializer.Serialize(cfg);
        return new(serialized);
    }

    private static Compilation Compile(SyntaxTree tree, OutputKind outputKind)
    {
        var options = new CSharpCompilationOptions(outputKind, concurrentBuild: false); // WASM is single threaded, so we can't use concurrent build.
        var compilation = CSharpCompilation.Create("ヽ༼ຈل͜ຈ༽ﾉヽ༼◉ل͜◉༽ﾉヽ༼◔ل͜◔༽ﾉ", options: options) // BEST assembly name.
            .AddReferences(BaseReference())
            .AddSyntaxTrees(tree);
        return compilation;
    }

    private static MetadataReference BaseReference()
    {
        var assembly = typeof(Compiler).Assembly;
        // This is a .NET Standard assembly that includes all the basic .NET APIs.
        // It's used as the target of the compilation so that we can resolve types and basic functionality.
        // It is found in "C:\Program Files\dotnet\sdk\8.0.100\ref".
        // The cool part is that it's a reference assembly, so it's quite small.
        // It needs to be marked as Embeded Resource in the .csproj file.
        // https://learn.microsoft.com/en-us/dotnet/standard/assembly/reference-assemblies
        using var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Assembly.netstandard.dll");
        return MetadataReference.CreateFromStream(stream);
    }

    private static SyntaxNode GetNode(SyntaxTree tree)
    {
        var root = tree.GetCompilationUnitRoot() as SyntaxNode;
        var firstMethodFound = root.DescendantNodes().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();
        return firstMethodFound ?? root;
    }
}

