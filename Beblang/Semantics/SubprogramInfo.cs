namespace Beblang.Semantics;

public record SubprogramInfo(
    string Name,
    ParserRuleContext? Context,
    IReadOnlyList<VariableInfo> Parameters,
    DataType ReturnType) : ISymbolInfo
{
    /// <summary>
    /// Whether the body of the subprogram has been defined.
    /// </summary>
    public bool IsDefined { get; private set; } 
    public void SetDefined() => IsDefined = true;
}
