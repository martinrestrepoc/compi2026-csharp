namespace frances;

// Se encarga de distinguir identificadores normales
// de palabras reservadas del lenguaje.
public static class TokenLookup
{
    // Tabla de palabras clave reconocidas por el lexer.
    private static readonly Dictionary<string, TokenType> Keywords = new(StringComparer.Ordinal)
    {
        ["function"] = TokenType.Function,
        ["for"] = TokenType.For,
        ["let"] = TokenType.Let,
        ["if"] = TokenType.If,
        ["else"] = TokenType.Else,
        ["elseif"] = TokenType.ElseIf,
        ["while"] = TokenType.While,
        ["return"] = TokenType.Return,
        ["continue"] = TokenType.Continue
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
            return TokenType.Identifier;
        }
    }
}

