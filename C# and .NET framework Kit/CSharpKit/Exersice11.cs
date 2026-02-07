// Interface and injection

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

public class PricingDumpE11
{
    public string ProductId { get; set; } = "";
    public DateTime RunTime { get; set; }
    public decimal Price { get; set; }
}

// Contract = interface: what we expect about a loader
public interface IDumpLoader // interface = method list liek "if you give me a path, i will Load (async) a PricingDumpE11  ; a loader = smtg that can load a dump async from a given path
{
    Task<PricingDumpE11> LoadAsync(string path);
}

// Implementaiton = to give a real realisation to a contract/interface (here really load JSON as the path)
public class JsonDumpLoader : IDumpLoader //here IDumperLoad is the interface
{
    public async Task<PricingDumpE11> LoadAsync(string path)
    {
        string json = await File.ReadAllTextAsync(path);

        return JsonSerializer.Deserialize<PricingDumpE11>(json)
            ?? throw new InvalidOperationException("Invalid dump format");
    }
}

public static class Exersice11
{
    public static async Task Run()
    {
        // Injection simple : We chosse an implementation (here JsonDumpLoader) and we fit it with an interface (here IDumpLoader)
        IDumpLoader loader = new JsonDumpLoader();

        PricingDumpE11 dump = await loader.LoadAsync("sample_dump.json");

        Console.WriteLine($"ProductId = {dump.ProductId}");
        Console.WriteLine($"Price = {dump.Price}");
    }
}
