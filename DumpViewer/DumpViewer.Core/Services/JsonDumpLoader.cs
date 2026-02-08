using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using DumpViewer.Core.Models;

namespace DumpViewer.Core.Services;

public sealed class JsonDumpLoader : IDumpLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<PricingDump> LoadAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path is empty.", nameof(path));

        if (!File.Exists(path))
            throw new FileNotFoundException("Dump file not found.", path);

        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);

        var dump = JsonSerializer.Deserialize<PricingDump>(json, Options);
        if (dump is null)
            throw new InvalidOperationException("Invalid JSON dump format.");

        // Minimal validation to avoid silent garbage
        if (string.IsNullOrWhiteSpace(dump.ProductId))
            throw new InvalidOperationException("Dump is missing ProductId.");

        return dump;
    }
}
