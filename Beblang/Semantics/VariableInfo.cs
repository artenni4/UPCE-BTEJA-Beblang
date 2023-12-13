namespace Beblang.Semantics;

public record VariableInfo(string Name, ParserRuleContext Context, DataType DataType) : ISymbolInfo;