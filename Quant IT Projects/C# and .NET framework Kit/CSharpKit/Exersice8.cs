// File IO (sync)
public static class Exersice8
{
    public static void Run()
    {
        string path = "sample_dump.json";

        string content = File.ReadAllText(path);

        string preview = content.Length > 100 //is the content making more than 100 char ? 
            ? content.Substring(0, 100) // if yes print the 100 first
            : content; // else print it entire ;           ? & : are abreviations in c# to go faster than if/else

        Console.WriteLine(preview);
    }
}
