namespace Beblang.Semantics;

public class BeblangSemanticVisitor : BeblangBaseVisitor<Result<DataType, SemanticError>?>
{
    private readonly List<SemanticError> _errors = new();
    public IReadOnlyList<SemanticError> Errors => _errors;
    private readonly SymbolTable _symbolTable;
    private SubprogramInfo? _currentSubprogram;
    private bool _isInLoop;

    public BeblangSemanticVisitor(SymbolTable symbolTable)
    {
        _symbolTable = symbolTable;
    }

    private SemanticError AddError(ParserRuleContext context, string message)
    {
        var error = new SemanticError(context, message);
        _errors.Add(error);
        return error;
    }
    
    private SemanticError AddError(SemanticError error)
    {
        _errors.Add(error);
        return error;
    }

    public override Result<DataType, SemanticError>? VisitModule(BeblangParser.ModuleContext context)
    {
        var moduleName = context.moduleName.GetFullName();
        if (!_symbolTable.TryDefine(new ModuleInfo(moduleName, context), out var error))
        {
            return AddError(error);
        }

        context.moduleStatements().Accept(this);
        
        return null;
    }

    public override Result<DataType, SemanticError>? VisitSubprogram(BeblangParser.SubprogramContext context)
    {
        var subprogramInfo = context.subprogramHeading().GetSubprogramInfo();
        if (_symbolTable.IsDefined(subprogramInfo.Name, out var existingSymbolInfo) &&
            existingSymbolInfo is SubprogramInfo existingSubprogramInfo)
        {
            if (existingSubprogramInfo.IsDefined)
            {
                AddError(context, $"Subprogram {subprogramInfo.Name} is already defined");
            }
            existingSubprogramInfo.SetDefined();
        }
        else
        {
            if (!_symbolTable.TryDefine(subprogramInfo, out var error))
            {
                return AddError(error);
            }
        }
        
        _currentSubprogram = subprogramInfo;
        _symbolTable.EnterScope();
        
        foreach (var parameter in subprogramInfo.Parameters)
        {
            if (!_symbolTable.TryDefine(parameter, out var error))
            {
                return AddError(error);
            }
        }
        context.variableDeclarationBlock()?.Accept(this);
        context.subprogramBody().Accept(this);
        
        _symbolTable.ExitScope();
        _currentSubprogram = null;
        
        return null;
    }

    public override Result<DataType, SemanticError>? VisitSubprogramDeclaration(BeblangParser.SubprogramDeclarationContext context)
    {
        var subprogramInfo = context.subprogramHeading().GetSubprogramInfo();
        if (!_symbolTable.TryDefine(subprogramInfo, out var error))
        {
            return AddError(error);
        }

        return null;
    }

    public override Result<DataType, SemanticError>? VisitVariableDeclarationBlock(BeblangParser.VariableDeclarationBlockContext context)
    {
        foreach (var variableDeclarationContext in context.variableDeclaration())
        {
            foreach (var symbolInfo in variableDeclarationContext.GetVariableSymbolInfo())
            {
                if (!_symbolTable.TryDefine(symbolInfo, out var error))
                {
                    return AddError(error);
                }
            }
        }

        return null;
    }

    public override Result<DataType, SemanticError>? VisitIfStatement(BeblangParser.IfStatementContext context)
    {
        CheckConditionExpression(context.expression());
        VisitChildren(context);
        return null;
    }

    public override Result<DataType, SemanticError>? VisitElseIfStatement(BeblangParser.ElseIfStatementContext context)
    {
        CheckConditionExpression(context.expression());
        VisitChildren(context);
        return null;
    }

    public override Result<DataType, SemanticError>? VisitWhileStatement(BeblangParser.WhileStatementContext context)
    {
        CheckConditionExpression(context.expression());
        _isInLoop = true;
        VisitChildren(context);
        _isInLoop = false;
        return null;
    }

    private void CheckConditionExpression(BeblangParser.ExpressionContext context)
    {
        var result = context.Accept(this)!;
        if (result.IsOk(out var expressionType) && expressionType != DataType.Boolean)
        {
            AddError(context, $"Cannot use {expressionType} as condition");
        }
    }

    public override Result<DataType, SemanticError>? VisitReturnStatement(BeblangParser.ReturnStatementContext context)
    {
        var result = context.expression()?.Accept(this)!;
        if (result.IsOk(out var expressionDataType) && expressionDataType != _currentSubprogram!.ReturnType)
        {
            AddError(context, $"Cannot return {expressionDataType} from {_currentSubprogram.Name}, expected {_currentSubprogram.ReturnType}");
        }

        return null;
    }

    public override Result<DataType, SemanticError>? VisitExitStatement(BeblangParser.ExitStatementContext context)
    {
        if (!_isInLoop)
        {
            AddError(context, "Exit statement is not allowed outside of a loop");
        }
        VisitChildren(context);

        return null;
    }

    public override Result<DataType, SemanticError>? VisitAssignment(BeblangParser.AssignmentContext context)
    {
        var designatorTypeResult = context.designator().Accept(this)!;
        var expressionTypeResult = context.expression().Accept(this)!;
        
        if (designatorTypeResult.IsOk(out var designatorType) &&
            expressionTypeResult.IsOk(out var expressionType) &&
            designatorType != expressionType)
        {
            AddError(context, $"Cannot assign {expressionType} to {designatorType}");
        }
        
        return null;
    }

