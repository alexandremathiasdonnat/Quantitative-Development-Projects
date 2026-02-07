//Async await


using System.Text.Json;
using System.Threading.Tasks;

public class PricingDumpE10
{
    public string ProductId { get; set; } = "";
    public DateTime RunTime { get; set; }
    public decimal Price { get; set; }
}

public static class Exersice10
{
    public static async Task<PricingDumpE10> LoadDumpAsync(string path) // this function wont directly give the return, but will when called return an object with form PricingDumpE10
    {
        string json = await File.ReadAllTextAsync(path);

        PricingDumpE10 dump = JsonSerializer.Deserialize<PricingDumpE10>(json)
            ?? throw new Exception("Deserialisation failed");

        return dump;
    }

    public static async Task Run()
    {
        try
        {
            var dump = await LoadDumpAsync("sample_dump.json");
            Console.WriteLine($"ProductId = {dump.ProductId}");
            Console.WriteLine($"Price = {dump.Price}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur: {ex.Message}");
        }
    }
}