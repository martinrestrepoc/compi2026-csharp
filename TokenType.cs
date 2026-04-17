namespace frances;

// Enum con todos los tipos de token que puede producir el lexer.
public enum TokenType
{
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
    LParen,
    LBrace,
    RBrace,
    Semicolon,
    Identifier,
    Integer,
    String,
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
    Eof,
    Illegal
}
