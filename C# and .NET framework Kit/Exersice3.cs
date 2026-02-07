// Exersice 2 : init (We assume variables are immutable (non-editable) after initialization)

public class PricingDumpInit
{
    public string ProductId { get; init; }
    public DateTime RunTime { get; init; }
    public decimal Price {get; init; }

    public PricingDumpInit(string productID, DateTime runTime, decimal price)
    {
        ProductId = productID;
        RunTime = runTime;
        Price = price;

    }
}

public static class Exersice3
{
    public static void Run()
    {
        var dump = new PricingDumpInit
        (
        "AAPL_CALL_150",
            DateTime.Now,
            12.35m

        ); 
        //dump.Price = 12.45m would error : init properties are immutable after initialization    

        Console.WriteLine(
            $"Product ID = {dump.ProductId} \nRunTime = {dump.RunTime} \nPrice = {dump.Price}"
            );
    }


}
