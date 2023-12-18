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
    private readonly VariableTable _variableTable = new();
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
        
        _variableTable.EnterScope();
        context.moduleBody.Accept(this);
        _builder.BuildRetVoid();
        _variableTable.ExitScope();
        
        return default;
    }

    public override LLVMValueRef VisitSubprogram(BeblangParser.SubprogramContext context)
    {
        var subprogramInfo = _annotationTable.GetSymbol<SubprogramInfo>(context);

        if (!_variableTable.IsDefined(subprogramInfo.Name, out var subprogram))
        {
            var subprogramType = LLVMTypeRef.CreateFunction(
                ReturnType: ToLLVMType(subprogramInfo.ReturnType), 
                ParamTypes: subprogramInfo.Parameters.Select(parameter => ToLLVMType(parameter.DataType)).ToArray());
            var subprogramValue = _module.AddFunction(subprogramInfo.Name, subprogramType);
            
            subprogram = new TypeValue(subprogramType, subprogramValue);
            _variableTable.Define(subprogramInfo.Name, subprogramType, subprogramValue);
        }
        
        var entryBlock = subprogram.Value.AppendBasicBlock("entry");
        _builder.PositionAtEnd(entryBlock);
        _variableTable.EnterScope();
        for (var i = 0; i < subprogramInfo.Parameters.Count; i++)
        {
            var parameterName = subprogramInfo.Parameters[i].Name;
            var parameter = subprogram.Value.GetParam((uint)i);
            var parameterType = ToLLVMType(subprogramInfo.Parameters[i].DataType);
            _variableTable.Define(parameterName, parameterType, parameter);
        }
        
        context.variableDeclarationBlock()?.Accept(this);
        context.subprogramBody().Accept(this);
        _variableTable.ExitScope();
        return default;
    }

    public override LLVMValueRef VisitSubprogramDeclaration(BeblangParser.SubprogramDeclarationContext context)
    {
        var subprogramInfo = _annotationTable.GetSymbol<SubprogramInfo>(context);
        var functionType = LLVMTypeRef.CreateFunction(
            ReturnType: ToLLVMType(subprogramInfo.ReturnType), 
            ParamTypes: subprogramInfo.Parameters.Select(parameter => ToLLVMType(parameter.DataType)).ToArray());
        
        var function = _module.AddFunction(subprogramInfo.Name, functionType);
        _variableTable.Define(subprogramInfo.Name, functionType, function);
        return default;
    }

    public override LLVMValueRef VisitVariableDeclarationBlock(BeblangParser.VariableDeclarationBlockContext context)
    {
        foreach (var variableDeclarationContext in context.variableDeclaration())
        {
            foreach (var variable in _annotationTable.GetSymbols<VariableInfo>(variableDeclarationContext))
            {
                var llvmType = ToLLVMType(variable.DataType);
                LLVMValueRef value;
                if (_globalScope)
                {
                    value = _module.AddGlobal(llvmType, variable.Name);
                    value.Initializer = GetDefaultValue(variable.DataType);
                    value.IsGlobalConstant = false;
                }
                else
                {
                    value = _builder.BuildAlloca(llvmType, variable.Name);
                }
                _variableTable.Define(variable.Name, llvmType, value);
            }
        }

        return default;
    }

    public override LLVMValueRef VisitIfStatement(BeblangParser.IfStatementContext context)
    {
        var currentFunction = _builder.InsertBlock.Parent;
        
        // handle if, multiple else if and else
        var conditionBlock = currentFunction.AppendBasicBlock("ifCondition");
        var ifBlock = currentFunction.AppendBasicBlock("if");
        var elseIfBlocks = context.elseIfStatement().Select(_ => currentFunction.AppendBasicBlock("elseIf")).ToArray();
        var elseBlock = currentFunction.AppendBasicBlock("else");
        var endBlock = currentFunction.AppendBasicBlock("end");

        // condition
        _builder.PositionAtEnd(conditionBlock);
        var condition = context.expression().Accept(this);
        _builder.BuildCondBr(condition, ifBlock, elseBlock);
        
        // if
        _builder.PositionAtEnd(ifBlock);
        context.statement(0).Accept(this);
        _builder.BuildBr(endBlock);
        
        // else if
        for (var i = 0; i < context.elseIfStatement().Length; i++)
        {
            _builder.PositionAtEnd(elseIfBlocks[i]);
            var elseIfCondition = context.elseIfStatement(i).expression().Accept(this);
            _builder.BuildCondBr(elseIfCondition, ifBlock, i == context.elseIfStatement().Length - 1 ? elseBlock : elseIfBlocks[i + 1]);
        }
        
        // else
        _builder.PositionAtEnd(elseBlock);
        context.statement().Last().Accept(this);
        _builder.BuildBr(endBlock);
        
        // end
        _builder.PositionAtEnd(endBlock);
        return default;
    }

    public override LLVMValueRef VisitWhileStatement(BeblangParser.WhileStatementContext context)
    {
        var currentFunction = _builder.InsertBlock.Parent;
        
        var conditionBlock = currentFunction.AppendBasicBlock("whileCondition");
        var whileBlock = currentFunction.AppendBasicBlock("while");
        var endBlock = currentFunction.AppendBasicBlock("end");

        // condition
        _builder.PositionAtEnd(conditionBlock);
        var condition = context.expression().Accept(this);
        _builder.BuildCondBr(condition, whileBlock, endBlock);
        
        // while
        _builder.PositionAtEnd(whileBlock);
        foreach (var statementContext in context.statement())
        {
            statementContext.Accept(this);
        }
        _builder.BuildBr(conditionBlock);
        
        // end
        _builder.PositionAtEnd(endBlock);
        return default;
    }

    public override LLVMValueRef VisitReturnStatement(BeblangParser.ReturnStatementContext context)
    {
        var returnValue = context.expression()?.Accept(this);
        return returnValue is null ? _builder.BuildRetVoid() : _builder.BuildRet(returnValue.Value);
    }

    public override LLVMValueRef VisitAssignment(BeblangParser.AssignmentContext context)
    {
        var variableName = context.designator().IDENTIFIER().GetText();
        if (!_variableTable.IsDefined(variableName, out var variableAlloca))
        {
            throw new InvalidOperationException($"Variable '{variableName}' not defined.");
        }

        var value = context.expression().Accept(this);
        _builder.BuildStore(value, variableAlloca.Value);
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
            if (!_variableTable.IsDefined(variableName, out var typeValue))
            {
                throw new InvalidOperationException($"Variable '{variableName}' not defined.");
            }

            return _builder.BuildLoad2(typeValue.Type, typeValue.Value);
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
            if (!_variableTable.IsDefined(subprogramName, out var function))
            {
                throw new InvalidOperationException($"Subprogram '{subprogramName}' not defined.");
            }

            subprogram = function;
        }
        var arguments = context.expressionList()?.expression()
            .Select(expressionContext => expressionContext.Accept(this))
            .ToArray() ?? Array.Empty<LLVMValueRef>();
        
        return _builder.BuildCall2(subprogram.Type, subprogram.Value, arguments);
    }

    private bool TryGetBuiltInSubprogram(SubprogramInfo subprogramInfo, [NotNullWhen(true)] out TypeValue? subprogram)
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

        if (variableDataType == DataType.Void)
        {
            return LLVMTypeRef.Void;
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