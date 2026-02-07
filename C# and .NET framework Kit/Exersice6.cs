// Nullable (=to accept a possible null value) and null coalescing (=if null value, : know how to react)
public static class Exersice6
{
    public static void Run()
    {
        decimal? maybePrice = null;

        Console.WriteLine(maybePrice ?? 0m); // "maybePrice ?? 0m" meaning = if maybePrice has a value print it, else print 0

    }
}