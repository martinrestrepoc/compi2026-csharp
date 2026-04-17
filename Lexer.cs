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
        // Ignora espacios, tabs y saltos de linea antes de tokenizar.
        SkipWhiteSpaces();

        Token token;

        if (string.IsNullOrEmpty(_character))
        {
            // No hay mas caracteres por leer.
            token = new Token(TokenType.Eof, string.Empty);
        }
        else if (_character == "+")
        {
            // Operador suma.
            token = new Token(TokenType.Plus, _character);
        }
        else if (_character == ">")
        {
            // Operador mayor que.
            token = new Token(TokenType.Gt, _character);
        }
        else if (_character == "-")
        {
            // Operador resta.
            token = new Token(TokenType.Minus, _character);
        }
        else if (_character == "^")
        {
            // Operador potencia.
            token = new Token(TokenType.Pow, _character);
        }
        else if (_character == "*")
        {
            // Operador multiplicacion.
            token = new Token(TokenType.Multiply, _character);
        }
        else if (_character == "%")
        {
            // Operador modulo.
            token = new Token(TokenType.Mod, _character);
        }
        else if (_character == "!" && PeekCharacter() == "=")
        {
            // Operador de diferencia: !=
            token = MakeTwoCharacterToken(TokenType.Dif);
        }
        else if (_character == "!")
        {
            // Operador de negacion.
            token = new Token(TokenType.Negation, _character);
        }
        else if (_character == "=" && PeekCharacter() == "=")
        {
            // Operador de igualdad: ==
            token = MakeTwoCharacterToken(TokenType.Eq);
        }
        else if (_character == "=")
        {
            // Operador de asignacion.
            token = new Token(TokenType.Assign, _character);
        }
        else if (IsLetter(_character))
        {
            // Si empieza con letra, puede ser identificador o keyword.
            var literal = ReadIdentifier();
            token = new Token(TokenLookup.LookupTokenType(literal), literal);
        }
        else if (IsDigit(_character))
        {
            // Si empieza con digito, consume el numero completo.
            token = new Token(TokenType.Integer, ReadNumber());
        }
        else
        {
            // Cualquier simbolo desconocido se marca como ilegal.
            token = new Token(TokenType.Illegal, _character);
        }

        // Avanza para dejar listo el siguiente analisis.
        ReadCharacter();
        return token;
    }

    // Consume todos los espacios en blanco consecutivos.
    private void SkipWhiteSpaces()
    {
        while (!string.IsNullOrEmpty(_character) && char.IsWhiteSpace(_character[0]))
        {
            ReadCharacter();
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

    // Lee un literal numerico continuo.
    private string ReadNumber()
    {
        var start = _position;
        while (IsDigit(_character))
        {
            ReadCharacter();
        }

        return _source[start.._position];
    }

    // Lee un identificador o palabra reservada compuesto por letras.
    private string ReadIdentifier()
    {
        var start = _position;
        while (IsLetter(_character))
        {
            ReadCharacter();
        }

        return _source[start.._position];
    }

    // Mira el siguiente caracter sin consumirlo.
    private string PeekCharacter()
    {
        return _readPosition >= _source.Length
            ? string.Empty
            : _source[_readPosition].ToString();
    }

    // Construye tokens de dos caracteres, por ejemplo == o !=.
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
        return !string.IsNullOrEmpty(character) && char.IsLetter(character[0]);
    }

    // Verifica si el caracter actual es un digito.
    private static bool IsDigit(string character)
    {
        return !string.IsNullOrEmpty(character) && char.IsDigit(character[0]);
    }
}
