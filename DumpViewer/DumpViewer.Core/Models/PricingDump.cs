using System;
using System.Collections.Generic;

namespace DumpViewer.Core.Models;

public sealed class PricingDump
{
    public string ProductId { get; init; } = "";
    public DateTime RunTime { get; init; }
    public decimal Price { get; init; }

    // Greeks like Delta, Vega, Gamma...
    public Dictionary<string, decimal> Greeks { get; init; } = new();
}
