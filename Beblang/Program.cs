using System.Diagnostics;
using Beblang.IRGeneration;
using Beblang.Semantics;

Trace.Listeners.Add(new ConsoleTraceListener());

var testPrograms = new[]
{
    "Resources/simple.beb",
    // "Resources/test.beb",
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
        return;
    }
    
    var irGenerationVisitor = new BeblangIrGenerationVisitor(beblangSemanticVisitor.AnnotationTable);
    irGenerationVisitor.Visit(startContext);
    var llFile = testProgram + ".ll";
    irGenerationVisitor.Module.PrintToFile(llFile);
    irGenerationVisitor.Module.Dump();
    
    var llcStartInfo = new ProcessStartInfo
    {
        FileName = "clang",
        Arguments = $"{llFile} -o {testProgram}.exe",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };

    using var llcProcess = Process.Start(llcStartInfo);
    if (llcProcess != null)
    {
        llcProcess.WaitForExit();

        // Handle output and errors
        var llcOutput = llcProcess.StandardOutput.ReadToEnd();
        var llcError = llcProcess.StandardError.ReadToEnd();

        Console.WriteLine(llcOutput);
        Console.WriteLine(llcError);
    }
}

static void PrintErrors(IEnumerable<SemanticError> errors)
{
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
}
