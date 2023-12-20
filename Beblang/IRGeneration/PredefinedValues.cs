using Beblang.Semantics;

namespace Beblang.IRGeneration;

public class PredefinedValues
{
    private readonly LLVMValueRef _printfString;
    private readonly LLVMValueRef _printfReal;
    private readonly LLVMValueRef _printfInteger;
    private readonly FunctionData _printf;
    private readonly FunctionData _exit;

    public PredefinedValues(LLVMModuleRef module)
    {
        var printfType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, new[] { LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0) }, true);
        _printf = new FunctionData(printfType, module.AddFunction("printf", printfType));
        
        var exitType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Void, new[] { LLVMTypeRef.Int32 });
        _exit = new FunctionData(exitType, module.AddFunction("exit", exitType));

        _printfString = module.CreateGlobalString("%s");
        _printfReal = module.CreateGlobalString("%f");
        _printfInteger = module.CreateGlobalString("%d");
    }
    
    public bool TryInvokeBuiltInSubprogram(LLVMBuilderRef builder, SubprogramInfo subprogramInfo, LLVMValueRef[] arguments, out ITypeData? result)
    {
        if (subprogramInfo == BuiltInSymbols.PrintString)
        {
            var printfArguments = new[] { _printfString, arguments[0] };
            builder.BuildCall2(_printf.ValueType, _printf.Reference, printfArguments);
            result = default;
            return true;
        }
        
        if (subprogramInfo == BuiltInSymbols.PrintInteger)
        {
            var printfArguments = new[] { _printfInteger, arguments[0] };
            builder.BuildCall2(_printf.ValueType, _printf.Reference, printfArguments);
            result = default;
            return true;
        }
        
        if (subprogramInfo == BuiltInSymbols.PrintReal)
        {
            var printfArguments = new[] { _printfReal, arguments[0] };
            builder.BuildCall2(_printf.ValueType, _printf.Reference, printfArguments);
            result = default;
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