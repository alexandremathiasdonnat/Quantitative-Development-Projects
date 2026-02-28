// Deserialisation (=transform a .json data to c# obejct/variable with a type associated so to be used directly just after)
using System.Text.Json;

public class PricingDumpE9
{
    public string ProductId { get; set; } = "";
    public DateTime RunTime { get; set; }
    public decimal Price { get; set; }
}

public static class Exersice9
{
    public static void Run()
    {
        string path = "sample_dump.json";
        string json = File.ReadAllText(path);

        PricingDumpE9 dump = JsonSerializer.Deserialize<PricingDumpE9>(json)
            ?? throw new Exception("Deserialisation failed");  //happend if missing value "" or null value

        Console.WriteLine($"ProductId = {dump.ProductId}");
        Console.WriteLine($"Price = {dump.Price}");
        }
    }


