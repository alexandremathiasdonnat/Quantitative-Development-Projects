using System.Collections.Generic;

namespace QuantDsl.Core.Diagnostics;

public sealed class AnalysisReport
{
    public string Path { get; init; } = "";
    public List<SyntaxError> SyntaxErrors { get; init; } = new();
    public List<Diagnostic> SemanticDiagnostics { get; init; } = new();

    public ReportSummary Summary => new(
        ok: SyntaxErrors.Count == 0 && SemanticDiagnostics.Count == 0,
        syntaxErrorCount: SyntaxErrors.Count,
        semanticErrorCount: SemanticDiagnostics.Count
    );
}

public sealed record ReportSummary(bool ok, int syntaxErrorCount, int semanticErrorCount);
