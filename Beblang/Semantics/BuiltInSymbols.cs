namespace Beblang.Semantics;

public static class BuiltInSymbols
{
    public static IReadOnlyList<ISymbolInfo> Symbols { get; } = new[]
    {
        new SubprogramInfo("PrintLine", Context: null, new []{ new VariableInfo("text", Context: null, DataType.String) }, ReturnType: null),
        new SubprogramInfo("Print", Context: null, new []{ new VariableInfo("text", Context: null, DataType.String) }, ReturnType: null),
        new SubprogramInfo("ReadLine", Context: null, new []{ new VariableInfo("text", Context: null, DataType.String) }, ReturnType: null),
        new SubprogramInfo("HALT", Context: null, new []{ new VariableInfo("code", Context: null, DataType.Integer) }, ReturnType: null),
        
        new SubprogramInfo("INTEGER_TO_STRING", Context: null, new []{ new VariableInfo("value", Context: null, DataType.Integer) }, ReturnType: DataType.String),
        new SubprogramInfo("REAL_TO_STRING", Context: null, new []{ new VariableInfo("value", Context: null, DataType.Real) }, ReturnType: DataType.String),
        new SubprogramInfo("BOOLEAN_TO_STRING", Context: null, new []{ new VariableInfo("value", Context: null, DataType.Boolean) }, ReturnType: DataType.String),
    };
}