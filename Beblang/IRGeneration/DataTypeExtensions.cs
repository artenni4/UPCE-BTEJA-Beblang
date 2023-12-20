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
        
        if (variableDataType.IsArray(out var arrayElementType))
        {
            var llvmElementType = arrayElementType.OfType.ToLlvmType();
            var elementDefaultValue = arrayElementType.OfType.GetDefaultValue();
    
            // Generate a list of default values for each element in the array
            var elements = Enumerable.Repeat(elementDefaultValue, arrayElementType.Size).ToArray();

            // Create a constant array with these elements
            var llvmArray = LLVMValueRef.CreateConstArray(llvmElementType, elements);
            return llvmArray;
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
        
        if (variableDataType.IsArray(out var arrayElementType))
        {
            var llvmElementType = arrayElementType.OfType.ToLlvmType();
            return LLVMTypeRef.CreateArray(llvmElementType, (uint)arrayElementType.Size);
        }

        if (variableDataType == DataType.Void)
        {
            return LLVMTypeRef.Void;
        }
        
        throw new NotSupportedException($"Type {variableDataType} is not supported");
    }
}