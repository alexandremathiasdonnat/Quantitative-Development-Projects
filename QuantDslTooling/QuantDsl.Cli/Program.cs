using System;
using System.IO;
using System.Text.Json;
using QuantDsl.Core.Analysis;

namespace QuantDsl.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length < 2 || args[0] != "analyze")
        {
            Console.WriteLine("Usage: qdsl analyze <file.dsl> [--out report.json]");
            return 2;
        }

        var path = args[1];
        string? outPath = null;

        // parse optional --out
        for (int i = 2; i < args.Length; i++)
        {
            if (args[i] == "--out" && i + 1 < args.Length)
            {
                outPath = args[i + 1];
                i++;
            }
        }

        try
        {
            var report = DslAnalyzer.Analyze(path);

            if (outPath is not null)
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(outPath, JsonSerializer.Serialize(report, options));
                Console.WriteLine($"Wrote report: {outPath}");
            }
            else
            {
                // Console output fallback
                if (report.Summary.ok)
                {
                    Console.WriteLine("OK: parsed with no syntax or semantic errors.");
                }
                else if (report.SyntaxErrors.Count > 0)
                {
                    Console.WriteLine($"Syntax errors: {report.SyntaxErrors.Count}");
                    foreach (var e in report.SyntaxErrors)
                        Console.WriteLine($"- line {e.Line}, col {e.Column}: {e.Message}");
                }
                else
                {
                    Console.WriteLine($"Semantic errors: {report.SemanticDiagnostics.Count}");
                    foreach (var d in report.SemanticDiagnostics)
                        Console.WriteLine($"- {d.Severity} {d.Code} at line {d.Line}, col {d.Column}: {d.Message}");
                }
            }

            return report.Summary.ok ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal: {ex.Message}");
            return 3;
        }
    }
}
