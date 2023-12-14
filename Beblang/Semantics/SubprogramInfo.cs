namespace Beblang.Semantics;

public record SubprogramInfo(string Name, ParserRuleContext? Context, IReadOnlyList<VariableInfo> Parameters, DataType ReturnType) : ISymbolInfo;
