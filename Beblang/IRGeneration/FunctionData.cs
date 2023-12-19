namespace Beblang.IRGeneration;

public record FunctionData(LLVMTypeRef ValueType, LLVMValueRef Reference) : ITypeData;