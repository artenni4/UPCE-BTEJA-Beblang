using System.Globalization;
using System.Text.RegularExpressions;
using Beblang.Semantics;

namespace Beblang.IRGeneration;

public class BeblangIrGenerationVisitor : BeblangBaseVisitor<ITypeData?>
{
    public LLVMModuleRef Module => _module;
    
    private LLVMModuleRef _module;
    private LLVMBuilderRef _builder;
    private readonly VariableTable _variableTable = new();
    private readonly AnnotationTable _annotationTable;
    private PredefinedValues _predefinedValues = null!;
    private readonly Dictionary<string, PointerData> _stringLiterals = new();
    
    private bool _globalScope = true;
    
    public BeblangIrGenerationVisitor(AnnotationTable annotationTable)
    {
        _annotationTable = annotationTable;
    }

    public override ITypeData? VisitModule(BeblangParser.ModuleContext context)
    {
        var moduleInfo = _annotationTable.GetSymbol<ModuleInfo>(context);
        _module = LLVMModuleRef.CreateWithName(moduleInfo.Name);
        _predefinedValues = new PredefinedValues(_module);
        _builder = _module.Context.CreateBuilder();
        context.moduleStatements().Accept(this);

        return default;
    }

    public override ITypeData? VisitModuleStatements(BeblangParser.ModuleStatementsContext context)
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
        EnsureVoidReturn(mainFunction);
        _variableTable.ExitScope();
        
