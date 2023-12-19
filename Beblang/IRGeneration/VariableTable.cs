using System.Diagnostics.CodeAnalysis;

namespace Beblang.IRGeneration;

public class VariableTable
{
    private readonly Stack<Dictionary<string, TypeValue>> _variables = new();

    public VariableTable()
    {
        EnterScope();
    }
    
    public void Define(string name, LLVMTypeRef type, LLVMValueRef variable)
    {
        var currentScope = _variables.Peek();
        currentScope[name] = new TypeValue(type, variable);
    }
    
    public bool IsDefined(string name, [NotNullWhen(true)] out TypeValue? variable)
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
        _variables.Push(new Dictionary<string, TypeValue>());
    }

    public void ExitScope()
    {
        _variables.Pop();
    }
}