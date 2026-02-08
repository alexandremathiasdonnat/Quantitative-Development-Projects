using System.Collections.Generic;
using Antlr4.Runtime;
using QuantDsl.Core.Diagnostics;
using QuantDsl.Core.Models;

namespace QuantDsl.Core.Semantics;

public static class ProductExtractor
{
    public static (ProductSpec Spec, List<Diagnostic> Diagnostics) Extract(QuantDslParser.FileContext tree)
    {
        var diags = new List<Diagnostic>();
        var spec = new ProductSpec();

        // product name
        var prod = tree.productDecl();
        spec.ProductName = prod.IDENT().GetText();

        // iterate statements
        foreach (var st in tree.statement())
        {
            var start = st.Start; // IToken with line/col

            if (st.NOTIONAL() != null)
            {
                spec.Notional = decimal.Parse(st.NUMBER().GetText(), System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (st.FIXED_RATE() != null)
            {
                spec.FixedRate = decimal.Parse(st.NUMBER().GetText(), System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (st.MATURITY() != null)
            {
                spec.MaturityTenor = st.TENOR().GetText();
            }
            else if (st.PAYOFF() != null)
            {
                spec.PayoffRaw = st.expr().GetText();
            }
            else
            {
                diags.Add(new Diagnostic(
                    DiagnosticSeverity.Error,
                    start.Line,
                    start.Column,
                    "UNKNOWN_STATEMENT",
                    "Unknown statement."
                ));
            }
        }

        // semantic validations
        ValidateRequired(spec, diags);
        ValidateRanges(spec, tree, diags);
        ValidatePayoffIdentifiers(tree, diags);

        return (spec, diags);
    }

    private static void ValidateRequired(ProductSpec spec, List<Diagnostic> diags)
    {
        if (string.IsNullOrWhiteSpace(spec.ProductName))
            diags.Add(new Diagnostic(DiagnosticSeverity.Error, 1, 0, "MISSING_PRODUCT", "Missing product name."));

        if (spec.Notional is null)
            diags.Add(new Diagnostic(DiagnosticSeverity.Error, 1, 0, "MISSING_NOTIONAL", "Missing notional."));
        if (spec.FixedRate is null)
            diags.Add(new Diagnostic(DiagnosticSeverity.Error, 1, 0, "MISSING_FIXED_RATE", "Missing fixed_rate."));
        if (spec.MaturityTenor is null)
            diags.Add(new Diagnostic(DiagnosticSeverity.Error, 1, 0, "MISSING_MATURITY", "Missing maturity."));
    }

    private static void ValidateRanges(ProductSpec spec, QuantDslParser.FileContext tree, List<Diagnostic> diags)
    {
        if (spec.Notional is { } n && n <= 0)
            diags.Add(new Diagnostic(DiagnosticSeverity.Error, 1, 0, "NOTIONAL_INVALID", "notional must be > 0."));

        if (spec.FixedRate is { } r && (r < 0 || r > 1))
            diags.Add(new Diagnostic(DiagnosticSeverity.Error, 1, 0, "FIXED_RATE_INVALID", "fixed_rate must be between 0 and 1."));
    }

    private static void ValidatePayoffIdentifiers(QuantDslParser.FileContext tree, List<Diagnostic> diags)
    {
        // Allowed identifiers in payoff:
        // - IDENT (user vars) are not supported (so should error)
        // - NOTIONAL / FIXED_RATE are allowed "variables"
        // We traverse all factors and flag IDENT usage.
        foreach (var st in tree.statement())
        {
            if (st.PAYOFF() == null) continue;

            var expr = st.expr();
            var tokens = expr.GetTokens(QuantDslParser.IDENT);

            foreach (var t in tokens)
            {
                diags.Add(new Diagnostic(
                    DiagnosticSeverity.Error,
                    t.Symbol.Line,
                    t.Symbol.Column,
                    "UNDECLARED_ID",
                    $"Unknown identifier: {t.GetText()}"
                ));
            }
        }
    }
}
