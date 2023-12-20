namespace Beblang.Semantics;

public static class BuiltInSymbols
{
    public static SubprogramInfo PrintString { get; } = new("PrintString", Context: null, new []{ new VariableInfo("text", Context: null, DataType.String) }, ReturnType: DataType.Void);
    public static SubprogramInfo PrintInteger { get; } = new("PrintInteger", Context: null, new []{ new VariableInfo("text", Context: null, DataType.Integer) }, ReturnType: DataType.Void);
    public static SubprogramInfo PrintReal { get; } = new("PrintReal", Context: null, new []{ new VariableInfo("text", Context: null, DataType.Real) }, ReturnType: DataType.Void);
    public static SubprogramInfo ReadInteger { get; } = new("ReadInteger", Context: null, Array.Empty<VariableInfo>(), ReturnType: DataType.Integer);
    public static SubprogramInfo ReadReal { get; } = new("ReadReal", Context: null, Array.Empty<VariableInfo>(), ReturnType: DataType.Real);
    public static SubprogramInfo Halt { get; } = new("HALT", Context: null, new []{ new VariableInfo("code", Context: null, DataType.Integer) }, ReturnType: DataType.Void);
    public static SubprogramInfo IntegerToReal { get; } = new("INTEGER_TO_REAL", Context: null, new []{ new VariableInfo("value", Context: null, DataType.Integer) }, ReturnType: DataType.Real);
    public static SubprogramInfo RealToInteger { get; } = new("REAL_TO_INTEGER", Context: null, new []{ new VariableInfo("value", Context: null, DataType.Real) }, ReturnType: DataType.Integer);
    
    public static readonly IReadOnlyList<ISymbolInfo> Symbols = new ISymbolInfo[]
    {
        PrintString,
        PrintInteger,
        PrintReal,
        ReadInteger,
        ReadReal,
        Halt,
        IntegerToReal,
        RealToInteger
    };
}