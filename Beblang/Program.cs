using System.Diagnostics;
using Beblang.Semantics;

Trace.Listeners.Add(new ConsoleTraceListener());

var testPrograms = new[]
{
    "Resources/test.beb",
    "Resources/factorial.beb",
    "Resources/gcd.beb",
    "Resources/real_numbers.beb",
    "Resources/strings.beb"
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
    var visitor = new BeblangSemanticVisitor(symbolTable);
    visitor.Visit(startContext);
}
