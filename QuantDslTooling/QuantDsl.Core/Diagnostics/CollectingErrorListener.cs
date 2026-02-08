using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;

namespace QuantDsl.Core.Diagnostics;

public sealed class CollectingErrorListener : IAntlrErrorListener<IToken>
{
    public List<SyntaxError> Errors { get; } = new();

    public void SyntaxError(
        TextWriter output,
        IRecognizer recognizer,
        IToken offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e)
    {
        Errors.Add(new SyntaxError(line, charPositionInLine, msg));
    }
}
