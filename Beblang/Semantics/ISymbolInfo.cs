namespace Beblang.Semantics;

public interface ISymbolInfo
{
    string Name { get; }
    ParserRuleContext Context { get; }
}