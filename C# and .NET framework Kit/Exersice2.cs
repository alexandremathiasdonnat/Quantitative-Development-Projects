// Exersice 2 : Properties

public class PricingDump
{
    public string ProductId { get; set; } = "";
    public DateTime RunTime { get; set;}
    public decimal Price {get; set;}
}

public static class Exersice2
{
    public static void Run()
    {
        var dump = new PricingDump
        {
            ProductId = "AAPL_CALL_150",
            RunTime = DateTime.Now,
            Price = 12.45m

        };


        Console.WriteLine(
            $"Product ID = {dump.ProductId} \nRunTime = {dump.RunTime} \nPrice = {dump.Price}"
            );
    }


}
