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

        if (string.IsNullOrEmpty(_character))
        {
            // No hay mas caracteres por leer.
            token = new Token(TokenType.EOF, string.Empty);
        }
        else if (_character == "+")
        {
            // Operador suma.
            token = new Token(TokenType.PLUS, _character);
        }
        else if (_character == ">")
        {
            // Operador mayor que o mayor o igual que.
            if (PeekCharacter() == "=")
            {
                token = MakeTwoCharacterToken(TokenType.GTE);
            }
            else
            {
                token = new Token(TokenType.GT, _character);
            }
        }
        else if (_character == "-")
        {
            // Operador resta.
            token = new Token(TokenType.MINUS, _character);
        }
        else if (_character == "^")
        {
            // Operador potencia.
            token = new Token(TokenType.POW, _character);
        }
        else if (_character == "*")
        {
            // Operador multiplicacion.
            token = new Token(TokenType.MULTIPLY, _character);
        }
        else if (_character == "/")
        {
            // Operador division. Los comentarios // se saltan antes.
            token = new Token(TokenType.DIVISION, _character);
        }
        else if (_character == "%")
        {
            // Operador modulo.
            token = new Token(TokenType.MOD, _character);
        }
        else if (_character == "!" && PeekCharacter() == "=")
        {
            // Operador de diferencia: !=
            token = MakeTwoCharacterToken(TokenType.DIF);
        }
        else if (_character == "!")
        {
            // Operador de negacion.
            token = new Token(TokenType.NEGATION, _character);
        }
        else if (_character == "=" && PeekCharacter() == "=")
        {
            // Operador de igualdad: ==
            token = MakeTwoCharacterToken(TokenType.EQ);
        }
        else if (_character == "=")
        {
            // Operador de asignacion.
            token = new Token(TokenType.ASSIGN, _character);
        }
        else if (_character == "<" && PeekCharacter() == "=")
        {
            // Operador menor o igual que: <=
            token = MakeTwoCharacterToken(TokenType.LTE);
        }
        else if (_character == "<")
        {
            // Operador menor que.
            token = new Token(TokenType.LT, _character);
        }
        else if (_character == ",")
        {
            // Separador de argumentos o elementos.
            token = new Token(TokenType.COMMA, _character);
        }
        else if (_character == ";")
        {
            // Delimitador de fin de sentencia.
            token = new Token(TokenType.SEMICOLON, _character);
        }
        else if (_character == "(")
        {
            // Parentesis de apertura.
            token = new Token(TokenType.LPAREN, _character);
        }
        else if (_character == ")")
        {
            // Parentesis de cierre.
            token = new Token(TokenType.RPAREN, _character);
        }
        else if (_character == "{")
        {
            // Llave de apertura.
            token = new Token(TokenType.LBRACE, _character);
        }
        else if (_character == "}")
        {
            // Llave de cierre.
            token = new Token(TokenType.RBRACE, _character);
        }
        else if (_character == "\"")
        {
            // Cadena encerrada entre comillas dobles.
            token = new Token(TokenType.STRING, ReadString());
            return token;
        }
        else if (IsLetter(_character))
        {
            // Si empieza con letra o _, puede ser identificador o keyword. 
            var literal = ReadIdentifier();
            token = new Token(TokenLookup.LookupTokenType(literal), literal);
            return token;
        }
        else if (IsDigit(_character))
        {
            // Si empieza con digito, consume entero o flotante. 
            var (literal, tokenType) = ReadNumber();
            token = new Token(tokenType, literal);
            return token;
        }
        else if (_character == "\"")
        {
            // Cualquier simbolo desconocido se marca como ilegal.
            token = new Token(TokenType.Illegal, _character);
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
