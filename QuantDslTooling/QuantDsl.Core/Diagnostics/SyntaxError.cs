namespace QuantDsl.Core.Diagnostics;

public sealed record SyntaxError(int Line, int Column, string Message);
