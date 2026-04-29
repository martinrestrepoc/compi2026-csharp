using System.Globalization;

namespace frances;

// =============================================================================
// Parser.cs - Analizador Sintactico
//
// El parser toma los tokens producidos por el Lexer y construye el AST.
// Usa Pratt Parsing para manejar precedencia de operadores:
//
//   Codigo fuente -> Lexer -> Tokens -> Parser -> AST
//
// Ejemplo:
//   let x = 5 + 3;
//
// Produce un ProgramNode con un LetStatement cuyo Value es un
// InfixExpression(5 + 3).
// =============================================================================

// Niveles de precedencia, de menor a mayor prioridad.
public enum Precedences
{
    LOWEST = 1,
    OR,
    AND,
    EQUALS,
    LESSGREATER,
    SUM,
    PRODUCT,
    PREFIX,
    POWER,
    CALL
}

// Convierte tokens en un AST. Mantiene un token actual y un token siguiente
// para poder validar la gramatica con look-ahead de un token.
public sealed class Parser
{
    private static readonly Dictionary<TokenType, Precedences> TokenPrecedences = new()
    {
        [TokenType.OR] = Precedences.OR,
        [TokenType.AND] = Precedences.AND,
        [TokenType.EQ] = Precedences.EQUALS,
        [TokenType.DIF] = Precedences.EQUALS,
        [TokenType.LT] = Precedences.LESSGREATER,
        [TokenType.LTE] = Precedences.LESSGREATER,
        [TokenType.GT] = Precedences.LESSGREATER,
        [TokenType.GTE] = Precedences.LESSGREATER,
        [TokenType.PLUS] = Precedences.SUM,
        [TokenType.MINUS] = Precedences.SUM,
        [TokenType.MULTIPLY] = Precedences.PRODUCT,
        [TokenType.DIVISION] = Precedences.PRODUCT,
        [TokenType.MOD] = Precedences.PRODUCT,
        [TokenType.POW] = Precedences.POWER,
        [TokenType.LPAREN] = Precedences.CALL
    };

    private readonly Lexer _lexer;
    private readonly List<string> _errors = new();
    private Token _currentToken;
    private Token _peekToken;

    // Inicializa el parser y llena los dos buffers de tokens.
    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        _currentToken = new Token(TokenType.ILLEGAL, string.Empty);
        _peekToken = new Token(TokenType.ILLEGAL, string.Empty);

