namespace Beblang.Semantics;

public static class SemanticContextExtensions
{
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