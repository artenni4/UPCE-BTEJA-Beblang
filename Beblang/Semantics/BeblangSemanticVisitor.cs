namespace Beblang.Semantics;

public class BeblangSemanticVisitor : BeblangBaseVisitor<object?>
{
    private readonly SymbolTable _symbolTable;

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
        _symbolTable.EnterScope();
        
        foreach (var parameter in subprogramInfo.Parameters)
        {
            _symbolTable.Define(parameter);
        }
        context.variableDeclarationBlock()?.Accept(this);
        context.subprogramBody().Accept(this);
        
        _symbolTable.ExitScope();

        return null;
    }
    
    public override object VisitSubprogramDeclaration(BeblangParser.SubprogramDeclarationContext context)
    {
        var subprogramName = context.IDENTIFIER().GetText();
        var returnType = context.type()?.GetDataType();
        var parameters = context.paramList().variableDeclaration()
            .SelectMany(vdc => vdc.GetVariableSymbolInfo())
            .ToArray();
        
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
        if (context.selector().Any())
        {
            if (variableType.IsArray(out var ofType))
            {
                throw new SemanticException(context, $"Symbol {name} is not an array");
            }

            foreach (var selector in context.selector())
            {
                var selectorType = (DataType)selector.Accept(this)!;
                if (selectorType != DataType.Integer)
                {
                    throw new SemanticException(context, $"Selector {selector.GetText()} is not an integer");
                }
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
}