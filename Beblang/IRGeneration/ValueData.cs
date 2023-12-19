namespace Beblang.IRGeneration;

public record ValueData(LLVMTypeRef ValueType, LLVMValueRef Value) : ITypeData;