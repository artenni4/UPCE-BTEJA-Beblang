using Beblang.Semantics;

namespace Beblang;

public class AnnotationTable
{
    private readonly Dictionary<ParserRuleContext, IReadOnlyList<ISymbolInfo>> _symbolTable = new();
    private readonly Dictionary<ParserRuleContext, DataType> _typeTable = new();

    public void AnnotateSymbols<TSymbol>(ParserRuleContext context, params TSymbol[] symbolInfo) where TSymbol : ISymbolInfo
    {
        _symbolTable[context] = symbolInfo.Cast<ISymbolInfo>().ToArray();
    }
    
    public TSymbol GetSymbol<TSymbol>(ParserRuleContext context) where TSymbol : ISymbolInfo
    {
        return (TSymbol)_symbolTable[context].Single();
    }
    
    public IReadOnlyList<TSymbol> GetSymbols<TSymbol>(ParserRuleContext context) where TSymbol : ISymbolInfo
    {
        return _symbolTable[context].Cast<TSymbol>().ToArray();
    }

    public void AnnotateType(ParserRuleContext context, DataType dataType)
    {
        _typeTable[context] = dataType;
    }

    public DataType GetType(ParserRuleContext context)
    {
        return _typeTable[context];
    }
}