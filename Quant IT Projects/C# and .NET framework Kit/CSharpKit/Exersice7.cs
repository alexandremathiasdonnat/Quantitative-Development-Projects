// 2 Exceptions management with ParseNotional function & Execution test
public static class Exersice7
{
    public static decimal ParseNotional(string s)
    {
        if (!decimal.TryParse(s, out var value))
            throw new ArgumentException("Notional not accepted");

        if (value <= 0)
            throw new ArgumentException("Notional should be positive");

        if (value >= 99999)
        throw new ArgumentException("Notional should be lower than 99999");    

        return value;
    }
    public static void Run()
    {
        try
        {
            decimal notional = ParseNotional("1111111");
            Console.WriteLine($"Notional = {notional}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"User error : {ex.Message}");
        }
    }
}  