namespace Beblang;

public static class ContextExtensions
{
    public static string GetFullName(this BeblangParser.QualifiedIdentifierContext context)
    {
        return string.Join('.', context.IDENTIFIER().Select(id => id.GetText()));
    }
}