        return default;
    }

    public override ITypeData? VisitSubprogram(BeblangParser.SubprogramContext context)
    {
        var subprogramInfo = _annotationTable.GetSymbol<SubprogramInfo>(context);

        if (!_variableTable.IsDefined(subprogramInfo.Name, out var symbolTypeData))
        {
            var subprogramType = LLVMTypeRef.CreateFunction(
                ReturnType: subprogramInfo.ReturnType.ToLlvmType(), 
                ParamTypes: subprogramInfo.Parameters.Select(parameter => parameter.DataType.ToLlvmType()).ToArray());
            var subprogramValue = _module.AddFunction(subprogramInfo.Name, subprogramType);
            
            symbolTypeData = new FunctionData(subprogramType, subprogramValue);
            _variableTable.Define(subprogramInfo.Name, symbolTypeData);
        }

        if (symbolTypeData is not FunctionData functionData)
        {
            throw new InvalidOperationException($"Symbol '{subprogramInfo.Name}' is not a function.");
        }

        var entryBlock = functionData.Reference.AppendBasicBlock("entry");
        _builder.PositionAtEnd(entryBlock);
        _variableTable.EnterScope();
        for (var i = 0; i < subprogramInfo.Parameters.Count; i++)
        {
            var parameterName = subprogramInfo.Parameters[i].Name;
            var parameter = functionData.Reference.GetParam((uint)i);
            var parameterType = subprogramInfo.Parameters[i].DataType.ToLlvmType();
            var parameterCopy = _builder.BuildAlloca(parameterType); // need to copy param to be able to modify it
            _builder.BuildStore(parameter, parameterCopy);
            _variableTable.Define(parameterName, new PointerData(parameterType, parameterCopy, IsValuePointer: true));
        }
        
        context.variableDeclarationBlock()?.Accept(this);
        context.subprogramBody().Accept(this);
        EnsureVoidReturn(functionData.Reference);
        _variableTable.ExitScope();
        return default;
    }

    private void EnsureVoidReturn(LLVMValueRef function)
    {
        // Check if function's return type is void
        if (function.TypeOf.ReturnType.Kind != LLVMTypeKind.LLVMVoidTypeKind)
        {
            return;
        }
        
        if (_builder.InsertBlock.LastInstruction.InstructionOpcode != LLVMOpcode.LLVMRet)
        {
            // Add a return void instruction
            _builder.BuildRetVoid();
        }
    }

    public override ITypeData? VisitSubprogramDeclaration(BeblangParser.SubprogramDeclarationContext context)
    {
        var subprogramInfo = _annotationTable.GetSymbol<SubprogramInfo>(context);
        var functionType = LLVMTypeRef.CreateFunction(
            ReturnType: subprogramInfo.ReturnType.ToLlvmType(), 
            ParamTypes: subprogramInfo.Parameters.Select(parameter => parameter.DataType.ToLlvmType()).ToArray());
        
        var function = _module.AddFunction(subprogramInfo.Name, functionType);
        _variableTable.Define(subprogramInfo.Name, new FunctionData(functionType, function));
        return default;
    }

    public override ITypeData? VisitVariableDeclarationBlock(BeblangParser.VariableDeclarationBlockContext context)
    {
        foreach (var variableDeclarationContext in context.variableDeclaration())
        {
            foreach (var variable in _annotationTable.GetSymbols<VariableInfo>(variableDeclarationContext))
            {
                var llvmType = variable.DataType.ToLlvmType();
                LLVMValueRef definedVariable;
                if (_globalScope)
                {
                    definedVariable = _module.AddGlobal(llvmType, variable.Name);
                    definedVariable.Initializer = variable.DataType.GetDefaultValue();
                    definedVariable.IsGlobalConstant = false;
                }
                else
                {
                    definedVariable = _builder.BuildAlloca(llvmType, variable.Name);
                }
                _variableTable.Define(variable.Name, new PointerData(llvmType, definedVariable, IsValuePointer: true));
            }
        }

        return default;
    }

    public override ITypeData? VisitIfStatement(BeblangParser.IfStatementContext context)
    {
        var currentFunction = _builder.InsertBlock.Parent;
        
        // handle if, multiple else if and else
        var conditionBlock = currentFunction.AppendBasicBlock("ifCondition");
        var ifBlock = currentFunction.AppendBasicBlock("if");
        var elseIfBlocks = context.elseIfStatement().Select(_ => currentFunction.AppendBasicBlock("elseIf")).ToArray();
        var elseBlock = currentFunction.AppendBasicBlock("else");
        var endBlock = currentFunction.AppendBasicBlock("end");

        // condition
        _builder.BuildBr(conditionBlock);
        _builder.PositionAtEnd(conditionBlock);
        var condition = GetValue(context.expression().Accept(this)!);
        _builder.BuildCondBr(condition, ifBlock, elseBlock);
        
        // if
        _builder.PositionAtEnd(ifBlock);
        foreach (var statementContext in context.statement())
        {
            statementContext.Accept(this);
        }
        if (_builder.InsertBlock.LastInstruction.InstructionOpcode != LLVMOpcode.LLVMRet)
        {
            _builder.BuildBr(elseIfBlocks.Any() ? elseIfBlocks.First() : endBlock);
        }
        
        // else if
        for (var i = 0; i < context.elseIfStatement().Length; i++)
        {
            _builder.PositionAtEnd(elseIfBlocks[i]);
            var elseIfStatement = context.elseIfStatement(i);
            var elseIfCondition = GetValue(elseIfStatement.expression().Accept(this)!);
            foreach (var statementContext in elseIfStatement.statement())
            {
                statementContext.Accept(this);
            }

            if (_builder.InsertBlock.LastInstruction.InstructionOpcode != LLVMOpcode.LLVMRet)
            {
                _builder.BuildCondBr(elseIfCondition, ifBlock, i == context.elseIfStatement().Length - 1 ? elseBlock : elseIfBlocks[i + 1]);
            }
        }
        
        // else
        _builder.PositionAtEnd(elseBlock);
        var elseStatement = context.elseStatement();
        if (elseStatement is not null)
        {
            foreach (var statementContext in elseStatement.statement())
            {
                statementContext.Accept(this);
            }
        }
        
        if (_builder.InsertBlock.LastInstruction.InstructionOpcode != LLVMOpcode.LLVMRet)
        {
            _builder.BuildBr(endBlock);
        }
        
        // end
        _builder.PositionAtEnd(endBlock);
        return default;
    }

    public override ITypeData? VisitWhileStatement(BeblangParser.WhileStatementContext context)
    {
        var currentFunction = _builder.InsertBlock.Parent;
        
        var conditionBlock = currentFunction.AppendBasicBlock("whileCondition");
        var whileBlock = currentFunction.AppendBasicBlock("while");
        var endBlock = currentFunction.AppendBasicBlock("end");

        // condition
        _builder.BuildBr(conditionBlock);
        _builder.PositionAtEnd(conditionBlock);
        var condition = GetValue(context.expression().Accept(this)!);
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

    public override ITypeData? VisitReturnStatement(BeblangParser.ReturnStatementContext context)
    {
        var returnTypeData = context.expression()?.Accept(this);
        if (returnTypeData is null)
        {
            _builder.BuildRetVoid();
        }
        else
        {
            var returnValue = GetValue(returnTypeData);
            _builder.BuildRet(returnValue);
        }
        
        return default;
    }

    public override ITypeData? VisitAssignment(BeblangParser.AssignmentContext context)
    {
        var variableName = GetPointer(context.designator().Accept(this)!);
        var value = GetValue(context.expression().Accept(this)!);
        _builder.BuildStore(value, variableName);
        
        return default;
    }

    public override ITypeData VisitExpression(BeblangParser.ExpressionContext context)
    {
        var leftTypeData = context.simpleExpression(0).Accept(this)!;
        if (context.simpleExpression().Length == 1)
        {
            return leftTypeData;
        }

        LLVMValueRef? resultValue = null;
        var leftValue = GetValue(leftTypeData);
        var rightValue = GetValue(context.simpleExpression(1).Accept(this)!);
        var dataType = _annotationTable.GetType(context.simpleExpression(0));
        var op = context.comparisonOp().GetText();
        if (dataType == DataType.Integer)
        {
            resultValue = op switch
            {
                "="  => _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, leftValue, rightValue),
                "#"  => _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, leftValue, rightValue),
                "<"  => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, leftValue, rightValue),
                "<=" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLE, leftValue, rightValue),
                ">"  => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGT, leftValue, rightValue),
                ">=" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, leftValue, rightValue),
                _ => throw new NotSupportedException($"Operator {op} is not supported")
            };

        }
        
        if (dataType == DataType.Real)
        {
            resultValue = op switch
            {
                "="  => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ, leftValue, rightValue),
                "#"  => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealONE, leftValue, rightValue),
                "<"  => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLT, leftValue, rightValue),
                "<=" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLE, leftValue, rightValue),
                ">"  => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGT, leftValue, rightValue),
                ">=" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGE, leftValue, rightValue),
                _ => throw new NotSupportedException($"Operator {op} is not supported")
            };
        }

        if (resultValue.HasValue)
        {
            return new ValueData(LLVMTypeRef.Int1, resultValue.Value); // return bool
        }
        
        throw new NotSupportedException($"Data type {dataType} does not support comparison");
    }

    public override ITypeData VisitSimpleExpression(BeblangParser.SimpleExpressionContext context)
    {
        var dataType = _annotationTable.GetType(context);
        var resultTypeData = context.term(0).Accept(this)!;
        var unaryOp = context.unaryOp()?.GetText();
        if (context.term().Length == 1)
        {
            if (unaryOp == "-")
            {
                if (dataType == DataType.Integer)
                {
                    return new ValueData(resultTypeData.ValueType, _builder.BuildNeg(GetValue(resultTypeData)));
                }
                if (dataType == DataType.Real)
                {
                    return new ValueData(resultTypeData.ValueType, _builder.BuildFNeg(GetValue(resultTypeData)));
                }
                
                throw new NotSupportedException($"Data type {dataType} does not support unary minus");
            }
            return resultTypeData;
        }
        
        var resultValue = GetValue(resultTypeData);
        if (context.unaryOp() is not null && context.unaryOp().GetText() == "-")
        {
            resultValue = _builder.BuildNeg(resultValue);
        }
        
        
        for (var i = 1; i < context.term().Length; i++)
        {
            var rightValue = GetValue(context.term(i).Accept(this)!);
            var op = context.binaryOp(i - 1).GetText();
            if (dataType == DataType.Integer)
            {
                resultValue = op switch
                {
                    "+" => _builder.BuildAdd(resultValue, rightValue),
                    "-" => _builder.BuildSub(resultValue, rightValue),
                    _ => throw new NotSupportedException($"Operator {op} is not supported")
                };
            }
            else if (dataType == DataType.Real)
            {
                resultValue = op switch
                {
                    "+" => _builder.BuildFAdd(resultValue, rightValue),
                    "-" => _builder.BuildFSub(resultValue, rightValue),
                    _ => throw new NotSupportedException($"Operator {op} is not supported")
                };
            }
            else if (dataType == DataType.Boolean)
            {
                resultValue = op switch
                {
                    "OR" => _builder.BuildOr(resultValue, rightValue),
                    _ => throw new NotSupportedException($"Operator {op} is not supported")
                };
            }
            else
            {
                throw new NotSupportedException($"Data type {dataType} does not support binary operations");
            }
        }

        return new ValueData(resultTypeData.ValueType, resultValue);
    }

    public override ITypeData VisitTerm(BeblangParser.TermContext context)
    {
        var dataType = _annotationTable.GetType(context);
        var resultTypeData = context.factor(0).Accept(this)!;
        if (context.factor().Length == 1)
        {
            return resultTypeData;
        }

        var resultValue = GetValue(resultTypeData);
        for (var i = 1; i < context.factor().Length; i++)
        {
            var rightValue = GetValue(context.factor(i).Accept(this)!);
            var op = context.termOp(i - 1).GetText();
            
            if (dataType == DataType.Integer)
            {
                resultValue = op switch
                {
                    "*" => _builder.BuildMul(resultValue, rightValue),
                    "/" => _builder.BuildSDiv(resultValue, rightValue),
                    "MOD" => _builder.BuildSRem(resultValue, rightValue),
                    _ => throw new NotSupportedException($"Operator {op} is not supported")
                };
            }
            else if (dataType == DataType.Real)
            {
                resultValue = op switch
                {
                    "*" => _builder.BuildFMul(resultValue, rightValue),
                    "/" => _builder.BuildFDiv(resultValue, rightValue),
                    _ => throw new NotSupportedException($"Operator {op} is not supported")
                };
            }
            else if (dataType == DataType.Boolean)
            {
                resultValue = op switch
                {
                    "AND" => _builder.BuildAnd(resultValue, rightValue),
                    _ => throw new NotSupportedException($"Operator {op} is not supported")
                };
            }
            else
            {
                throw new NotSupportedException($"Data type {dataType} does not support binary operations");
            }
        }

        return new ValueData(resultTypeData.ValueType, resultValue);
    }

    public override ITypeData? VisitFactor(BeblangParser.FactorContext context)
    {
        if (context.literal() is not null)
        {
            return context.literal().Accept(this);
        }
        
        if (context.designator() is not null)
        {
            return context.designator().Accept(this);
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

    public override ITypeData VisitDesignator(BeblangParser.DesignatorContext context)
    {
        var variableName = context.IDENTIFIER().GetText();
        if (!_variableTable.IsDefined(variableName, out var typeValue))
        {
            throw new InvalidOperationException($"Variable '{variableName}' not defined.");
        }
        
        foreach (var selectorContext in context.selector())
        {
            if (typeValue is not PointerData pointerData)
            {
                throw new InvalidOperationException($"Variable '{variableName}' is not a pointer.");
            }

            var index = GetValue(selectorContext.expression().Accept(this)!);
            var zero = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0);
            var elementPointer = _builder.BuildGEP2(pointerData.ValueType, pointerData.Pointer, new [] { zero, index });
            
            typeValue = new PointerData(pointerData.ValueType.ElementType, elementPointer, IsValuePointer: true);
        }

        return typeValue;
    }

    public override ITypeData? VisitSubprogramCall(BeblangParser.SubprogramCallContext context)
    {
        var subprogramName = context.designator().IDENTIFIER().GetText();
        var subprogramInfo = _annotationTable.GetSymbol<SubprogramInfo>(context);
        var arguments = context.expressionList()?.expression()
            .Select(expressionContext => expressionContext.Accept(this)!)
            .Select(argument =>
            {
                TryGetValueIfPointer(argument, out var value);
                return value;
            })
            .ToArray() ?? Array.Empty<LLVMValueRef>();

        if (_predefinedValues.TryInvokeBuiltInSubprogram(_builder, subprogramInfo, arguments, out var result))
        {
            return result;
        }
        
        if (!_variableTable.IsDefined(subprogramName, out var typeData) || typeData is not FunctionData functionData)
        {
            throw new InvalidOperationException($"Subprogram '{subprogramName}' not defined.");
        }

        var returnValue = _builder.BuildCall2(functionData.ValueType, functionData.Reference, arguments);
        if (subprogramInfo.ReturnType == DataType.Void)
        {
            return default;
        }
        
        if (returnValue.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind)
        {
            return new PointerData(functionData.ValueType, returnValue, IsValuePointer: false);
        }
        
        return new ValueData(functionData.ValueType, returnValue);
    }

    public override ITypeData VisitLiteral(BeblangParser.LiteralContext context)
    {
        if (context.INTEGER_LITERAL() is not null)
        {
            var value = int.Parse(context.INTEGER_LITERAL().GetText(), CultureInfo.InvariantCulture);
            return new ValueData(LLVMTypeRef.Int32, LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (ulong)value));
        }
        
        if (context.REAL_LITERAL() is not null)
        {
            var value = double.Parse(context.REAL_LITERAL().GetText(), CultureInfo.InvariantCulture);
            return new ValueData(LLVMTypeRef.Double, LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, value));
        }

        if (context.STRING_LITERAL() is not null)
        {
            var str = Regex.Unescape(context.STRING_LITERAL().GetText().Trim('"')); // unescape to real escape sequences
            if (_stringLiterals.TryGetValue(str, out var stringLiteral))
            {
                return stringLiteral;
            }
            
            stringLiteral = new PointerData(LLVMTypeRef.Int8, _module.CreateGlobalString(str), IsValuePointer: false);
            _stringLiterals[str] = stringLiteral;
            return stringLiteral;
        }
        
        throw new NotSupportedException($"Literal {context.GetText()} is not supported");
    }
    
    private bool TryGetValueIfPointer(ITypeData typeData, out LLVMValueRef value)
    {
        if (typeData is ValueData valueData)
        {
            value = valueData.Value;
            return true;
        }

        if (typeData is PointerData pointerData)
        {
            if (pointerData.IsValuePointer)
            {
                value = _builder.BuildLoad2(typeData.ValueType, pointerData.Pointer);
                return true;
            }

            value = pointerData.Pointer;
            return false;
        }

        throw new InvalidOperationException("TypeValue is not a value nor a pointer");
    }

    private LLVMValueRef GetValue(ITypeData typeData)
    {
        if (TryGetValueIfPointer(typeData, out var value))
        {
            return value;
        }
        
        throw new InvalidOperationException("TypeValue is not a value");
    }

    private static LLVMValueRef GetPointer(ITypeData typeData)
    {
        if (typeData is not PointerData pointerData)
        {
            throw new InvalidOperationException("TypeValue is not a pointer");
        }

        return pointerData.Pointer;
    }
}