namespace Beblang.Semantics;

public record ModuleInfo(string Name, ParserRuleContext? Context) : ISymbolInfo;
