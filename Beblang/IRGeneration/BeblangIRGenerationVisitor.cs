using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Beblang.Semantics;

namespace Beblang.IRGeneration;

public class BeblangIRGenerationVisitor : BeblangBaseVisitor<LLVMValueRef>
{
    public LLVMModuleRef Module => _module;
    
    private LLVMModuleRef _module;
    private LLVMBuilderRef _builder;
    private readonly Dictionary<string, LLVMValueRef> _variables = new();
    private readonly AnnotationTable _annotationTable;
    private ExternalBindings _externalBindings = null!;
    
    private bool _globalScope = true;
    
    public BeblangIRGenerationVisitor(AnnotationTable annotationTable)
    {
        _annotationTable = annotationTable;
    }

    public override LLVMValueRef VisitModule(BeblangParser.ModuleContext context)
    {
        var moduleInfo = _annotationTable.GetSymbol<ModuleInfo>(context);
        _module = LLVMModuleRef.CreateWithName(moduleInfo.Name);
        _externalBindings = new ExternalBindings(_module);
        _builder = _module.Context.CreateBuilder();

        context.moduleStatements().Accept(this);

        return default;
    }

    public override LLVMValueRef VisitModuleStatements(BeblangParser.ModuleStatementsContext context)
    {
        context.variableDeclarationBlock()?.Accept(this);
        _globalScope = false;
        
        foreach (var subprogramDeclarationContext in context.subprogramDeclaration())
        {
            subprogramDeclarationContext.Accept(this);
        }
        foreach (var subprogramContext in context.subprogram())
        {
            subprogramContext.Accept(this);
        }

        var mainFunctionType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Void, Array.Empty<LLVMTypeRef>());
        var mainFunction = _module.AddFunction("main", mainFunctionType);
        var entryBlock = mainFunction.AppendBasicBlock("entry");
        _builder.PositionAtEnd(entryBlock);
        context.moduleBody.Accept(this);
        return default;
    }

    public override LLVMValueRef VisitVariableDeclarationBlock(BeblangParser.VariableDeclarationBlockContext context)
    {
        foreach (var variableDeclarationContext in context.variableDeclaration())
        {
            foreach (var variable in _annotationTable.GetSymbols<VariableInfo>(variableDeclarationContext))
            {
                var llvmType = ToLLVMType(variable.DataType);
                if (_globalScope)
                {
                    var global = _module.AddGlobal(llvmType, variable.Name);
                    global.Initializer = GetDefaultValue(variable.DataType);
                    global.IsGlobalConstant = false;
                    _variables[variable.Name] = global;
                }
                else
                {
                    var alloca = _builder.BuildAlloca(llvmType, variable.Name);
                    _variables[variable.Name] = alloca;
                }
            }
        }

        return default;
    }

    public override LLVMValueRef VisitAssignment(BeblangParser.AssignmentContext context)
    {
        var variableName = context.designator().IDENTIFIER().GetText();
        if (!_variables.TryGetValue(variableName, out var variableAlloca))
        {
            throw new InvalidOperationException($"Variable '{variableName}' not defined.");
        }

        var value = context.expression().Accept(this);
        _builder.BuildStore(value, variableAlloca);
        return value;
    }

    public override LLVMValueRef VisitExpression(BeblangParser.ExpressionContext context)
    {
        var left = context.simpleExpression(0).Accept(this);
        if (context.simpleExpression().Length == 1)
        {
            return left;
        }

        var right = context.simpleExpression(1).Accept(this);
        var op = context.comparisonOp().GetText();
        if (op == "=")
        {
            return _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, left, right);
        }
        if (op == "#")
        {
            return _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, left, right);
        }
        if (op == "<")
        {
            return _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, left, right);
        }
        if (op == "<=")
        {
            return _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLE, left, right);
        }
        if (op == ">")
        {
            return _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGT, left, right);
        }
        if (op == ">=")
        {
            return _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, left, right);
        }
        
        throw new NotSupportedException($"Operator {op} is not supported");
    }

    public override LLVMValueRef VisitSimpleExpression(BeblangParser.SimpleExpressionContext context)
    {
        var result = context.term(0).Accept(this);
        if (context.unaryOp() is not null && context.unaryOp().GetText() == "-")
        {
            result = _builder.BuildNeg(result);
        }
        
        if (context.term().Length == 1)
        {
            return result;
        }
        
        for (var i = 1; i < context.term().Length; i++)
        {
            var right = context.term(i).Accept(this);
            var op = context.binaryOp(i - 1);
            
            if (op.GetText() == "+")
            {
                result = _builder.BuildAdd(result, right);
            }
            else if (op.GetText() == "-")
            {
                result = _builder.BuildSub(result, right);
            }
            else if (op.GetText() == "OR")
            {
                result = _builder.BuildOr(result, right);
            }
            else
            {
                throw new NotSupportedException($"Operator {op} is not supported");
            }
        }

        return result;
    }

    public override LLVMValueRef VisitTerm(BeblangParser.TermContext context)
    {
        var result = context.factor(0).Accept(this);
        if (context.factor().Length == 1)
        {
            return result;
        }
        
        for (var i = 1; i < context.factor().Length; i++)
        {
            var right = context.factor(i).Accept(this);
            var op = context.termOp(i - 1);
            
            if (op.GetText() == "*")
            {
                result = _builder.BuildMul(result, right);
            }
            else if (op.GetText() == "/")
            {
                result = _builder.BuildSDiv(result, right);
            }
            else if (op.GetText() == "MOD")
            {
                result = _builder.BuildSRem(result, right);
            }
            else if (op.GetText() == "&")
            {
                result = _builder.BuildAnd(result, right);
            }
            else
            {
                throw new NotSupportedException($"Operator {op} is not supported");
            }
        }

        return result;
    }

    public override LLVMValueRef VisitFactor(BeblangParser.FactorContext context)
    {
        if (context.literal() is not null)
        {
            return context.literal().Accept(this);
        }
        
        if (context.designator() is not null)
        {
            var variableName = context.designator().IDENTIFIER().GetText();
            if (!_variables.TryGetValue(variableName, out var variableAlloca))
            {
                throw new InvalidOperationException($"Variable '{variableName}' not defined.");
            }

            return _builder.BuildLoad2(LLVMTypeRef.Int32, variableAlloca);
        }
        
        if (context.subprogramCall() is not null)
        {
            return context.subprogramCall().Accept(this);
        }
        
        if (context.expression() is not null)
        {
            return context.expression().Accept(this);
        }
        
        throw new NotSupportedException($"Factor {context.GetText()} is not supported");
    }

    public override LLVMValueRef VisitSubprogramCall(BeblangParser.SubprogramCallContext context)
    {
        var subprogramName = context.designator().IDENTIFIER().GetText();
        var subprogramInfo = _annotationTable.GetSymbol<SubprogramInfo>(context);
        if (!TryGetBuiltInSubprogram(subprogramInfo, out var subprogram))
        {
            subprogram = _module.GetNamedFunction(subprogramName);
        }
        var arguments = context.expressionList()?.expression()
            .Select(expressionContext => expressionContext.Accept(this))
            .ToArray() ?? Array.Empty<LLVMValueRef>();

        return _builder.BuildCall2(subprogram.TypeOf.ReturnType, subprogram, arguments);
    }

    private bool TryGetBuiltInSubprogram(SubprogramInfo subprogramInfo, out LLVMValueRef subprogram)
    {
        if (subprogramInfo.Name == "PrintLine")
        {
            subprogram = _externalBindings.printf;
            return true;
        }

        subprogram = default;
        return false;
    }

    public override LLVMValueRef VisitLiteral(BeblangParser.LiteralContext context)
    {
        if (context.INTEGER_LITERAL() is not null)
        {
            var value = int.Parse(context.INTEGER_LITERAL().GetText(), CultureInfo.InvariantCulture);
            return LLVMValueRef.CreateConstInt(_module.Context.Int32Type, (ulong)value);
        }
        
        if (context.REAL_LITERAL() is not null)
        {
            var value = double.Parse(context.REAL_LITERAL().GetText(), CultureInfo.InvariantCulture);
            return LLVMValueRef.CreateConstReal(_module.Context.DoubleType, value);
        }

        if (context.STRING_LITERAL() is not null)
        {
            var str = context.STRING_LITERAL().GetText().Trim('"') + "\0"; // Ensure null-termination
            return _builder.BuildGlobalStringPtr(str);
        }
        
        throw new NotSupportedException($"Literal {context.GetText()} is not supported");
    }

    private LLVMTypeRef ToLLVMType(DataType variableDataType)
    {
        if (variableDataType == DataType.Integer)
        {
            return LLVMTypeRef.Int32;
        }
        
        if (variableDataType == DataType.Real)
        {
            return LLVMTypeRef.Double;
        }
        
        if (variableDataType == DataType.String)
        {
            return LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);
        }
        
        throw new NotSupportedException($"Type {variableDataType} is not supported");
    }
    
    private LLVMValueRef GetDefaultValue(DataType variableDataType)
    {
        var llvmType = ToLLVMType(variableDataType);
        if (variableDataType == DataType.Integer)
        {
            return LLVMValueRef.CreateConstInt(llvmType, 0);
        }
        
        if (variableDataType == DataType.Real)
        {
            return LLVMValueRef.CreateConstReal(llvmType, 0);
        }
        
        if (variableDataType == DataType.String)
        {
            return LLVMValueRef.CreateConstNull(llvmType);
        }
        
        throw new NotSupportedException($"Type {variableDataType} is not supported");
    }
}