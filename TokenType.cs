namespace frances;

// Enum con todos los tipos de token que puede producir el lexer.
//
// Los nombres se mantienen en MAYUSCULAS para dejar una convencion estable
// entre lexer, parser, AST, evaluator y sistema de objetos.
public enum TokenType
{
    // Operadores aritmeticos
    PLUS,
    MINUS,
    MULTIPLY,
    DIVISION,
    MOD,
    POW,

    // Operadores de comparacion
    EQ,
    DIF,
    LT,
    LTE,
    GT,
    GTE,

    // Operadores logicos
    AND,
    OR,
    NEGATION,

    // Operador de asignacion
    ASSIGN,

    // Delimitadores y puntuacion
    COMMA,
    SEMICOLON,
    LPAREN,
    RPAREN,
    LBRACE,
    RBRACE,

    // Tipos de datos literales
    INTEGER,
    FLOAT,
    STRING,
    TRUE,
    FALSE,

    // Identificadores
    IDENTIFIER,

    // Palabras reservadas
    FUNCTION,
    LET,
    RETURN,
    IF,
    ELSEIF,
    ELSE,
    WHILE,
    FOR,
    BREAK,
    CONTINUE,
    PRINT,

    // Tokens especiales
    EOF,
    ILLEGAL
}
