using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.CFG;
using SonarAnalyzer.CFG.Roslyn;
using System.Reflection;

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

    static Compilation Compile(SyntaxTree tree)
    {
        var metadata = GetReferences(
            [
                //typeof(object)
            ]);
        var compilation = CSharpCompilation.Create("JustGiveMeTheSemanticModel")
            .AddReferences(metadata)
            .AddSyntaxTrees(tree);

        return compilation;
    }

    static List<MetadataReference> GetReferences(Type[] types)
    {
        var res = new List<MetadataReference>();
        foreach (var assembly in types.Select(x => x.Assembly))
        {
            var reference = MetadataReference.CreateFromFile(assembly.Location);
            res.Add(reference);
        }
        return res;
    }

    static SyntaxNode GetNode(SyntaxTree tree)
    {
        var root = tree.GetCompilationUnitRoot() as SyntaxNode;
        var firstMethodFound = root.DescendantNodes().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();
        return firstMethodFound ?? root;
    }
}

