namespace Beblang;

public static class Tests
{
    public static void TestFunctionCall()
    {
        var module = LLVMModuleRef.CreateWithName("module");
        var builder = module.Context.CreateBuilder();
    
        var testFuncType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Void, Array.Empty<LLVMTypeRef>());
        var testFunc = module.AddFunction("TestFunc", testFuncType);
    
        var functionType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Void, Array.Empty<LLVMTypeRef>());
        var function = module.AddFunction("main", functionType);
        var entryBlock = function.AppendBasicBlock("entry");
    
        builder.PositionAtEnd(entryBlock);
        builder.BuildCall2(testFuncType, testFunc, Array.Empty<LLVMValueRef>());
    
        module.Dump();
    }
}