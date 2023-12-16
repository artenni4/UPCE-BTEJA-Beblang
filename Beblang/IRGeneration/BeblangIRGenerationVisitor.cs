using System.Globalization;

namespace Beblang.IRGeneration;

public class BeblangIRGenerationVisitor : BeblangBaseVisitor<LLVMValueRef>
{
    private LLVMModuleRef _module;
    public LLVMModuleRef Module => _module;
    
    private LLVMBuilderRef _builder;
    
    public BeblangIRGenerationVisitor()
    {
        _module = LLVMModuleRef.CreateWithName("BeblangModule");
        _builder = _module.Context.CreateBuilder();
    }

    // public override LLVMValueRef VisitAssignment(BeblangParser.AssignmentContext context)
    // {
    //     var variableName = context.designator().IDENTIFIER().GetText();
    //     var variable = _module.GetNamedGlobal(variableName);
    //     var value = Visit(context.expression());
    //     _builder.BuildStore(value, variable);
    //     return value;
    // }

    public override LLVMValueRef VisitLiteral(BeblangParser.LiteralContext context)
    {
        if (context.INTEGER_LITERAL() is not null)
        {
            var value = int.Parse(context.INTEGER_LITERAL().GetText());
            return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (ulong)value);
        }
        
        if (context.REAL_LITERAL() is not null)
        {
            var value = double.Parse(context.REAL_LITERAL().GetText(), CultureInfo.InvariantCulture);
            return LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, value);
        }
        
        throw new NotSupportedException($"Literal {context.GetText()} is not supported");
    }
}