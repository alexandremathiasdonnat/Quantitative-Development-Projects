using QuantDsl.Core.Diagnostics;
using QuantDsl.Core.Parsing;
using QuantDsl.Core.Semantics;

namespace QuantDsl.Core.Analysis;

public static class DslAnalyzer
{
    public static AnalysisReport Analyze(string path)
    {
        var (tree, syntaxListener) = DslParser.ParseFromFile(path);

        if (syntaxListener.Errors.Count > 0)
        {
            return new AnalysisReport
            {
                Path = path,
                SyntaxErrors = syntaxListener.Errors
            };
        }

        var (_, diags) = ProductExtractor.Extract(tree!);

        return new AnalysisReport
        {
        Path = path,
        SyntaxErrors = syntaxListener.Errors,
        SemanticDiagnostics = diags
        };
    }
}