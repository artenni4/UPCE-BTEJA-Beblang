namespace Beblang.IRGeneration;

/// <summary>
/// Represents a pointer to a value.
/// </summary>
/// <param name="ValueType">type of value, not pointer</param>
/// <param name="Pointer">pointer value</param>
/// <param name="IsValuePointer">whether the actual data is behind the pointer, indicator that value should be loaded first for performing operations</param>
public record PointerData(LLVMTypeRef ValueType, LLVMValueRef Pointer, bool IsValuePointer) : ITypeData;