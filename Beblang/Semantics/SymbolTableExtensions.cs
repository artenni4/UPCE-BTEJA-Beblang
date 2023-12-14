namespace Beblang.Semantics;

public static class SymbolTableExtensions
{
    public static SymbolTable Merge(this SymbolTable symbolTable, IEnumerable<ISymbolInfo> symbolInfos)
    {
        foreach (var symbolInfo in symbolInfos)
        {
            symbolTable.Define(symbolInfo);
        }

        return symbolTable;
    }
}