// LINQ in greeks
public static class Exersice5
{
    public static void Run()
    {
        var greeks = new Dictionary<string, decimal>
        {
          { "Delta", 0.52m },
          { "Vega", 0.18m },
          { "Gamma", 0.03m },
          { "Theta", -0.01m },
          { "Rho",   0.07m }
        };

        var topGreeks = greeks
            .OrderByDescending(kv => Math.Abs(kv.Value))
            .Take(3);


        foreach (var kv in topGreeks)
        {
            Console.WriteLine($"{kv.Key} = {kv.Value}");
        }
        
    }

}