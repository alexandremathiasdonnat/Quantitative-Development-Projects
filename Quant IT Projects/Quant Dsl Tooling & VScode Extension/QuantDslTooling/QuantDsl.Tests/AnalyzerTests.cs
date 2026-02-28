using System.IO;
using QuantDsl.Core.Analysis;
using Xunit;

public class AnalyzerTests
{
    [Fact]
    public void Analyze_ValidFile_IsOk()
    {
        var path = Path.Combine("..", "..", "..", "..", "QuantDsl.Cli", "Samples", "sample.dsl");
        var report = DslAnalyzer.Analyze(path);

        Assert.True(report.Summary.ok);
        Assert.Empty(report.SyntaxErrors);
        Assert.Empty(report.SemanticDiagnostics);
    }

    [Fact]
    public void Analyze_SemanticErrors_ReturnsDiagnostics()
    {
        var path = Path.Combine("..", "..", "..", "..", "QuantDsl.Cli", "Samples", "bad_semantic.dsl");
        var report = DslAnalyzer.Analyze(path);

        Assert.False(report.Summary.ok);
        Assert.Empty(report.SyntaxErrors);
        Assert.NotEmpty(report.SemanticDiagnostics);
    }
}
