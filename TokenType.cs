namespace frances;

// Enum con todos los tipos de token que puede producir el lexer.
public enum TokenType
{
    // Operadores aritmeticos
    Division,
    Assign,
    Comma,
    Dif,
    Eq,
    Gt,
    Gte,
    Lt,
    Lte,
    Plus,
    Minus,
    Negation,
    Pow,
    Multiply,
    Mod,

    // Operadores logicos y booleanos
    And,
    Or,

    // Delimitadores
    LParen,
    RParen,
    LBrace,
    RBrace,
    Semicolon,

    // Literales
    Identifier,
    Integer,
    Float,
    String,
    True,
    False,

    // Keywords
    Function,
    Let,
    If,
    Else,
    ElseIf,
    For,
    While,
    Return,
    Break,
    Continue,
    Print,

    // Especiales
    Eof,
    Illegal
}
