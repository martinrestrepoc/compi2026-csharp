namespace frances;

// El lexer recorre el texto de entrada caracter por caracter
// y lo transforma en una secuencia de tokens.
public sealed class Lexer
{
    // Fuente completa que se va a analizar.
    private readonly string _source;

    // Caracter actual bajo analisis.
    private string _character = string.Empty;

    // Posicion del caracter actual.
    private int _position;

    // Posicion del siguiente caracter que se va a leer.
    private int _readPosition;

    // Inicializa el lexer y carga el primer caracter.
    public Lexer(string source)
    {
        _source = source;
        ReadCharacter();
    }

    // Devuelve el siguiente token reconocido en la entrada.
    public Token NextToken()
    {
        // Ignora espacios, tabs, saltos de linea y comentarios simples.
        SkipWhiteSpacesAndComments();

        Token token;

        switch (_character)
        {
            // No hay mas caracteres por leer.
            case "":
                token = new Token(TokenType.EOF, string.Empty);
                break;

            // Operadores aritmeticos simples.
            case "+":
                token = new Token(TokenType.PLUS, _character);
                break;
            case "-":
                token = new Token(TokenType.MINUS, _character);
                break;
            case "^":
                token = new Token(TokenType.POW, _character);
                break;
            case "*":
                token = new Token(TokenType.MULTIPLY, _character);
                break;
            case "/":
                token = new Token(TokenType.DIVISION, _character);
                break;
            case "%":
                token = new Token(TokenType.MOD, _character);
                break;

            // Operadores de uno o dos caracteres.
            case ">":
                token = PeekCharacter() == "="
                    ? MakeTwoCharacterToken(TokenType.GTE)
                    : new Token(TokenType.GT, _character);
                break;
            case "!":
                token = PeekCharacter() == "="
                    ? MakeTwoCharacterToken(TokenType.DIF)
                    : new Token(TokenType.NEGATION, _character);
                break;
            case "=":
                token = PeekCharacter() == "="
                    ? MakeTwoCharacterToken(TokenType.EQ)
                    : new Token(TokenType.ASSIGN, _character);
                break;
            case "<":
                token = PeekCharacter() == "="
                    ? MakeTwoCharacterToken(TokenType.LTE)
                    : new Token(TokenType.LT, _character);
                break;

            // Delimitadores.
            case ",":
                token = new Token(TokenType.COMMA, _character);
                break;
            case ";":
                token = new Token(TokenType.SEMICOLON, _character);
                break;
            case "(":
                token = new Token(TokenType.LPAREN, _character);
                break;
            case ")":
                token = new Token(TokenType.RPAREN, _character);
                break;
            case "{":
                token = new Token(TokenType.LBRACE, _character);
                break;
            case "}":
                token = new Token(TokenType.RBRACE, _character);
                break;

            // Literales compuestos: estos metodos ya avanzan el cursor.
            case "\"":
                token = new Token(TokenType.STRING, ReadString());
                return token;

            default:
                if (IsLetter(_character))
                {
                    var literal = ReadIdentifier();
                    token = new Token(TokenLookup.LookupTokenType(literal), literal);
                    return token;
                }

                if (IsDigit(_character))
                {
                    var (literal, tokenType) = ReadNumber();
                    token = new Token(tokenType, literal);
                    return token;
                }

                token = new Token(TokenType.ILLEGAL, _character);
                break;
        }

        // Avanza para dejar listo el siguiente analisis.
        ReadCharacter();
        return token;
    }

    // Consume todos los espacios en blanco consecutivos.
    private void SkipWhiteSpacesAndComments()
    {
        while (true)
        {
            if (!string.IsNullOrEmpty(_character) && char.IsWhiteSpace(_character[0]))
            {
                ReadCharacter();
            }
            else if (_character == "/" && PeekCharacter() == "/")
            {
                while (!string.IsNullOrEmpty(_character) && _character != "\n")
                {
                    ReadCharacter();
                }
            }
            else
            {
                break;
            }
        }
    }

    // Lee un nuevo caracter y actualiza las posiciones internas.
    private void ReadCharacter()
    {
        if (_readPosition >= _source.Length)
        {
            _character = string.Empty;
        }
        else
        {
            _character = _source[_readPosition].ToString();
        }

        _position = _readPosition;
        _readPosition += 1;
    }

    // Lee un literal numerico continuo, entero o decimal.
    private (string Literal, TokenType TokenType) ReadNumber()
    {
        var start = _position;
        while (IsDigit(_character))
        {
            ReadCharacter();
        }

        var tokenType = TokenType.INTEGER;

        if (_character == "." && IsDigit(PeekCharacter()))
        {
            tokenType = TokenType.FLOAT;
            ReadCharacter();

            while (IsDigit(_character))
            {
                ReadCharacter();
            }
        }

        return (_source[start.._position], tokenType);
    }

    // Lee un identificador o palabra reservada compuesto por letras, numeros o _.
    private string ReadIdentifier()
    {
        var start = _position;
        while (IsLetter(_character) || IsDigit(_character))
        {
            ReadCharacter();
        }
        // toma de start hasta _position, sin incluir _position, el texto que forma el identificador o keyword
        return _source[start.._position];
    }

    // Lee el contenido de una cadena entre comillas dobles.
    private string ReadString()
    {
        var start = _position + 1;

        do
        {
            ReadCharacter();
        }
        while (!string.IsNullOrEmpty(_character) && _character != "\"");

        var literal = _source[start.._position];
        ReadCharacter();
        return literal;
    }

    // Mira el siguiente caracter sin consumirlo.
    private string PeekCharacter()
    {
        if (_readPosition >= _source.Length)
        {
            return string.Empty;
        }

        return _source[_readPosition].ToString();
    }

    // Construye tokens de dos caracteres.
    private Token MakeTwoCharacterToken(TokenType tokenType)
    {
        var prefix = _character;
        ReadCharacter();
        var suffix = _character;
        return new Token(tokenType, $"{prefix}{suffix}");
    }

    // Verifica si el caracter actual puede iniciar o continuar un identificador.
    private static bool IsLetter(string character)
    {
        return !string.IsNullOrEmpty(character) && (char.IsLetter(character[0]) || character[0] == '_');
    }

    // Verifica si el caracter actual es un digito.
    private static bool IsDigit(string character)
    {
        return !string.IsNullOrEmpty(character) && char.IsDigit(character[0]);
    }
}
