namespace Beblang.Semantics;

public class SemanticError
{
    private readonly ParserRuleContext? _context;
    private readonly string _message;

    public SemanticError(string message) 
        : this(null, message)
    {
    }

    public SemanticError(ParserRuleContext? context, string message)
    {
        _context = context;
        _message = message;
    }

    public override string ToString()
    {
        return _context is null 
            ? _message 
            : $"{_message} at line {_context.Start.Line} column {_context.Start.Column}";
    }
}