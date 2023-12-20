using System.Diagnostics;
using Beblang.IRGeneration;
using Beblang.Semantics;

Trace.Listeners.Add(new ConsoleTraceListener());

if (args.Length > 0)
{
    foreach (var arg in args)
    {
        if (!RunCompiler(arg))
        {
            return;
        }
    }
    return;
}

var testPrograms = new[]
{
    "Resources/test.beb",
    //"Resources/simple.beb",
    //"Resources/arrays.beb",
    //"Resources/factorial.beb",
    //"Resources/gcd.beb",
    //"Resources/real_numbers.beb",
};

foreach (var testProgram in testPrograms)
{
    RunCompiler(testProgram);
}

static bool RunCompiler(string sourcePath)
{
    Console.WriteLine($"Running compiler for {sourcePath}");

    var inputStream = new AntlrFileStream(sourcePath);

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
        return false;
    }
    
    var irGenerationVisitor = new BeblangIrGenerationVisitor(beblangSemanticVisitor.AnnotationTable);
    
#if DEBUG
    irGenerationVisitor.Visit(startContext);
    irGenerationVisitor.Module.Dump();
#else
    try
    {
        irGenerationVisitor.Visit(startContext);
    }
    catch (Exception e)
    {
        Console.WriteLine("\n\nException encountered:\n" + e.StackTrace);
        return false;
    }
#endif

    var llFile = sourcePath + ".ll";
    irGenerationVisitor.Module.PrintToFile(llFile);
    
    var llcStartInfo = new ProcessStartInfo
    {
        FileName = "clang",
        Arguments = $"{llFile} -llegacy_stdio_definitions -o {sourcePath}.exe",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };

    using var llcProcess = Process.Start(llcStartInfo);
    if (llcProcess == null)
    {
        Console.WriteLine("Failed to start clang");
        return false;
    }
    llcProcess.WaitForExit();

    // Handle output and errors
    var llcOutput = llcProcess.StandardOutput.ReadToEnd();
    var llcError = llcProcess.StandardError.ReadToEnd();

    Console.WriteLine(llcOutput);
    Console.WriteLine(llcError);

    return true;
}

static void PrintErrors(IEnumerable<SemanticError> errors)
{
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
}
