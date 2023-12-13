namespace Beblang.Semantics;

public class SemanticException : Exception
{
    public SemanticException(ParserRuleContext context, string? message) 
        : base($"{message} at line {context.Start.Line} column {context.Start.Column}")
    {
    }

    public SemanticException(ParserRuleContext context, string? message, Exception? innerException) 
        : base($"{message} at line {context.Start.Line} column {context.Start.Column}", innerException)
    {
    }
}