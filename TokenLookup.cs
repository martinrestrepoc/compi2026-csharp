namespace frances;

// Se encarga de distinguir identificadores normales
// de palabras reservadas del lenguaje.
public static class TokenLookup
{
    // Tabla de palabras clave reconocidas por el lexer.
    private static readonly Dictionary<string, TokenType> Keywords = new(StringComparer.Ordinal)
    {
        ["function"] = TokenType.FUNCTION,
        ["for"] = TokenType.FOR,
        ["let"] = TokenType.LET,
        ["if"] = TokenType.IF,
        ["else"] = TokenType.ELSE,
        ["elseif"] = TokenType.ELSEIF,
        ["while"] = TokenType.WHILE,
        ["return"] = TokenType.RETURN,
        ["break"] = TokenType.BREAK,
        ["continue"] = TokenType.CONTINUE,
        ["true"] = TokenType.TRUE,
        ["false"] = TokenType.FALSE,
        ["and"] = TokenType.AND,
        ["or"] = TokenType.OR,
        ["print"] = TokenType.PRINT
    };

    // Devuelve el tipo reservado si existe; si no, Identifier.
    public static TokenType LookupTokenType(string literal)
    {
        // out indica que el parametro no entra con un valor, sino que el metodo lo va a retornar 
        if (Keywords.TryGetValue(literal, out var tokenType))
        {
            return tokenType;
        }
        else
        {
            return TokenType.IDENTIFIER;
        }
    }
}
