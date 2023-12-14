using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Beblang.Semantics;

public class SymbolTable
{
    private readonly Stack<Dictionary<string, ISymbolInfo>> _scopes = new();

    public SymbolTable()
    {
        EnterScope();
    }

    public void Define(ISymbolInfo symbolInfo)
    {
        Debug.WriteLine($"Defining {symbolInfo.Name}, at scope {_scopes.Count}");
        var currentScope = _scopes.Peek();
        if (currentScope.TryGetValue(symbolInfo.Name, out var existingSymbolInfo))
        {
            throw new SemanticException(existingSymbolInfo.Context,
                symbolInfo.Context is null 
                    ? $"Symbol {symbolInfo.Name} is already defined" 
                    : $"Symbol {symbolInfo.Name} at line {symbolInfo.Context.Start.Line} is already defined");
        }
        
        currentScope[symbolInfo.Name] = symbolInfo;
    }
    
    public bool IsDefined(string name, [NotNullWhen(true)] out ISymbolInfo? symbolInfo)
    {
        foreach (var scope in _scopes)
        {
            if (scope.TryGetValue(name, out symbolInfo))
            {
                return true;
            }
        }

        symbolInfo = null;
        return false;
    }

    public void EnterScope()
    {
        _scopes.Push(new Dictionary<string, ISymbolInfo>());
    }

    public void ExitScope()
    {
        _scopes.Pop();
    }
}