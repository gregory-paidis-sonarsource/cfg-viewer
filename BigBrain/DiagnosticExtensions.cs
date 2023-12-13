using Microsoft.CodeAnalysis;
using System.Text;

namespace VisualCfg.BigBrain;

public static class DiagnosticExtensions
{
    public static string ToPrettyString(this List<Diagnostic> diagnostics)
    {
        var sb = new StringBuilder();
        foreach (var diagnostic in diagnostics)
        {
            sb.AppendLine(diagnostic.Severity + ":" + diagnostic.Id + ": " + diagnostic.GetMessage());
        }
        return sb.ToString();
    }
}
