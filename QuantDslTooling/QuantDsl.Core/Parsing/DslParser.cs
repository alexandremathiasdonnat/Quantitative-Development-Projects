using System.IO;
using Antlr4.Runtime;
using QuantDsl.Core.Diagnostics;

namespace QuantDsl.Core.Parsing;

public static class DslParser
{
    public static (QuantDslParser.FileContext? Tree, CollectingErrorListener Errors) ParseFromFile(string path)
    {
        var input = File.ReadAllText(path);

        var inputStream = new AntlrInputStream(input);
        var lexer = new QuantDslLexer(inputStream);

        var tokenStream = new CommonTokenStream(lexer);
        var parser = new QuantDslParser(tokenStream);

        // Collect syntax errors
        var errorListener = new CollectingErrorListener();
        parser.RemoveErrorListeners();           // remove default console spam
        parser.AddErrorListener(errorListener);  // add our collector

        var tree = parser.file();
        return (tree, errorListener);
    }
}
