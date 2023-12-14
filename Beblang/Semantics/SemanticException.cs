namespace Beblang.Semantics;

public class SemanticException : Exception
{
    public SemanticException(ParserRuleContext? context, string? message) 
        : base(context is null ? message : $"{message} at line {context.Start.Line} column {context.Start.Column}")
    {
    }

    public SemanticException(ParserRuleContext? context, string? message, Exception? innerException) 
        : base(context is null ? message : $"{message} at line {context.Start.Line} column {context.Start.Column}", innerException)
    {
    }
}