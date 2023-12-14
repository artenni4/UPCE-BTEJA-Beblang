namespace Beblang.Semantics;

public class BeblangSemanticVisitor : BeblangBaseVisitor<object?>
{
    private readonly SymbolTable _symbolTable;
    private SubprogramInfo? _currentSubprogram;
    private bool _isInLoop;

    public BeblangSemanticVisitor(SymbolTable symbolTable)
    {
        _symbolTable = symbolTable;
    }

    public override object? VisitModule(BeblangParser.ModuleContext context)
    {
        var moduleName = context.moduleName.GetFullName();
        _symbolTable.Define(new ModuleInfo(moduleName, context));

        context.moduleStatements().Accept(this);
        
        return null;
    }

    public override object? VisitSubprogram(BeblangParser.SubprogramContext context)
    {
        var subprogramInfo = (SubprogramInfo)context.subprogramDeclaration().Accept(this)!;
        _currentSubprogram = subprogramInfo;
        _symbolTable.EnterScope();
        
        foreach (var parameter in subprogramInfo.Parameters)
        {
            _symbolTable.Define(parameter);
        }
        context.variableDeclarationBlock()?.Accept(this);
        context.subprogramBody().Accept(this);
        
        _symbolTable.ExitScope();
        _currentSubprogram = null;
        
        return null;
    }
    
    public override object VisitSubprogramDeclaration(BeblangParser.SubprogramDeclarationContext context)
    {
        var subprogramName = context.IDENTIFIER().GetText();
        var returnType = context.type()?.GetDataType() ?? DataType.Void;
        var parameters = context.paramList()?.variableDeclaration()
            .SelectMany(vdc => vdc.GetVariableSymbolInfo())
            .ToArray() ?? Array.Empty<VariableInfo>();
        
        var subprogramInfo = new SubprogramInfo(subprogramName, context, parameters, returnType);
        _symbolTable.Define(subprogramInfo);

        return subprogramInfo;
    }

    public override object? VisitVariableDeclarationBlock(BeblangParser.VariableDeclarationBlockContext context)
    {
        foreach (var variableDeclarationContext in context.variableDeclaration())
        {
            foreach (var symbolInfo in variableDeclarationContext.GetVariableSymbolInfo())
            {
                _symbolTable.Define(symbolInfo);
            }
        }

        return null;
    }

    public override object? VisitIfStatement(BeblangParser.IfStatementContext context)
    {
        CheckConditionExpression(context.expression());
        VisitChildren(context);
        return null;
    }

    public override object? VisitElseIfStatement(BeblangParser.ElseIfStatementContext context)
    {
        CheckConditionExpression(context.expression());
        VisitChildren(context);
        return null;
    }

    public override object? VisitWhileStatement(BeblangParser.WhileStatementContext context)
    {
        CheckConditionExpression(context.expression());
        _isInLoop = true;
        VisitChildren(context);
        _isInLoop = false;
        return null;
    }

    private void CheckConditionExpression(BeblangParser.ExpressionContext context)
    {
        var expressionType = (DataType)context.Accept(this)!;
        if (expressionType != DataType.Boolean)
        {
            throw new SemanticException(context, $"Cannot use {expressionType} as condition");
        }
    }

    public override object? VisitReturnStatement(BeblangParser.ReturnStatementContext context)
    {
        var expressionDataType = (DataType)context.expression()?.Accept(this)!;
        if (expressionDataType != _currentSubprogram!.ReturnType)
        {
            throw new SemanticException(context, $"Cannot return {expressionDataType} from {_currentSubprogram.Name}, expected {_currentSubprogram.ReturnType}");
        }

        return null;
    }

    public override object? VisitExitStatement(BeblangParser.ExitStatementContext context)
    {
        if (!_isInLoop)
        {
            throw new SemanticException(context, "Exit statement is not allowed outside of a loop");
        }
        VisitChildren(context);

        return null;
    }

    public override object? VisitAssignment(BeblangParser.AssignmentContext context)
    {
        var designatorType = (DataType)context.designator().Accept(this)!;
        var expressionType = (DataType)context.expression().Accept(this)!;
        
        if (designatorType != expressionType)
        {
            throw new SemanticException(context, $"Cannot assign {expressionType} to {designatorType}");
        }
        
        return null;
    }

