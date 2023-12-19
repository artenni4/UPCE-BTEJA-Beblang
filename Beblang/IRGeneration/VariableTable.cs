using System.Diagnostics.CodeAnalysis;

namespace Beblang.IRGeneration;

public class VariableTable
{
    private readonly Stack<Dictionary<string, ITypeData>> _variables = new();

    public VariableTable()
    {
        EnterScope();
    }
    
    public void Define(string name, ITypeData variable)
    {
        var currentScope = _variables.Peek();
        currentScope[name] = variable;
    }
    
    public bool IsDefined(string name, [NotNullWhen(true)] out ITypeData? variable)
    {
        foreach (var scope in _variables)
        {
            if (scope.TryGetValue(name, out variable))
            {
                return true;
            }
        }

        variable = null;
        return false;
    }

    public void EnterScope()
    {
        _variables.Push(new Dictionary<string, ITypeData>());
    }

    public void ExitScope()
    {
        _variables.Pop();
    }
}