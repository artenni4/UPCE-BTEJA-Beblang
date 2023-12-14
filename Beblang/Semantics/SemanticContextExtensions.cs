namespace Beblang.Semantics;

public static class SemanticContextExtensions
{
    public static SubprogramInfo GetSubprogramInfo(this BeblangParser.SubprogramHeadingContext context)
    {
        var subprogramName = context.IDENTIFIER().GetText();
        var returnType = context.type()?.GetDataType() ?? DataType.Void;
        var parameters = context.paramList()?.variableDeclaration()
            .SelectMany(vdc => vdc.GetVariableSymbolInfo())
            .ToArray() ?? Array.Empty<VariableInfo>();
        
        var subprogramInfo = new SubprogramInfo(subprogramName, context, parameters, returnType);

        return subprogramInfo;
    }
    
    public static VariableInfo[] GetVariableSymbolInfo(this BeblangParser.VariableDeclarationContext context)
    {
        var dataType = context.type().GetDataType();
        return context.IDENTIFIER()
            .Select(node => node.GetText())
            .Select(identifier => new VariableInfo(identifier, context, dataType))
            .ToArray();
    }
    
    public static DataType GetDataType(this BeblangParser.TypeContext context)
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
            var ofType = context.type().GetDataType();
            return DataType.Array(ofType);
        }

        throw new NotSupportedException($"Type {context.GetText()} is not supported");
    }
}