    public override Result<DataType, SemanticError> VisitDesignator(BeblangParser.DesignatorContext context)
    {
        var name = context.IDENTIFIER().GetText();
        if (!_symbolTable.IsDefined(name, out var symbolInfo))
        {
            return AddError(context, $"Symbol {name} is not defined");
        }

        if (symbolInfo is not VariableInfo variableInfo)
        {
            return AddError(context, $"Symbol {name} is not a variable");
        }
        
        var variableType = variableInfo.DataType;
        foreach (var selector in context.selector())
        {
            if (!variableType.IsArray(out var ofType))
            {
                return AddError(context, $"Symbol {name} is not an array");
            }
            
            var result = selector.expression().Accept(this)!;
            if (result.IsOk(out var selectorType) && selectorType != DataType.Integer)
            {
                AddError(context, $"Selector {selector.GetText()} is not an integer");
            }
            
            variableType = ofType;
        }
            
        return variableType;
    }

    public override Result<DataType, SemanticError>? VisitExpression(BeblangParser.ExpressionContext context)
    {
        if (context.simpleExpression().Length == 1)
        {
            return context.simpleExpression(0).Accept(this);
        }

        var leftTypeResult = context.simpleExpression(0).Accept(this)!;
        var rightTypeResult = context.simpleExpression(1).Accept(this)!;

        if (leftTypeResult.IsOk(out var leftType) &&
            rightTypeResult.IsOk(out var rightType) &&
            leftType != rightType)
        {
            AddError(context, $"Cannot compare {leftType} with {rightType}");
        }

        return DataType.Boolean;
    }

    public override Result<DataType, SemanticError>? VisitSimpleExpression(BeblangParser.SimpleExpressionContext context)
    {
        if (context.term().Length == 1)
        {
            return context.term(0).Accept(this);
        }

        for (var i = 1; i < context.term().Length; i++)
        {
            var leftTypeResult = context.term(i - 1).Accept(this)!;
            var rightTypeResult = context.term(i).Accept(this)!;

            if (leftTypeResult.IsOk(out var leftType))
            {
                if (context.unaryOp() is not null &&
                    leftType != DataType.Integer)
                {
                    AddError(context, $"Cannot perform unary operation ({context.unaryOp().GetText()}) on {leftType}");
                }

                if (rightTypeResult.IsOk(out var rightType) && leftType != rightType)
                {
                    _errors.Add(new SemanticError(context, $"Cannot perform binary operation ({context.binaryOp(i - 1).GetText()}) on {leftType} and {rightType}"));
                }
            }
        }

        return context.term(0).Accept(this)!;
    }

    public override Result<DataType, SemanticError>? VisitTerm(BeblangParser.TermContext context)
    {
        if (context.factor().Length == 1)
        {
            return context.factor(0).Accept(this);
        }

        for (var i = 1; i < context.factor().Length; i++)
        {
            var leftType = context.factor(i - 1).Accept(this)!;
            var rightType = context.factor(i).Accept(this)!;

            if (leftType != rightType)
            {
                AddError(context, $"Cannot perform binary operation ({context.termOp(i - 1).GetText()}) on {leftType} and {rightType}");
            }
        }

        return context.factor(0).Accept(this)!;
    }

    public override Result<DataType, SemanticError>? VisitFactor(BeblangParser.FactorContext context)
    {
        if (context.factor() is not null)
        {
            return context.factor().Accept(this);
        }
        
        if (context.expression() is not null)
        {
            return context.expression().Accept(this);
        }

        if (context.designator() is not null)
        {
            return context.designator().Accept(this);
        }

        if (context.subprogramCall() is not null)
        {
            return context.subprogramCall().Accept(this);
        }

        if (context.literal() is not null)
        {
            return context.literal().Accept(this);
        }

        throw new NotSupportedException($"Factor {context.GetText()} is not supported");
    }

    public override Result<DataType, SemanticError> VisitSubprogramCall(BeblangParser.SubprogramCallContext context)
    {
        var name = context.designator().IDENTIFIER().GetText();
        if (!_symbolTable.IsDefined(name, out var symbolInfo))
        {
            return new SemanticError(context, $"Symbol {name} is not defined");
        }

        if (symbolInfo is not SubprogramInfo subprogramInfo)
        {
            return new SemanticError(context, $"Symbol {name} is not a subprogram");
        }

        var argumentsResults = context.expressionList()?.expression()
            .Select(e => e.Accept(this)!)
            .ToArray() ?? Array.Empty<Result<DataType, SemanticError>>();

        if (argumentsResults.Length != subprogramInfo.Parameters.Count)
        {
            return AddError(context, $"Subprogram {name} expects {subprogramInfo.Parameters.Count} arguments, but {argumentsResults.Length} were provided");
        }

        for (var i = 0; i < argumentsResults.Length; i++)
        {
            if (argumentsResults[i].IsOk(out var argument) && argument != subprogramInfo.Parameters[i].DataType)
            {
                return AddError(context, $"Subprogram {name} expects {subprogramInfo.Parameters[i].DataType} as argument {i + 1}, but {argument} was provided");
            }
        }

        return subprogramInfo.ReturnType;
    }

    public override Result<DataType, SemanticError> VisitLiteral(BeblangParser.LiteralContext context)
    {
        if (context.INTEGER_LITERAL() is not null)
        {
            return DataType.Integer;
        }

        if (context.REAL_LITERAL() is not null)
        {
            return DataType.Real;
        }

        if (context.STRING_LITERAL() is not null)
        {
            return DataType.String;
        }

        if (context.boolean() is not null)
        {
            return DataType.Boolean;
        }

        throw new NotSupportedException($"Literal {context.GetText()} is not supported");
    }
}