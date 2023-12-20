using Beblang.Semantics;

namespace Beblang.IRGeneration;

public class PredefinedValues
{
    private readonly LLVMValueRef _formatString;
    private readonly LLVMValueRef _formatReal;
    private readonly LLVMValueRef _formatInteger;
    private readonly FunctionData _printf;
    private readonly FunctionData _scanf;
    private readonly FunctionData _exit;

    public PredefinedValues(LLVMModuleRef module)
    {
        var printfType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, new[] { LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0) }, true);
        _printf = new FunctionData(printfType, module.AddFunction("printf", printfType));
        
        var scanfType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, new[] { LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0) }, true);
        _scanf = new FunctionData(scanfType, module.AddFunction("scanf", scanfType));
        
        var exitType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Void, new[] { LLVMTypeRef.Int32 });
        _exit = new FunctionData(exitType, module.AddFunction("exit", exitType));

        _formatString = module.CreateGlobalString("%s");
        _formatReal = module.CreateGlobalString("%f");
        _formatInteger = module.CreateGlobalString("%d");
    }
    
    public bool TryInvokeBuiltInSubprogram(LLVMBuilderRef builder, SubprogramInfo subprogramInfo, LLVMValueRef[] arguments, out ITypeData? result)
    {
        if (subprogramInfo == BuiltInSymbols.PrintString)
        {
            var printfArguments = new[] { _formatString, arguments[0] };
            builder.BuildCall2(_printf.ValueType, _printf.Reference, printfArguments);
            result = default;
            return true;
        }
        
        if (subprogramInfo == BuiltInSymbols.PrintInteger)
        {
            var printfArguments = new[] { _formatInteger, arguments[0] };
            builder.BuildCall2(_printf.ValueType, _printf.Reference, printfArguments);
            result = default;
            return true;
        }
        
        if (subprogramInfo == BuiltInSymbols.PrintReal)
        {
            var printfArguments = new[] { _formatReal, arguments[0] };
            builder.BuildCall2(_printf.ValueType, _printf.Reference, printfArguments);
            result = default;
            return true;
        }

        if (subprogramInfo == BuiltInSymbols.ReadInteger)
        {
            var variable = builder.BuildAlloca(LLVMTypeRef.Int32);
            var scanfArguments = new[] { _formatInteger, variable };
            builder.BuildCall2(_scanf.ValueType, _scanf.Reference, scanfArguments);
            result = new PointerData(LLVMTypeRef.Int32, variable, IsValuePointer: true);
            return true;
        }
        
        if (subprogramInfo == BuiltInSymbols.ReadReal)
        {
            var variable = builder.BuildAlloca(LLVMTypeRef.Double);
            var scanfArguments = new[] { _formatReal, variable };
            builder.BuildCall2(_scanf.ValueType, _scanf.Reference, scanfArguments);
            result = new PointerData(LLVMTypeRef.Double, variable, IsValuePointer: true);
            return true;
        }

        if (subprogramInfo == BuiltInSymbols.IntegerToReal)
        {
            result = new ValueData(LLVMTypeRef.Double, builder.BuildSIToFP(arguments[0], LLVMTypeRef.Double));
            return true;
        }

        if (subprogramInfo == BuiltInSymbols.RealToInteger)
        {
            result = new ValueData(LLVMTypeRef.Int32, builder.BuildFPToSI(arguments[0], LLVMTypeRef.Int32));
            return true;
        }

        if (subprogramInfo == BuiltInSymbols.Halt)
        {
            builder.BuildCall2(_exit.ValueType, _exit.Reference, arguments);
            result = default;
            return true;
        }
        
        result = default;
        return false;
    }
}