using Antlr4.Runtime;
using Beblang;

Console.WriteLine("Running compiler");

var inputStream = new AntlrFileStream("Resources/test.beb");

var lexer = new BeblangLexer(inputStream);
var commonTokens = new CommonTokenStream(lexer);
var parser = new BeblangParser(commonTokens);

var startContext = parser.start();
var visitor = new BasicBeblangVisitor();
visitor.Visit(startContext);