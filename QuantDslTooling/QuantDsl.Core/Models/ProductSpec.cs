namespace QuantDsl.Core.Models;

public sealed class ProductSpec
{
    public string? ProductName { get; set; }
    public decimal? Notional { get; set; }
    public decimal? FixedRate { get; set; }
    public string? MaturityTenor { get; set; } // "5Y"
    public string? PayoffRaw { get; set; }     // just for display/debug
}
