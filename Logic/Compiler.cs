using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.CFG;
using SonarAnalyzer.CFG.Roslyn;

namespace VisualCfg.Logic;

public static class Compiler
{
    public static Either<string, List<Diagnostic>> CompileCfg(string snippet)
    {
        var tree = CSharpSyntaxTree.ParseText(snippet);
        var compilation = Compile(tree);
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

    private static Compilation Compile(SyntaxTree tree)
    {
        var metadata = GetReference();
        var options = new CSharpCompilationOptions(OutputKind.ConsoleApplication, concurrentBuild: false);
        var compilation = CSharpCompilation.Create("ヽ༼ຈل͜ຈ༽ﾉヽ༼◉ل͜◉༽ﾉヽ༼◔ل͜◔༽ﾉ", options: options)
            .AddReferences(metadata)
            .AddSyntaxTrees(tree);
        return compilation;
    }

    private static MetadataReference GetReference()
    {
        var assembly = typeof(Compiler).Assembly;
        using var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Martin.netstandard.dll");
        return MetadataReference.CreateFromStream(stream);
    }

    private static SyntaxNode GetNode(SyntaxTree tree)
    {
        var root = tree.GetCompilationUnitRoot() as SyntaxNode;
        var firstMethodFound = root.DescendantNodes().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();
        return firstMethodFound ?? root;
    }
}