        AdvanceTokens();
        AdvanceTokens();
    }

    // Errores acumulados durante el parseo.
    public IReadOnlyList<string> Errors => _errors;

    // Parsea todo el input hasta EOF y retorna el nodo raiz.
    public ProgramNode ParseProgram()
    {
        var program = new ProgramNode();

        while (!CurrentTokenIs(TokenType.EOF))
        {
            var statement = ParseStatement();
            if (statement is not null)
            {
                program.Statements.Add(statement);
            }

            AdvanceTokens();
        }

        return program;
    }

    // Decide que tipo de sentencia parsear segun el token actual.
    private Statement? ParseStatement()
    {
        return _currentToken.TokenType switch
        {
            TokenType.LET => ParseLetStatement(),
            TokenType.RETURN => ParseReturnStatement(),
            TokenType.PRINT => ParsePrintStatement(),
            TokenType.WHILE => ParseWhileStatement(),
            TokenType.FOR => ParseForStatement(),
            TokenType.BREAK => ParseBreakStatement(),
            TokenType.CONTINUE => ParseContinueStatement(),
            _ => ParseExpressionStatement()
        };
    }

    // Parsea: let <nombre> = <expresion>;
    private LetStatement? ParseLetStatement()
    {
        var token = _currentToken;

        if (!ExpectPeek(TokenType.IDENTIFIER))
        {
            return null;
        }

        var name = new Identifier(_currentToken, _currentToken.Literal);

        if (!ExpectPeek(TokenType.ASSIGN))
        {
            return null;
        }

        AdvanceTokens();
        var value = ParseExpression(Precedences.LOWEST);

        if (value is FunctionLiteral functionLiteral)
        {
            functionLiteral.Name = name.Value;
        }

        if (PeekTokenIs(TokenType.SEMICOLON))
        {
            AdvanceTokens();
        }

        return new LetStatement(token, name, value);
    }

    // Parsea: return <expresion>;
    private ReturnStatement ParseReturnStatement()
    {
        var token = _currentToken;

        AdvanceTokens();
        var returnValue = ParseExpression(Precedences.LOWEST);

        if (PeekTokenIs(TokenType.SEMICOLON))
        {
            AdvanceTokens();
        }

        return new ReturnStatement(token, returnValue);
    }

    // Parsea: print(<expresion>);
    private PrintStatement? ParsePrintStatement()
    {
        var token = _currentToken;

        if (!ExpectPeek(TokenType.LPAREN))
        {
            return null;
        }

        AdvanceTokens();
        var value = ParseExpression(Precedences.LOWEST);

        if (!ExpectPeek(TokenType.RPAREN))
        {
            return null;
        }

        if (PeekTokenIs(TokenType.SEMICOLON))
        {
            AdvanceTokens();
        }

        return value is null ? null : new PrintStatement(token, value);
    }

    // Parsea: while (<condicion>) { <cuerpo> }
    private WhileStatement? ParseWhileStatement()
    {
        var token = _currentToken;

        if (!ExpectPeek(TokenType.LPAREN))
        {
            return null;
        }

        AdvanceTokens();
        var condition = ParseExpression(Precedences.LOWEST);

        if (condition is null || !ExpectPeek(TokenType.RPAREN))
        {
            return null;
        }

        if (!ExpectPeek(TokenType.LBRACE))
        {
            return null;
        }

        var body = ParseBlockStatement();
        return new WhileStatement(token, condition, body);
    }

    // Parsea: for (<init>; <condicion>; <update>) { <cuerpo> }
    private ForStatement? ParseForStatement()
    {
        var token = _currentToken;

        if (!ExpectPeek(TokenType.LPAREN))
        {
            return null;
        }

        AdvanceTokens();
        Statement? init = null;
        if (!CurrentTokenIs(TokenType.SEMICOLON))
        {
            init = ParseStatement();
        }

        if (!CurrentTokenIs(TokenType.SEMICOLON) && !ExpectPeek(TokenType.SEMICOLON))
        {
            return null;
        }

        AdvanceTokens();
        Expression? condition = null;
        if (!CurrentTokenIs(TokenType.SEMICOLON))
        {
            condition = ParseExpression(Precedences.LOWEST);
        }

        if (!ExpectPeek(TokenType.SEMICOLON))
        {
            return null;
        }

        AdvanceTokens();
        Statement? update = null;
        if (!CurrentTokenIs(TokenType.RPAREN))
        {
            update = ParseStatement();
        }

        if (!CurrentTokenIs(TokenType.RPAREN) && !ExpectPeek(TokenType.RPAREN))
        {
            return null;
        }

        if (!ExpectPeek(TokenType.LBRACE))
        {
            return null;
        }

        var body = ParseBlockStatement();
        return new ForStatement(token, init, condition, update, body);
    }

    // Parsea: break;
    private BreakStatement ParseBreakStatement()
    {
        var token = _currentToken;

        if (PeekTokenIs(TokenType.SEMICOLON))
        {
            AdvanceTokens();
        }

        return new BreakStatement(token);
    }

    // Parsea: continue;
    private ContinueStatement ParseContinueStatement()
    {
        var token = _currentToken;

        if (PeekTokenIs(TokenType.SEMICOLON))
        {
            AdvanceTokens();
        }

        return new ContinueStatement(token);
    }

    // Parsea una expresion usada como sentencia completa.
    private ExpressionStatement ParseExpressionStatement()
    {
        var token = _currentToken;
        var expression = ParseExpression(Precedences.LOWEST);

        if (PeekTokenIs(TokenType.SEMICOLON))
        {
            AdvanceTokens();
        }

        return new ExpressionStatement(token, expression);
    }

    // Parsea un bloque: { stmt1; stmt2; }
    private BlockStatement ParseBlockStatement()
    {
        var block = new BlockStatement(_currentToken);

        AdvanceTokens();

        while (!CurrentTokenIs(TokenType.RBRACE) && !CurrentTokenIs(TokenType.EOF))
        {
            var statement = ParseStatement();
            if (statement is not null)
            {
                block.Statements.Add(statement);
            }

            AdvanceTokens();
        }

        return block;
    }

    // Corazon del Pratt Parser. LOWEST significa "parsea toda la expresion
    // posible hasta que un operador con menor o igual precedencia obligue a parar".
    private Expression? ParseExpression(Precedences precedence)
    {
        var leftExpression = ParsePrefixExpressionByToken();
        if (leftExpression is null)
        {
            return null;
        }

        while (!PeekTokenIs(TokenType.SEMICOLON) && precedence < PeekPrecedence())
        {
            if (!IsInfixToken(_peekToken.TokenType))
            {
                return leftExpression;
            }

            AdvanceTokens();
            leftExpression = ParseInfixExpressionByToken(leftExpression);
        }

        return leftExpression;
    }

    // Despacha la funcion prefix segun el token actual.
    private Expression? ParsePrefixExpressionByToken()
    {
        return _currentToken.TokenType switch
        {
            TokenType.IDENTIFIER => ParseIdentifier(),
            TokenType.INTEGER => ParseIntegerLiteral(),
            TokenType.FLOAT => ParseFloatLiteral(),
            TokenType.STRING => ParseStringLiteral(),
            TokenType.TRUE => ParseBooleanLiteral(),
            TokenType.FALSE => ParseBooleanLiteral(),
            TokenType.MINUS => ParsePrefixExpression(),
            TokenType.NEGATION => ParsePrefixExpression(),
            TokenType.LPAREN => ParseGroupedExpression(),
            TokenType.IF => ParseIfExpression(),
            TokenType.FUNCTION => ParseFunctionLiteral(),
            _ => AddNoPrefixParseFunctionError()
        };
    }

    // Parsea un identificador.
    private Identifier ParseIdentifier()
    {
        return new Identifier(_currentToken, _currentToken.Literal);
    }

    // Parsea un entero.
    private IntegerLiteral? ParseIntegerLiteral()
    {
        if (!long.TryParse(_currentToken.Literal, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            _errors.Add($"No se pudo parsear \"{_currentToken.Literal}\" como entero");
            return null;
        }

        return new IntegerLiteral(_currentToken, value);
    }

    // Parsea un flotante.
    private FloatLiteral? ParseFloatLiteral()
    {
        if (!double.TryParse(_currentToken.Literal, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            _errors.Add($"No se pudo parsear \"{_currentToken.Literal}\" como flotante");
            return null;
        }

        return new FloatLiteral(_currentToken, value);
    }

    // Parsea un string. El lexer ya entrega el contenido sin comillas.
    private StringLiteral ParseStringLiteral()
    {
        return new StringLiteral(_currentToken, _currentToken.Literal);
    }

    // Parsea true o false.
    private BooleanLiteral ParseBooleanLiteral()
    {
        return new BooleanLiteral(_currentToken, CurrentTokenIs(TokenType.TRUE));
    }

    // Parsea !expr o -expr.
    private PrefixExpression ParsePrefixExpression()
    {
        var token = _currentToken;
        var operatorLiteral = _currentToken.Literal;

        AdvanceTokens();
        var right = ParseExpression(Precedences.PREFIX);

        return new PrefixExpression(token, operatorLiteral, right);
    }

    // Parsea una expresion agrupada entre parentesis.
    private Expression? ParseGroupedExpression()
    {
        AdvanceTokens();
        var expression = ParseExpression(Precedences.LOWEST);

        return ExpectPeek(TokenType.RPAREN) ? expression : null;
    }

    // Parsea if (...) { } elseif (...) { } else { }.
    private IfExpression? ParseIfExpression()
    {
        var token = _currentToken;

        if (!ExpectPeek(TokenType.LPAREN))
        {
            return null;
        }

        AdvanceTokens();
        var condition = ParseExpression(Precedences.LOWEST);

        if (condition is null || !ExpectPeek(TokenType.RPAREN))
        {
            return null;
        }

        if (!ExpectPeek(TokenType.LBRACE))
        {
            return null;
        }

        var consequence = ParseBlockStatement();
        var alternatives = new List<(Expression Condition, BlockStatement Block)>();

        while (PeekTokenIs(TokenType.ELSEIF))
        {
            AdvanceTokens();

            if (!ExpectPeek(TokenType.LPAREN))
            {
                return null;
            }

            AdvanceTokens();
            var alternativeCondition = ParseExpression(Precedences.LOWEST);

            if (alternativeCondition is null || !ExpectPeek(TokenType.RPAREN))
            {
                return null;
            }

            if (!ExpectPeek(TokenType.LBRACE))
            {
                return null;
            }

            var alternativeBlock = ParseBlockStatement();
            alternatives.Add((alternativeCondition, alternativeBlock));
        }

        BlockStatement? elseBlock = null;
        if (PeekTokenIs(TokenType.ELSE))
        {
            AdvanceTokens();

            if (!ExpectPeek(TokenType.LBRACE))
            {
                return null;
            }

            elseBlock = ParseBlockStatement();
        }

        return new IfExpression(token, condition, consequence, alternatives, elseBlock);
    }

    // Parsea function(<params>) { <body> }.
    private FunctionLiteral? ParseFunctionLiteral()
    {
        var token = _currentToken;

        if (!ExpectPeek(TokenType.LPAREN))
        {
            return null;
        }

        var parameters = ParseFunctionParameters();

        if (!ExpectPeek(TokenType.LBRACE))
        {
            return null;
        }

        var body = ParseBlockStatement();
        return new FunctionLiteral(token, parameters, body);
    }

    // Parsea la lista de parametros de una funcion.
    private List<Identifier> ParseFunctionParameters()
    {
        var parameters = new List<Identifier>();

        if (PeekTokenIs(TokenType.RPAREN))
        {
            AdvanceTokens();
            return parameters;
        }

        AdvanceTokens();
        parameters.Add(new Identifier(_currentToken, _currentToken.Literal));

        while (PeekTokenIs(TokenType.COMMA))
        {
            AdvanceTokens();
            AdvanceTokens();
            parameters.Add(new Identifier(_currentToken, _currentToken.Literal));
        }

        if (!ExpectPeek(TokenType.RPAREN))
        {
            return new List<Identifier>();
        }

        return parameters;
    }

    // Verifica si el token puede operar como infix.
    private static bool IsInfixToken(TokenType tokenType)
    {
        return tokenType is TokenType.PLUS
            or TokenType.MINUS
            or TokenType.MULTIPLY
            or TokenType.DIVISION
            or TokenType.MOD
            or TokenType.POW
            or TokenType.EQ
            or TokenType.DIF
            or TokenType.LT
            or TokenType.LTE
            or TokenType.GT
            or TokenType.GTE
            or TokenType.AND
            or TokenType.OR
            or TokenType.LPAREN;
    }

    // Despacha parseo infix o llamada segun el token actual.
    private Expression ParseInfixExpressionByToken(Expression left)
    {
        return _currentToken.TokenType == TokenType.LPAREN
            ? ParseCallExpression(left)
            : ParseInfixExpression(left);
    }

    // Parsea expresiones infijas: <left> <operator> <right>.
    private InfixExpression ParseInfixExpression(Expression left)
    {
        var token = _currentToken;
        var operatorLiteral = _currentToken.Literal;
        var precedence = CurrentPrecedence();

        AdvanceTokens();
        var right = ParseExpression(precedence);

        return new InfixExpression(token, left, operatorLiteral, right);
    }

    // Parsea llamada a funcion: funcion(arg1, arg2).
    private CallExpression ParseCallExpression(Expression function)
    {
        var token = _currentToken;
        var arguments = ParseCallArguments();
        return new CallExpression(token, function, arguments);
    }

    // Parsea argumentos de una llamada.
    private List<Expression> ParseCallArguments()
    {
        var arguments = new List<Expression>();

        if (PeekTokenIs(TokenType.RPAREN))
        {
            AdvanceTokens();
            return arguments;
        }

        AdvanceTokens();
        var firstArgument = ParseExpression(Precedences.LOWEST);
        if (firstArgument is not null)
        {
            arguments.Add(firstArgument);
        }

        while (PeekTokenIs(TokenType.COMMA))
        {
            AdvanceTokens();
            AdvanceTokens();

            var argument = ParseExpression(Precedences.LOWEST);
            if (argument is not null)
            {
                arguments.Add(argument);
            }
        }

        if (!ExpectPeek(TokenType.RPAREN))
        {
            return new List<Expression>();
        }

        return arguments;
    }

    // Avanza el buffer de tokens.
    private void AdvanceTokens()
    {
        _currentToken = _peekToken;
        _peekToken = _lexer.NextToken();
    }

    // Verifica si el token actual tiene el tipo indicado.
    private bool CurrentTokenIs(TokenType tokenType)
    {
        return _currentToken.TokenType == tokenType;
    }

    // Verifica si el siguiente token tiene el tipo indicado.
    private bool PeekTokenIs(TokenType tokenType)
    {
        return _peekToken.TokenType == tokenType;
    }

    // Exige que el siguiente token tenga cierto tipo y avanza si coincide.
    private bool ExpectPeek(TokenType tokenType)
    {
        if (PeekTokenIs(tokenType))
        {
            AdvanceTokens();
            return true;
        }

        _errors.Add($"Se esperaba {tokenType}, pero se obtuvo {_peekToken.TokenType}");
        return false;
    }

    // Precedencia del token actual.
    private Precedences CurrentPrecedence()
    {
        return TokenPrecedences.TryGetValue(_currentToken.TokenType, out var precedence)
            ? precedence
            : Precedences.LOWEST;
    }

    // Precedencia del token siguiente.
    private Precedences PeekPrecedence()
    {
        return TokenPrecedences.TryGetValue(_peekToken.TokenType, out var precedence)
            ? precedence
            : Precedences.LOWEST;
    }

    // Registra error cuando no existe parseo prefix para un token.
    private Expression? AddNoPrefixParseFunctionError()
    {
        _errors.Add($"No se encontro funcion de parseo prefix para {_currentToken.TokenType}");
        return null;
    }
}