    public override object VisitDesignator(BeblangParser.DesignatorContext context)
    {
        var name = context.IDENTIFIER().GetText();
        if (!_symbolTable.IsDefined(name, out var symbolInfo))
        {
            throw new SemanticException(context, $"Symbol {name} is not defined");
        }

        if (symbolInfo is not VariableInfo variableInfo)
        {
            throw new SemanticException(context, $"Symbol {name} is not a variable");
        }
        
        var variableType = variableInfo.DataType;
        foreach (var selector in context.selector())
        {
            if (!variableType.IsArray(out var ofType))
            {
                throw new SemanticException(context, $"Symbol {name} is not an array");
            }
            
            var selectorType = (DataType)selector.expression().Accept(this)!;
            if (selectorType != DataType.Integer)
            {
                throw new SemanticException(context, $"Selector {selector.GetText()} is not an integer");
            }
            
            variableType = ofType;
        }
            
        return variableType;
    }

    public override object? VisitExpression(BeblangParser.ExpressionContext context)
    {
        if (context.simpleExpression().Length == 1)
        {
            return context.simpleExpression(0).Accept(this);
        }

        var leftType = (DataType)context.simpleExpression(0).Accept(this)!;
        var rightType = (DataType)context.simpleExpression(1).Accept(this)!;

        if (leftType != rightType)
        {
            throw new SemanticException(context, $"Cannot compare {leftType} with {rightType}");
        }

        return DataType.Boolean;
    }

    public override object? VisitSimpleExpression(BeblangParser.SimpleExpressionContext context)
    {
        if (context.term().Length == 1)
        {
            return context.term(0).Accept(this);
        }

        for (var i = 1; i < context.term().Length; i++)
        {
            var leftType = (DataType)context.term(i - 1).Accept(this)!;
            var rightType = (DataType)context.term(i).Accept(this)!;

            if (context.unaryOp() is not null && leftType != DataType.Integer)
            {
                throw new SemanticException(context, $"Cannot perform unary operation ({context.unaryOp().GetText()}) on {leftType}");
            }

            if (leftType != rightType)
            {
                throw new SemanticException(context, $"Cannot perform binary operation ({context.binaryOp(i - 1).GetText()}) on {leftType} and {rightType}");
            }
        }

        return (DataType)context.term(0).Accept(this)!;
    }

    public override object? VisitTerm(BeblangParser.TermContext context)
    {
        if (context.factor().Length == 1)
        {
            return context.factor(0).Accept(this);
        }

        for (var i = 1; i < context.factor().Length; i++)
        {
            var leftType = (DataType)context.factor(i - 1).Accept(this)!;
            var rightType = (DataType)context.factor(i).Accept(this)!;

            if (leftType != rightType)
            {
                throw new SemanticException(context, $"Cannot perform binary operation ({context.termOp(i - 1).GetText()}) on {leftType} and {rightType}");
            }
        }

        return (DataType)context.factor(0).Accept(this)!;
    }

    public override object? VisitFactor(BeblangParser.FactorContext context)
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

    public override object VisitSubprogramCall(BeblangParser.SubprogramCallContext context)
    {
        var name = context.designator().IDENTIFIER().GetText();
        if (!_symbolTable.IsDefined(name, out var symbolInfo))
        {
            throw new SemanticException(context, $"Symbol {name} is not defined");
        }

        if (symbolInfo is not SubprogramInfo subprogramInfo)
        {
            throw new SemanticException(context, $"Symbol {name} is not a subprogram");
        }

        var arguments = context.expressionList()?.expression()
            .Select(e => (DataType)e.Accept(this)!)
            .ToArray() ?? Array.Empty<DataType>();

        if (arguments.Length != subprogramInfo.Parameters.Count)
        {
            throw new SemanticException(context, $"Subprogram {name} expects {subprogramInfo.Parameters.Count} arguments, but {arguments.Length} were provided");
        }

        for (var i = 0; i < arguments.Length; i++)
        {
            if (arguments[i] != subprogramInfo.Parameters[i].DataType)
            {
                throw new SemanticException(context, $"Subprogram {name} expects {subprogramInfo.Parameters[i].DataType} as argument {i + 1}, but {arguments[i]} was provided");
            }
        }

        return subprogramInfo.ReturnType;
    }

    public override object VisitLiteral(BeblangParser.LiteralContext context)
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