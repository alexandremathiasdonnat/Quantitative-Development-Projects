// Exersice 2 : Properties (variables are mutable (editable) after initialization "set")

public class PricingDump
{
    public string ProductId { get; set; } = "";
    public DateTime RunTime { get; set; }
    public decimal Price {get; set; }
}

public static class Exersice2
{
    public static void Run()
    {
        var dump = new PricingDump
        {
            ProductId = "AAPL_CALL_150",
            RunTime = DateTime.Now,
            Price = 12.35m

        };
        //dump.Price = 12.45m would not error : set properties are mutable after initialization 


        Console.WriteLine(
            $"Product ID = {dump.ProductId} \nRunTime = {dump.RunTime} \nPrice = {dump.Price}"
            );
    }


}
