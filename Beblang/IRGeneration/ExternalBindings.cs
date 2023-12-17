namespace Beblang.IRGeneration;

public class ExternalBindings
{
    public LLVMValueRef printf { get; }

    public ExternalBindings(LLVMModuleRef module)
    {
        var printfType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, new[] { LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0) }, true);
        printf = module.AddFunction("printf", printfType);
    }
}