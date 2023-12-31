﻿namespace Beblang.Semantics;

public class BeblangSemanticVisitor : BeblangBaseVisitor<Result<DataType, SemanticError>?>
{
    public AnnotationTable AnnotationTable { get; } = new();
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
        var moduleInfo = new ModuleInfo(moduleName, context);
        if (!_symbolTable.TryDefine(moduleInfo, out var error))
        {
            return AddError(error);
        }
        AnnotationTable.AnnotateSymbols(context, moduleInfo);
        
        context.moduleStatements().Accept(this);
        
        return null;
    }

    public override Result<DataType, SemanticError>? VisitSubprogram(BeblangParser.SubprogramContext context)
    {
        var subprogramInfoResult = GetSubprogramInfo(context.subprogramHeading());
        if (subprogramInfoResult.IsError(out var subprogramInfoError, out var subprogramInfo))
        {
            return subprogramInfoError;
        }
        
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
        
        AnnotationTable.AnnotateSymbols(context, subprogramInfo);
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
        var subprogramInfoResult = GetSubprogramInfo(context.subprogramHeading());
        if (subprogramInfoResult.IsError(out var subprogramInfoError, out var subprogramInfo))
        {
            return subprogramInfoError;
        }
        
        if (!_symbolTable.TryDefine(subprogramInfo, out var error))
        {
            return AddError(error);
        }
        AnnotationTable.AnnotateSymbols(context, subprogramInfo);

        return null;
    }

    private Result<SubprogramInfo, SemanticError> GetSubprogramInfo(BeblangParser.SubprogramHeadingContext context)
    {
        var subprogramName = context.IDENTIFIER().GetText();
        var returnType = context.type() is null ? DataType.Void : GetDataType(context.type());
        var parameters = context.paramList()?.variableDeclaration()
            .SelectMany(GetVariableSymbolInfo)
            .ToArray() ?? Array.Empty<VariableInfo>();

        if (returnType.IsArray(out _))
        {
            return AddError(context, $"Cannot return {returnType} from {subprogramName}");
        }
        
        return new SubprogramInfo(subprogramName, context, parameters, returnType);
    }

    public override Result<DataType, SemanticError>? VisitVariableDeclarationBlock(BeblangParser.VariableDeclarationBlockContext context)
    {
        foreach (var variableDeclarationContext in context.variableDeclaration())
        {
            var variableInfos = GetVariableSymbolInfo(variableDeclarationContext);
            foreach (var symbolInfo in variableInfos)
            {
                if (!_symbolTable.TryDefine(symbolInfo, out var error))
                {
                    return AddError(error);
                }
            }
            AnnotationTable.AnnotateSymbols(variableDeclarationContext, variableInfos);
        }

        return null;
    }

    private static VariableInfo[] GetVariableSymbolInfo(BeblangParser.VariableDeclarationContext context)
    {
        var dataType = GetDataType(context.type());
        return context.IDENTIFIER()
            .Select(node => node.GetText())
            .Select(identifier => new VariableInfo(identifier, context, dataType))
            .ToArray();
    }

    private static DataType GetDataType(BeblangParser.TypeContext context)
    {
        if (context.INTEGER() is not null)
        {
            return DataType.Integer;
        }

        if (context.REAL() is not null)
        {
            return DataType.Real;
        }

        if (context.STRING() is not null)
        {
            return DataType.String;
        }

        if (context.ARRAY() is not null)
        {
            var ofType = GetDataType(context.type());
            var size = int.Parse(context.INTEGER_LITERAL().GetText());
            return DataType.Array(ofType, size);
        }

        throw new NotSupportedException($"Type {context.GetText()} is not supported");
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
        if (result is null && _currentSubprogram!.ReturnType != DataType.Void)
        {
            AddError(context, $"Cannot return {DataType.Void} from {_currentSubprogram.Name}, expected {_currentSubprogram.ReturnType}");
        }

        if (result is not null && result.IsOk(out var expressionDataType) &&
            expressionDataType != _currentSubprogram!.ReturnType)
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

    public override Result<DataType, SemanticError> VisitExpression(BeblangParser.ExpressionContext context)
    {
        if (context.simpleExpression().Length == 1)
        {
            return context.simpleExpression(0).Accept(this)!;
        }
        
        var comparisonOp = context.comparisonOp().GetText();
        var leftTypeResult = context.simpleExpression(0).Accept(this)!;
        if (!leftTypeResult.IsOk(out var leftType))
        {
            return leftTypeResult;
        }
        var rightTypeResult = context.simpleExpression(1).Accept(this)!;
        if (!rightTypeResult.IsOk(out var rightType))
        {
            return rightTypeResult;
        }

        if (leftType != rightType)
        {
            return AddError(context, $"Cannot compare {leftType} with {rightType} using {comparisonOp}");
        }
        
        if (comparisonOp != "=" && comparisonOp != "#" && !leftType.IsNumeric())
        {
            return AddError(context, $"Cannot compare {leftType} with {rightType} using {comparisonOp}");
        }

        return DataType.Boolean;
    }

    public override Result<DataType, SemanticError> VisitSimpleExpression(BeblangParser.SimpleExpressionContext context)
    {
        var result = context.term(0).Accept(this)!;
        if (!result.IsOk(out var resultType))
        {
            return result;
        }
        AnnotationTable.AnnotateType(context, resultType);
        var unaryOp = context.unaryOp()?.GetText();
        
        if (context.term().Length == 1)
        {
            if (unaryOp is not null && !resultType.IsNumeric())
            {
                return AddError(context, $"Cannot perform unary operation ({unaryOp}) on {resultType}");
            }
            return result;
        }
        
        // operations with more than one term are always numeric
        var binaryOp = context.binaryOp(0).GetText();
        if (binaryOp != "OR" && !resultType.IsNumeric() ||
            binaryOp == "OR" && resultType != DataType.Boolean)
        {
            return AddError(context, $"Cannot perform binary operation ({binaryOp}) on {resultType}");
        }

        for (var i = 1; i < context.term().Length; i++)
        {
            var rightTypeResult = context.term(i).Accept(this)!;
            if (!rightTypeResult.IsOk(out var rightType))
            {
                return rightTypeResult;
            }
            
            if (resultType != rightType)
            {
                return AddError(context, $"Cannot perform binary operation ({context.binaryOp(i - 1).GetText()}) on {resultType} and {rightType}");
            }
        }
        
        return result;
    }

    public override Result<DataType, SemanticError> VisitTerm(BeblangParser.TermContext context)
    {
        var result = context.factor(0).Accept(this)!;
        if (!result.IsOk(out var resultType))
        {
            return result;
        }
        
        AnnotationTable.AnnotateType(context, resultType);
        if (context.factor().Length == 1)
        {
            return result;
        }

        var termOp = context.termOp(0).GetText();
        if (!resultType.IsNumeric())
        {
            return AddError(context, $"Cannot perform binary operation ({termOp}) on {resultType}");
        }

        for (var i = 1; i < context.factor().Length; i++)
        {
            var rightTypeResult = context.factor(i).Accept(this)!;
            if (!rightTypeResult.IsOk(out var rightType))
            {
                return rightTypeResult;
            }

            if (resultType != rightType)
            {
                return AddError(context, $"Cannot perform binary operation ({context.termOp(i - 1).GetText()}) on {resultType} and {rightType}");
            }
        }
        
        return context.factor(0).Accept(this)!;
    }

    public override Result<DataType, SemanticError> VisitFactor(BeblangParser.FactorContext context)
    {
        if (context.factor() is not null)
        {
            return context.factor().Accept(this)!;
        }
        
        if (context.expression() is not null)
        {
            return context.expression().Accept(this)!;
        }

        if (context.designator() is not null)
        {
            return context.designator().Accept(this)!;
        }

        if (context.subprogramCall() is not null)
        {
            return context.subprogramCall().Accept(this)!;
        }

        if (context.literal() is not null)
        {
            return context.literal().Accept(this)!;
        }

        throw new NotSupportedException($"Factor {context.GetText()} is not supported");
    }

    public override Result<DataType, SemanticError> VisitSubprogramCall(BeblangParser.SubprogramCallContext context)
    {
        var name = context.designator().IDENTIFIER().GetText();
        if (!_symbolTable.IsDefined(name, out var symbolInfo))
        {
            return AddError(context, $"Symbol {name} is not defined");
        }

        if (symbolInfo is not SubprogramInfo subprogramInfo)
        {
            return AddError(context, $"Symbol {name} is not a subprogram");
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
        
        AnnotationTable.AnnotateSymbols(context, subprogramInfo);
        return subprogramInfo.ReturnType;
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
            if (!variableType.IsArray(out var arrayInfo))
            {
                return AddError(context, $"Symbol {name} is not an array");
            }
            
            var result = selector.expression().Accept(this)!;
            if (result.IsOk(out var selectorType) && selectorType != DataType.Integer)
            {
                AddError(context, $"Selector {selector.GetText()} is not an integer");
            }
            
            variableType = arrayInfo.OfType;
        }
            
        return variableType;
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