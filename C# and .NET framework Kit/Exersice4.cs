// List and ditionaries
public static class Exersice4
{
    public static void Run()
    {
        var prices = new List<decimal>
        {
            12.35m,
            12.40m,
            12.10m,
            12,60m
        };
        var greeks = new Dictionary<string, decimal>
        {
          { "Delta", 0.52m },
          { "Vega", 0.18m },
          { "Gamma", 0.03m },
          { "Theta", -0.01m },


        };

        foreach (var kv in greeks)
        {
            Console.WriteLine($"{kv.Key} = {kv.Value}");
        }
        
    }

}