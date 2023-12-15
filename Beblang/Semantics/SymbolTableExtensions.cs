using System.Diagnostics.CodeAnalysis;

namespace Beblang.Semantics;

public static class SymbolTableExtensions
{
    public static SymbolTable Merge(this SymbolTable symbolTable, IEnumerable<ISymbolInfo> symbolInfos)
    {
        foreach (var symbolInfo in symbolInfos)
        {
            if (!symbolTable.TryDefine(symbolInfo, out _))
            {
                throw new Exception("Built-in symbols should not conflict with each other");
            }
        }

        return symbolTable;
    }
}