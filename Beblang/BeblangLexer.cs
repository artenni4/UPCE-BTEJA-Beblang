//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.13.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from Beblang.g4 by ANTLR 4.13.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.13.1")]
[System.CLSCompliant(false)]
public partial class BeblangLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		T__0=1, T__1=2, T__2=3, T__3=4, T__4=5, T__5=6, T__6=7, T__7=8, T__8=9, 
		T__9=10, T__10=11, T__11=12, T__12=13, T__13=14, T__14=15, T__15=16, T__16=17, 
		T__17=18, T__18=19, T__19=20, T__20=21, MODULE=22, IMPORT=23, VAR=24, 
		BEGIN=25, END=26, EXIT=27, IF=28, THEN=29, ELSIF=30, ELSE=31, WHILE=32, 
		DO=33, MOD=34, TRUE=35, FALSE=36, OR=37, RETURN=38, ARRAY=39, OF=40, INTEGER=41, 
		REAL=42, STRING=43, BOOLEAN=44, PROCEDURE=45, INTEGER_LITERAL=46, IDENTIFIER=47, 
		REAL_LITERAL=48, STRING_LITERAL=49, WS=50, COMMENT=51;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"T__0", "T__1", "T__2", "T__3", "T__4", "T__5", "T__6", "T__7", "T__8", 
		"T__9", "T__10", "T__11", "T__12", "T__13", "T__14", "T__15", "T__16", 
		"T__17", "T__18", "T__19", "T__20", "MODULE", "IMPORT", "VAR", "BEGIN", 
		"END", "EXIT", "IF", "THEN", "ELSIF", "ELSE", "WHILE", "DO", "MOD", "TRUE", 
		"FALSE", "OR", "RETURN", "ARRAY", "OF", "INTEGER", "REAL", "STRING", "BOOLEAN", 
		"PROCEDURE", "INTEGER_LITERAL", "IDENTIFIER", "REAL_LITERAL", "STRING_LITERAL", 
		"WS", "COMMENT"
	};


	public BeblangLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public BeblangLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, "';'", "'.'", "'('", "')'", "':'", "','", "':='", "'='", "'#'", 
		"'<'", "'<='", "'>'", "'>='", "'+'", "'-'", "'/'", "'*'", "'&'", "'~'", 
		"'['", "']'", "'MODULE'", "'IMPORT'", "'VAR'", "'BEGIN'", "'END'", "'EXIT'", 
		"'IF'", "'THEN'", "'ELSIF'", "'ELSE'", "'WHILE'", "'DO'", "'MOD'", "'TRUE'", 
		"'FALSE'", "'OR'", "'RETURN'", "'ARRAY'", "'OF'", "'INTEGER'", "'REAL'", 
		"'STRING'", "'BOOLEAN'", "'PROCEDURE'"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, null, null, null, null, null, null, null, null, null, 
		null, null, null, null, null, null, null, null, null, null, "MODULE", 
		"IMPORT", "VAR", "BEGIN", "END", "EXIT", "IF", "THEN", "ELSIF", "ELSE", 
		"WHILE", "DO", "MOD", "TRUE", "FALSE", "OR", "RETURN", "ARRAY", "OF", 
		"INTEGER", "REAL", "STRING", "BOOLEAN", "PROCEDURE", "INTEGER_LITERAL", 
		"IDENTIFIER", "REAL_LITERAL", "STRING_LITERAL", "WS", "COMMENT"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "Beblang.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static BeblangLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static int[] _serializedATN = {
		4,0,51,334,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,7,
		6,2,7,7,7,2,8,7,8,2,9,7,9,2,10,7,10,2,11,7,11,2,12,7,12,2,13,7,13,2,14,
		7,14,2,15,7,15,2,16,7,16,2,17,7,17,2,18,7,18,2,19,7,19,2,20,7,20,2,21,
		7,21,2,22,7,22,2,23,7,23,2,24,7,24,2,25,7,25,2,26,7,26,2,27,7,27,2,28,
		7,28,2,29,7,29,2,30,7,30,2,31,7,31,2,32,7,32,2,33,7,33,2,34,7,34,2,35,
		7,35,2,36,7,36,2,37,7,37,2,38,7,38,2,39,7,39,2,40,7,40,2,41,7,41,2,42,
		7,42,2,43,7,43,2,44,7,44,2,45,7,45,2,46,7,46,2,47,7,47,2,48,7,48,2,49,
		7,49,2,50,7,50,1,0,1,0,1,1,1,1,1,2,1,2,1,3,1,3,1,4,1,4,1,5,1,5,1,6,1,6,
		1,6,1,7,1,7,1,8,1,8,1,9,1,9,1,10,1,10,1,10,1,11,1,11,1,12,1,12,1,12,1,
		13,1,13,1,14,1,14,1,15,1,15,1,16,1,16,1,17,1,17,1,18,1,18,1,19,1,19,1,
		20,1,20,1,21,1,21,1,21,1,21,1,21,1,21,1,21,1,22,1,22,1,22,1,22,1,22,1,
		22,1,22,1,23,1,23,1,23,1,23,1,24,1,24,1,24,1,24,1,24,1,24,1,25,1,25,1,
		25,1,25,1,26,1,26,1,26,1,26,1,26,1,27,1,27,1,27,1,28,1,28,1,28,1,28,1,
		28,1,29,1,29,1,29,1,29,1,29,1,29,1,30,1,30,1,30,1,30,1,30,1,31,1,31,1,
		31,1,31,1,31,1,31,1,32,1,32,1,32,1,33,1,33,1,33,1,33,1,34,1,34,1,34,1,
		34,1,34,1,35,1,35,1,35,1,35,1,35,1,35,1,36,1,36,1,36,1,37,1,37,1,37,1,
		37,1,37,1,37,1,37,1,38,1,38,1,38,1,38,1,38,1,38,1,39,1,39,1,39,1,40,1,
		40,1,40,1,40,1,40,1,40,1,40,1,40,1,41,1,41,1,41,1,41,1,41,1,42,1,42,1,
		42,1,42,1,42,1,42,1,42,1,43,1,43,1,43,1,43,1,43,1,43,1,43,1,43,1,44,1,
		44,1,44,1,44,1,44,1,44,1,44,1,44,1,44,1,44,1,45,4,45,283,8,45,11,45,12,
		45,284,1,46,1,46,5,46,289,8,46,10,46,12,46,292,9,46,1,47,4,47,295,8,47,
		11,47,12,47,296,1,47,1,47,4,47,301,8,47,11,47,12,47,302,1,48,1,48,5,48,
		307,8,48,10,48,12,48,310,9,48,1,48,1,48,1,49,4,49,315,8,49,11,49,12,49,
		316,1,49,1,49,1,50,1,50,1,50,1,50,5,50,325,8,50,10,50,12,50,328,9,50,1,
		50,1,50,1,50,1,50,1,50,2,308,326,0,51,1,1,3,2,5,3,7,4,9,5,11,6,13,7,15,
		8,17,9,19,10,21,11,23,12,25,13,27,14,29,15,31,16,33,17,35,18,37,19,39,
		20,41,21,43,22,45,23,47,24,49,25,51,26,53,27,55,28,57,29,59,30,61,31,63,
		32,65,33,67,34,69,35,71,36,73,37,75,38,77,39,79,40,81,41,83,42,85,43,87,
		44,89,45,91,46,93,47,95,48,97,49,99,50,101,51,1,0,4,1,0,48,57,3,0,65,90,
		95,95,97,122,4,0,48,57,65,90,95,95,97,122,3,0,9,10,13,13,32,32,340,0,1,
		1,0,0,0,0,3,1,0,0,0,0,5,1,0,0,0,0,7,1,0,0,0,0,9,1,0,0,0,0,11,1,0,0,0,0,
		13,1,0,0,0,0,15,1,0,0,0,0,17,1,0,0,0,0,19,1,0,0,0,0,21,1,0,0,0,0,23,1,
		0,0,0,0,25,1,0,0,0,0,27,1,0,0,0,0,29,1,0,0,0,0,31,1,0,0,0,0,33,1,0,0,0,
		0,35,1,0,0,0,0,37,1,0,0,0,0,39,1,0,0,0,0,41,1,0,0,0,0,43,1,0,0,0,0,45,
		1,0,0,0,0,47,1,0,0,0,0,49,1,0,0,0,0,51,1,0,0,0,0,53,1,0,0,0,0,55,1,0,0,
		0,0,57,1,0,0,0,0,59,1,0,0,0,0,61,1,0,0,0,0,63,1,0,0,0,0,65,1,0,0,0,0,67,
		1,0,0,0,0,69,1,0,0,0,0,71,1,0,0,0,0,73,1,0,0,0,0,75,1,0,0,0,0,77,1,0,0,
		0,0,79,1,0,0,0,0,81,1,0,0,0,0,83,1,0,0,0,0,85,1,0,0,0,0,87,1,0,0,0,0,89,
		1,0,0,0,0,91,1,0,0,0,0,93,1,0,0,0,0,95,1,0,0,0,0,97,1,0,0,0,0,99,1,0,0,
		0,0,101,1,0,0,0,1,103,1,0,0,0,3,105,1,0,0,0,5,107,1,0,0,0,7,109,1,0,0,
		0,9,111,1,0,0,0,11,113,1,0,0,0,13,115,1,0,0,0,15,118,1,0,0,0,17,120,1,
		0,0,0,19,122,1,0,0,0,21,124,1,0,0,0,23,127,1,0,0,0,25,129,1,0,0,0,27,132,
		1,0,0,0,29,134,1,0,0,0,31,136,1,0,0,0,33,138,1,0,0,0,35,140,1,0,0,0,37,
		142,1,0,0,0,39,144,1,0,0,0,41,146,1,0,0,0,43,148,1,0,0,0,45,155,1,0,0,
		0,47,162,1,0,0,0,49,166,1,0,0,0,51,172,1,0,0,0,53,176,1,0,0,0,55,181,1,
		0,0,0,57,184,1,0,0,0,59,189,1,0,0,0,61,195,1,0,0,0,63,200,1,0,0,0,65,206,
		1,0,0,0,67,209,1,0,0,0,69,213,1,0,0,0,71,218,1,0,0,0,73,224,1,0,0,0,75,
		227,1,0,0,0,77,234,1,0,0,0,79,240,1,0,0,0,81,243,1,0,0,0,83,251,1,0,0,
		0,85,256,1,0,0,0,87,263,1,0,0,0,89,271,1,0,0,0,91,282,1,0,0,0,93,286,1,
		0,0,0,95,294,1,0,0,0,97,304,1,0,0,0,99,314,1,0,0,0,101,320,1,0,0,0,103,
		104,5,59,0,0,104,2,1,0,0,0,105,106,5,46,0,0,106,4,1,0,0,0,107,108,5,40,
		0,0,108,6,1,0,0,0,109,110,5,41,0,0,110,8,1,0,0,0,111,112,5,58,0,0,112,
		10,1,0,0,0,113,114,5,44,0,0,114,12,1,0,0,0,115,116,5,58,0,0,116,117,5,
		61,0,0,117,14,1,0,0,0,118,119,5,61,0,0,119,16,1,0,0,0,120,121,5,35,0,0,
		121,18,1,0,0,0,122,123,5,60,0,0,123,20,1,0,0,0,124,125,5,60,0,0,125,126,
		5,61,0,0,126,22,1,0,0,0,127,128,5,62,0,0,128,24,1,0,0,0,129,130,5,62,0,
		0,130,131,5,61,0,0,131,26,1,0,0,0,132,133,5,43,0,0,133,28,1,0,0,0,134,
		135,5,45,0,0,135,30,1,0,0,0,136,137,5,47,0,0,137,32,1,0,0,0,138,139,5,
		42,0,0,139,34,1,0,0,0,140,141,5,38,0,0,141,36,1,0,0,0,142,143,5,126,0,
		0,143,38,1,0,0,0,144,145,5,91,0,0,145,40,1,0,0,0,146,147,5,93,0,0,147,
		42,1,0,0,0,148,149,5,77,0,0,149,150,5,79,0,0,150,151,5,68,0,0,151,152,
		5,85,0,0,152,153,5,76,0,0,153,154,5,69,0,0,154,44,1,0,0,0,155,156,5,73,
		0,0,156,157,5,77,0,0,157,158,5,80,0,0,158,159,5,79,0,0,159,160,5,82,0,
		0,160,161,5,84,0,0,161,46,1,0,0,0,162,163,5,86,0,0,163,164,5,65,0,0,164,
		165,5,82,0,0,165,48,1,0,0,0,166,167,5,66,0,0,167,168,5,69,0,0,168,169,
		5,71,0,0,169,170,5,73,0,0,170,171,5,78,0,0,171,50,1,0,0,0,172,173,5,69,
		0,0,173,174,5,78,0,0,174,175,5,68,0,0,175,52,1,0,0,0,176,177,5,69,0,0,
		177,178,5,88,0,0,178,179,5,73,0,0,179,180,5,84,0,0,180,54,1,0,0,0,181,
		182,5,73,0,0,182,183,5,70,0,0,183,56,1,0,0,0,184,185,5,84,0,0,185,186,
		5,72,0,0,186,187,5,69,0,0,187,188,5,78,0,0,188,58,1,0,0,0,189,190,5,69,
		0,0,190,191,5,76,0,0,191,192,5,83,0,0,192,193,5,73,0,0,193,194,5,70,0,
		0,194,60,1,0,0,0,195,196,5,69,0,0,196,197,5,76,0,0,197,198,5,83,0,0,198,
		199,5,69,0,0,199,62,1,0,0,0,200,201,5,87,0,0,201,202,5,72,0,0,202,203,
		5,73,0,0,203,204,5,76,0,0,204,205,5,69,0,0,205,64,1,0,0,0,206,207,5,68,
		0,0,207,208,5,79,0,0,208,66,1,0,0,0,209,210,5,77,0,0,210,211,5,79,0,0,
		211,212,5,68,0,0,212,68,1,0,0,0,213,214,5,84,0,0,214,215,5,82,0,0,215,
		216,5,85,0,0,216,217,5,69,0,0,217,70,1,0,0,0,218,219,5,70,0,0,219,220,
		5,65,0,0,220,221,5,76,0,0,221,222,5,83,0,0,222,223,5,69,0,0,223,72,1,0,
		0,0,224,225,5,79,0,0,225,226,5,82,0,0,226,74,1,0,0,0,227,228,5,82,0,0,
		228,229,5,69,0,0,229,230,5,84,0,0,230,231,5,85,0,0,231,232,5,82,0,0,232,
		233,5,78,0,0,233,76,1,0,0,0,234,235,5,65,0,0,235,236,5,82,0,0,236,237,
		5,82,0,0,237,238,5,65,0,0,238,239,5,89,0,0,239,78,1,0,0,0,240,241,5,79,
		0,0,241,242,5,70,0,0,242,80,1,0,0,0,243,244,5,73,0,0,244,245,5,78,0,0,
		245,246,5,84,0,0,246,247,5,69,0,0,247,248,5,71,0,0,248,249,5,69,0,0,249,
		250,5,82,0,0,250,82,1,0,0,0,251,252,5,82,0,0,252,253,5,69,0,0,253,254,
		5,65,0,0,254,255,5,76,0,0,255,84,1,0,0,0,256,257,5,83,0,0,257,258,5,84,
		0,0,258,259,5,82,0,0,259,260,5,73,0,0,260,261,5,78,0,0,261,262,5,71,0,
		0,262,86,1,0,0,0,263,264,5,66,0,0,264,265,5,79,0,0,265,266,5,79,0,0,266,
		267,5,76,0,0,267,268,5,69,0,0,268,269,5,65,0,0,269,270,5,78,0,0,270,88,
		1,0,0,0,271,272,5,80,0,0,272,273,5,82,0,0,273,274,5,79,0,0,274,275,5,67,
		0,0,275,276,5,69,0,0,276,277,5,68,0,0,277,278,5,85,0,0,278,279,5,82,0,
		0,279,280,5,69,0,0,280,90,1,0,0,0,281,283,7,0,0,0,282,281,1,0,0,0,283,
		284,1,0,0,0,284,282,1,0,0,0,284,285,1,0,0,0,285,92,1,0,0,0,286,290,7,1,
		0,0,287,289,7,2,0,0,288,287,1,0,0,0,289,292,1,0,0,0,290,288,1,0,0,0,290,
		291,1,0,0,0,291,94,1,0,0,0,292,290,1,0,0,0,293,295,7,0,0,0,294,293,1,0,
		0,0,295,296,1,0,0,0,296,294,1,0,0,0,296,297,1,0,0,0,297,298,1,0,0,0,298,
		300,5,46,0,0,299,301,7,0,0,0,300,299,1,0,0,0,301,302,1,0,0,0,302,300,1,
		0,0,0,302,303,1,0,0,0,303,96,1,0,0,0,304,308,5,34,0,0,305,307,9,0,0,0,
		306,305,1,0,0,0,307,310,1,0,0,0,308,309,1,0,0,0,308,306,1,0,0,0,309,311,
		1,0,0,0,310,308,1,0,0,0,311,312,5,34,0,0,312,98,1,0,0,0,313,315,7,3,0,
		0,314,313,1,0,0,0,315,316,1,0,0,0,316,314,1,0,0,0,316,317,1,0,0,0,317,
		318,1,0,0,0,318,319,6,49,0,0,319,100,1,0,0,0,320,321,5,40,0,0,321,322,
		5,42,0,0,322,326,1,0,0,0,323,325,9,0,0,0,324,323,1,0,0,0,325,328,1,0,0,
		0,326,327,1,0,0,0,326,324,1,0,0,0,327,329,1,0,0,0,328,326,1,0,0,0,329,
		330,5,42,0,0,330,331,5,41,0,0,331,332,1,0,0,0,332,333,6,50,0,0,333,102,
		1,0,0,0,8,0,284,290,296,302,308,316,326,1,6,0,0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
