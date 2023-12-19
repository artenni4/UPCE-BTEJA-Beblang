namespace Beblang.IRGeneration;

public class PredefinedValues
{
    public LLVMValueRef PrintfString { get; }
    public LLVMValueRef PrintfReal { get; }
    public LLVMValueRef PrintfInteger { get; }
    public FunctionData Printf { get; }

    public PredefinedValues(LLVMModuleRef module)
    {
        var printfType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, new[] { LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0) }, true);
        Printf = new FunctionData(printfType, module.AddFunction("printf", printfType));

        PrintfString = module.CreateGlobalString("%s");
        PrintfReal = module.CreateGlobalString("%f");
        PrintfInteger = module.CreateGlobalString("%d");
    }
}