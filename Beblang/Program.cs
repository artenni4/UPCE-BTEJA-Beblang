using System.Diagnostics;
using Beblang.IRGeneration;
using Beblang.Semantics;

Trace.Listeners.Add(new ConsoleTraceListener());

var testPrograms = new[]
{
    "Resources/test.beb",
    // "Resources/factorial.beb",
    // "Resources/gcd.beb",
    // "Resources/real_numbers.beb",
    // "Resources/strings.beb"
};

foreach (var testProgram in testPrograms)
{
    Console.WriteLine($"Running compiler for {testProgram}");

    var inputStream = new AntlrFileStream(testProgram);

    var lexer = new BeblangLexer(inputStream);
    var commonTokens = new CommonTokenStream(lexer);
    var parser = new BeblangParser(commonTokens);

    var startContext = parser.start();
    var symbolTable = new SymbolTable().Merge(BuiltInSymbols.Symbols);
    var beblangSemanticVisitor = new BeblangSemanticVisitor(symbolTable);
    beblangSemanticVisitor.Visit(startContext);
    if (beblangSemanticVisitor.Errors.Any())
    {
        Console.WriteLine("\n\nErrors encountered:");
        PrintErrors(beblangSemanticVisitor.Errors);
        Console.WriteLine("\n\n");
        return;
    }
    
    var irGenerationVisitor = new BeblangIRGenerationVisitor();
    irGenerationVisitor.Visit(startContext);
    irGenerationVisitor.Module.Dump();
}

static void PrintErrors(IEnumerable<SemanticError> errors)
{
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
}
