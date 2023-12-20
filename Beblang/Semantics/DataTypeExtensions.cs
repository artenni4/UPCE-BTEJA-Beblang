namespace Beblang.Semantics;

public static class DataTypeExtensions
{
    public static bool IsNumeric(this DataType dataType)
    {
        return dataType == DataType.Integer || dataType == DataType.Real;
    }
    
    public static bool IsValueType(this DataType dataType)
    {
        return dataType == DataType.Integer || dataType == DataType.Real || dataType == DataType.Boolean;
    }
}