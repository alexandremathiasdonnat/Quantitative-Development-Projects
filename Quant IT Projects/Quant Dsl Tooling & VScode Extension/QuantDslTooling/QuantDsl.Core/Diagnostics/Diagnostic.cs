namespace QuantDsl.Core.Diagnostics;

public enum DiagnosticSeverity { Info, Warning, Error }

public sealed record Diagnostic(
    DiagnosticSeverity Severity,
    int Line,
    int Column,
    string Code,
    string Message
);
