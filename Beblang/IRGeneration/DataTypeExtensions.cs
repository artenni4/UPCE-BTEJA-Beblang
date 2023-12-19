using Beblang.Semantics;

namespace Beblang.IRGeneration;

public static class DataTypeExtensions
{
    public static LLVMValueRef GetDefaultValue(this DataType variableDataType)
    {
        var llvmType = variableDataType.ToLlvmType();
        if (variableDataType == DataType.Integer)
        {
            return LLVMValueRef.CreateConstInt(llvmType, 0);
        }
        
        if (variableDataType == DataType.Real)
        {
            return LLVMValueRef.CreateConstReal(llvmType, 0);
        }
        
        if (variableDataType == DataType.String)
        {
            return LLVMValueRef.CreateConstNull(llvmType);
        }
        
        throw new NotSupportedException($"Type {variableDataType} is not supported");
    }
    
    public static LLVMTypeRef ToLlvmType(this DataType variableDataType)
    {
        if (variableDataType == DataType.Integer)
        {
            return LLVMTypeRef.Int32;
        }
        
        if (variableDataType == DataType.Real)
        {
            return LLVMTypeRef.Double;
        }
        
        if (variableDataType == DataType.String)
        {
            return LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);
        }

        if (variableDataType == DataType.Void)
        {
            return LLVMTypeRef.Void;
        }
        
        throw new NotSupportedException($"Type {variableDataType} is not supported");
    }
}