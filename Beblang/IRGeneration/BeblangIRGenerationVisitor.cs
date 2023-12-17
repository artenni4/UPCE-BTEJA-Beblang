using System.Globalization;
using Beblang.Semantics;

namespace Beblang.IRGeneration;

public class BeblangIRGenerationVisitor : BeblangBaseVisitor<LLVMValueRef>
{
    public LLVMModuleRef Module => _module;
    
    private LLVMModuleRef _module;
    private LLVMBuilderRef _builder;
    private readonly Dictionary<string, LLVMValueRef> _variables = new();
    private AnnotationTable _annotationTable;
    
    public BeblangIRGenerationVisitor(AnnotationTable annotationTable)
    {
        _annotationTable = annotationTable;
        _module = LLVMModuleRef.CreateWithName("BeblangModule");
        _builder = _module.Context.CreateBuilder();
    }

    public override LLVMValueRef VisitVariableDeclarationBlock(BeblangParser.VariableDeclarationBlockContext context)
    {
        foreach (var variableDeclarationContext in context.variableDeclaration())
        {
            foreach (var variable in _annotationTable.GetSymbols<VariableInfo>(variableDeclarationContext))
            {
                var llvmType = ToLLVMType(variable.DataType);
                var alloca = _builder.BuildAlloca(llvmType, variable.Name);
                _variables[variable.Name] = alloca;
            }
        }

        return base.VisitVariableDeclarationBlock(context);
    }

    private LLVMTypeRef ToLLVMType(DataType variableDataType)
    {
        if (variableDataType == DataType.Integer)
        {
            return _module.Context.Int32Type;
        }
        
        if (variableDataType == DataType.Real)
        {
            return _module.Context.DoubleType;
        }
        
        throw new NotSupportedException($"Type {variableDataType} is not supported");
    }

    public override LLVMValueRef VisitAssignment(BeblangParser.AssignmentContext context)
    {
        var variableName = context.designator().IDENTIFIER().GetText();
        var value = context.expression().Accept(this);

        if (!_variables.TryGetValue(variableName, out var variableAlloca))
        {
            throw new InvalidOperationException($"Variable '{variableName}' not defined.");
        }

        _builder.BuildStore(value, variableAlloca);
        return value;
    }

    public override LLVMValueRef VisitLiteral(BeblangParser.LiteralContext context)
    {
        if (context.INTEGER_LITERAL() is not null)
        {
            var value = int.Parse(context.INTEGER_LITERAL().GetText());
            return LLVMValueRef.CreateConstInt(_module.Context.Int32Type, (ulong)value);
        }
        
        if (context.REAL_LITERAL() is not null)
        {
            var value = double.Parse(context.REAL_LITERAL().GetText(), CultureInfo.InvariantCulture);
            return LLVMValueRef.CreateConstReal(_module.Context.DoubleType, value);
        }
        
        throw new NotSupportedException($"Literal {context.GetText()} is not supported");
    }
}