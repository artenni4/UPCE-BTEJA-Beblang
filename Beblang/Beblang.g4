grammar Beblang;

// Lexer rules
MODULE: 'MODULE';
IMPORT: 'IMPORT';
VAR: 'VAR';
BEGIN: 'BEGIN';
END: 'END';
EXIT: 'EXIT';
IF: 'IF';
THEN: 'THEN';
ELSIF: 'ELSIF';
ELSE: 'ELSE';
WHILE: 'WHILE';
DO: 'DO';
MOD: 'MOD';
TRUE: 'TRUE';
FALSE: 'FALSE';
OR: 'OR';
RETURN: 'RETURN';
ARRAY: 'ARRAY';
OF: 'OF';
INTEGER: 'INTEGER';
REAL: 'REAL';
STRING: 'STRING';
BOOLEAN: 'BOOLEAN';
PROCEDURE: 'PROCEDURE';
INTEGER_LITERAL: [0-9]+;
IDENTIFIER: [a-zA-Z_] [a-zA-Z_0-9]*;
REAL_LITERAL: [0-9]+ '.' [0-9]+;
STRING_LITERAL: '"' ( ~["\\] | '\\' . )* '"';
WS: [ \t\r\n]+ -> skip;
COMMENT: '(*' .*? '*)' -> skip;

// Parser rules
start: module EOF;

module: MODULE moduleName=qualifiedIdentifier ';' moduleStatements '.' ;
moduleStatements: moduleImport* variableDeclarationBlock? (subprogram | subprogramDeclaration)* moduleBody=subprogramBody ;

moduleImport: IMPORT qualifiedIdentifier ';' ;

subprogramDeclaration: subprogramHeading ';' ;
subprogram: subprogramHeading variableDeclarationBlock? subprogramBody ';' ;
subprogramHeading: PROCEDURE IDENTIFIER '(' paramList? ')' (':' type)?;


paramList: variableDeclaration (',' variableDeclaration)*;
variableDeclaration: IDENTIFIER (',' IDENTIFIER)* ':' type ;
variableDeclarationBlock: VAR variableDeclaration (';' variableDeclaration)* ';' ;

subprogramBody: BEGIN statement* END;

statement: ( assignment
           | returnStatement
           | exitStatement
           | ifStatement
           | whileStatement
           | subprogramCall
           ) ';';

assignment: designator ':=' expression;
subprogramCall: designator '(' expressionList? ')' ;
expressionList: expression (',' expression)* ;
returnStatement: RETURN expression?;
exitStatement: EXIT;

ifStatement: IF expression THEN statement* elseIfStatement* elseStatement? END;
elseIfStatement: ELSIF expression THEN statement*;
elseStatement: ELSE statement*;

whileStatement: WHILE expression DO statement* END;

expression: simpleExpression (comparisonOp simpleExpression)?;
comparisonOp: '=' | '#' | '<' | '<=' | '>' | '>=';
simpleExpression: unaryOp? term (binaryOp term)*;
unaryOp: '+' | '-';
binaryOp: '+' | '-' | OR;
term: factor (termOp factor)*;
termOp: '/' | '*' | MOD | '&';
factor: '~' factor 
       | '(' expression ')' 
       | subprogramCall 
       | designator 
       | literal;

designator: IDENTIFIER selector*;
selector: '[' expression ']' | '.' IDENTIFIER;
qualifiedIdentifier: IDENTIFIER ('.' IDENTIFIER)*;

literal: REAL_LITERAL | INTEGER_LITERAL | STRING_LITERAL | boolean;
boolean: TRUE | FALSE;

type: INTEGER | REAL | STRING | BOOLEAN | ARRAY INTEGER_LITERAL OF type;
