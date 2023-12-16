using LLVMSharp.Interop;

namespace Beblang.IRGeneration;

public class BeblangIRGenerationVisitor : BeblangBaseVisitor<LLVMValueRef>
{
    private readonly LLVMModuleRef _module;
    private readonly LLVMBuilderRef _builder;
    
    public BeblangIRGenerationVisitor()
    {
        _module = LLVMModuleRef.CreateWithName("BeblangModule");
        _builder = _module.Context.CreateBuilder();
    }

    public override LLVMValueRef VisitLiteral(BeblangParser.LiteralContext context)
    {
        if (context.INTEGER_LITERAL() is not null)
        {
            var value = int.Parse(context.INTEGER_LITERAL().GetText());
            return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (ulong)value);
        }
        
        throw new NotSupportedException($"Literal {context.GetText()} is not supported");
    }
}