namespace frances;

// Representa una unidad lexica producida por el lexer.
public readonly record struct Token(TokenType TokenType, string Literal)
{
    // Formato legible para mostrar tokens en consola.
    public override string ToString()
    {
        return $"Type {TokenType}, Literal {Literal}";
    }
}
