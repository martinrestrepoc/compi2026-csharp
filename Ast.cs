using System.Globalization;

namespace frances;

// =============================================================================
// Ast.cs - Arbol Sintactico Abstracto (AST)
//
// El AST es la representacion intermedia del programa. El parser convierte la
// secuencia de tokens del lexer en estos nodos, y el evaluator los recorre para
// ejecutar el lenguaje.
//
// Jerarquia:
//   Node
//   - Statement: sentencias como let, return, print, while, for.
//   - Expression: valores o calculos como 5, x, x + y, function(...) { }.
// =============================================================================

// Clase base de todos los nodos del AST.
public abstract class Node
{
    // Retorna el literal del token principal que origino este nodo.
    public abstract string TokenLiteral();

    // Retorna una representacion legible del nodo para pruebas y debug.
    public abstract override string ToString();
}

// Nodo base para sentencias. Las sentencias ejecutan acciones.
public abstract class Statement : Node
{
}

// Nodo base para expresiones. Las expresiones producen valores.
public abstract class Expression : Node
{
}

// Nodo raiz del AST. Representa un programa completo.
public sealed class ProgramNode : Node
{
    public List<Statement> Statements { get; } = new();

    public override string TokenLiteral()
    {
        return Statements.Count > 0 ? Statements[0].TokenLiteral() : string.Empty;
    }

    public override string ToString()
    {
        return string.Join('\n', Statements.Select(statement => statement.ToString()));
    }
}

// Nodo para declaracion de variable: let <nombre> = <valor>;
public sealed class LetStatement : Statement
{
    public LetStatement(Token token, Identifier name, Expression? value)
    {
        Token = token;
        Name = name;
        Value = value;
    }

    public Token Token { get; }

    public Identifier Name { get; }

    public Expression? Value { get; set; }

    public override string TokenLiteral()
    {
        return Token.Literal;
    }

    public override string ToString()
    {
        return $"let {Name} = {Value?.ToString() ?? string.Empty};";
    }
}

// Nodo para sentencia de retorno: return <valor>;
public sealed class ReturnStatement : Statement
{
    public ReturnStatement(Token token, Expression? returnValue)
    {
        Token = token;
        ReturnValue = returnValue;
    }

    public Token Token { get; }

    public Expression? ReturnValue { get; set; }

    public override string TokenLiteral()
    {
        return Token.Literal;
    }

    public override string ToString()
    {
        return $"return {ReturnValue?.ToString() ?? string.Empty};";
    }
}

// Nodo para una expresion usada como sentencia completa.
public sealed class ExpressionStatement : Statement
{
    public ExpressionStatement(Token token, Expression? expression = null)
    {
        Token = token;
        Expression = expression;
    }

    public Token Token { get; }

    public Expression? Expression { get; set; }

    public override string TokenLiteral()
    {
        return Token.Literal;
    }

    public override string ToString()
    {
        return Expression?.ToString() ?? string.Empty;
    }
}

// Nodo para bloques delimitados por llaves: { stmt; stmt; }
public sealed class BlockStatement : Statement
{
    public BlockStatement(Token token)
    {
        Token = token;
    }

    public Token Token { get; }

    public List<Statement> Statements { get; } = new();

    public override string TokenLiteral()
    {
        return Token.Literal;
    }

    public override string ToString()
    {
        var statements = string.Join('\n', Statements.Select(statement => $"  {statement}"));
        return $"{{\n{statements}\n}}";
    }
}

// Nodo para sentencia de impresion: print(<expresion>);
public sealed class PrintStatement : Statement
{
    public PrintStatement(Token token, Expression value)
    {
        Token = token;
        Value = value;
    }

    public Token Token { get; }

    public Expression Value { get; set; }

    public override string TokenLiteral()
    {
        return Token.Literal;
    }

    public override string ToString()
    {
        return $"print({Value});";
    }
}

// Nodo para bucle while: while (<condicion>) { <cuerpo> }
public sealed class WhileStatement : Statement
{
    public WhileStatement(Token token, Expression condition, BlockStatement body)
    {
        Token = token;
        Condition = condition;
        Body = body;
    }

    public Token Token { get; }

    public Expression Condition { get; set; }

    public BlockStatement Body { get; set; }

    public override string TokenLiteral()
    {
        return Token.Literal;
    }

    public override string ToString()
    {
        return $"while ({Condition}) {Body}";
    }
}

// Nodo para bucle for: for (<init>; <condicion>; <update>) { <cuerpo> }
public sealed class ForStatement : Statement
{
    public ForStatement(
        Token token,
        Statement? init,
        Expression? condition,
        Statement? update,
        BlockStatement body)
    {
        Token = token;
        Init = init;
        Condition = condition;
        Update = update;
        Body = body;
    }

    public Token Token { get; }

    public Statement? Init { get; set; }

    public Expression? Condition { get; set; }

    public Statement? Update { get; set; }

    public BlockStatement Body { get; set; }

    public override string TokenLiteral()
    {
        return Token.Literal;
    }

    public override string ToString()
    {
        return $"for ({Init}; {Condition}; {Update}) {Body}";
    }
}

// Nodo para break; dentro de ciclos.
public sealed class BreakStatement : Statement
{
    public BreakStatement(Token token)
    {
        Token = token;
    }

    public Token Token { get; }

    public override string TokenLiteral()
    {
        return Token.Literal;
    }

    public override string ToString()
    {
        return "break;";
    }
}

// Nodo para continue; dentro de ciclos.
public sealed class ContinueStatement : Statement
{
    public ContinueStatement(Token token)
    {
        Token = token;
    }

    public Token Token { get; }

    public override string TokenLiteral()
    {
        return Token.Literal;
    }

    public override string ToString()
    {
        return "continue;";
    }
}

// Nodo para identificadores: nombres de variables o funciones.
public sealed class Identifier : Expression
{
    public Identifier(Token token, string value)
    {
        Token = token;
        Value = value;
    }

    public Token Token { get; }

    public string Value { get; }

    public override string TokenLiteral()
    {
        return Token.Literal;
    }

    public override string ToString()
    {
        return Value;
    }
}

// Nodo para literales enteros: 42, 0, 100.
public sealed class IntegerLiteral : Expression
{
    public IntegerLiteral(Token token, long value)
    {
        Token = token;
        Value = value;
    }

    public Token Token { get; }

    public long Value { get; }

    public override string TokenLiteral()
    {
        return Token.Literal;
    }

    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}

// Nodo para literales flotantes: 3.14, 0.5, 2.0.
public sealed class FloatLiteral : Expression
{
    public FloatLiteral(Token token, double value)
    {
        Token = token;
        Value = value;
    }

    public Token Token { get; }

    public double Value { get; }

    public override string TokenLiteral()
    {
        return Token.Literal;
    }

    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}

// Nodo para literales string: "hola".
public sealed class StringLiteral : Expression
{
    public StringLiteral(Token token, string value)
    {
        Token = token;
        Value = value;
    }

    public Token Token { get; }

    public string Value { get; }

    public override string TokenLiteral()
    {
        return Token.Literal;
    }

    public override string ToString()
    {
        return $"\"{Value}\"";
    }
}

// Nodo para literales booleanos: true o false.
public sealed class BooleanLiteral : Expression
{
    public BooleanLiteral(Token token, bool value)
    {
        Token = token;
        Value = value;
    }

    public Token Token { get; }

    public bool Value { get; }

    public override string TokenLiteral()
    {
        return Token.Literal;
    }

    public override string ToString()
    {
        return Value ? "true" : "false";
    }
}

// Nodo para expresiones prefijas: !true, -5.
public sealed class PrefixExpression : Expression
{
    public PrefixExpression(Token token, string operatorLiteral, Expression? right = null)
    {
        Token = token;
        Operator = operatorLiteral;
        Right = right;
    }

    public Token Token { get; }

    public string Operator { get; }

    public Expression? Right { get; set; }

    public override string TokenLiteral()
    {
        return Token.Literal;
    }

    public override string ToString()
    {
        return $"({Operator}{Right?.ToString() ?? string.Empty})";
    }
}

// Nodo para expresiones infijas: 5 + 3, x == y.
public sealed class InfixExpression : Expression
{
    public InfixExpression(Token token, Expression left, string operatorLiteral, Expression? right = null)
    {
        Token = token;
        Left = left;
        Operator = operatorLiteral;
        Right = right;
    }

    public Token Token { get; }

    public Expression Left { get; set; }

    public string Operator { get; }

    public Expression? Right { get; set; }

    public override string TokenLiteral()
    {
        return Token.Literal;
    }

    public override string ToString()
    {
        return $"({Left} {Operator} {Right?.ToString() ?? string.Empty})";
    }
}

// Nodo para condicionales: if (...) { } elseif (...) { } else { }
public sealed class IfExpression : Expression
{
    public IfExpression(
        Token token,
        Expression condition,
        BlockStatement consequence,
        List<(Expression Condition, BlockStatement Block)>? alternatives = null,
        BlockStatement? elseBlock = null)
    {
        Token = token;
        Condition = condition;
        Consequence = consequence;
        Alternatives = alternatives ?? new List<(Expression Condition, BlockStatement Block)>();
        ElseBlock = elseBlock;
    }

    public Token Token { get; }

    public Expression Condition { get; set; }

    public BlockStatement Consequence { get; set; }

    public List<(Expression Condition, BlockStatement Block)> Alternatives { get; }

    public BlockStatement? ElseBlock { get; set; }

    public override string TokenLiteral()
    {
        return Token.Literal;
    }

    public override string ToString()
    {
        var result = $"if ({Condition}) {Consequence}";

        foreach (var (condition, block) in Alternatives)
        {
            result += $" elseif ({condition}) {block}";
        }

        if (ElseBlock is not null)
        {
            result += $" else {ElseBlock}";
        }

        return result;
    }
}

// Nodo para definicion de funcion: function(a, b) { return a + b; }
public sealed class FunctionLiteral : Expression
{
    public FunctionLiteral(Token token, List<Identifier> parameters, BlockStatement body, string name = "")
    {
        Token = token;
        Parameters = parameters;
        Body = body;
        Name = name;
    }

    public Token Token { get; }

    public List<Identifier> Parameters { get; }

    public BlockStatement Body { get; set; }

    public string Name { get; set; }

    public override string TokenLiteral()
    {
        return Token.Literal;
    }

    public override string ToString()
    {
        var parameters = string.Join(", ", Parameters.Select(parameter => parameter.ToString()));
        var name = string.IsNullOrEmpty(Name) ? string.Empty : $" {Name}";
        return $"function{name}({parameters}) {Body}";
    }
}

// Nodo para llamada a funcion: sumar(1, 2).
public sealed class CallExpression : Expression
{
    public CallExpression(Token token, Expression function, List<Expression> arguments)
    {
        Token = token;
        Function = function;
        Arguments = arguments;
    }

    public Token Token { get; }

    public Expression Function { get; set; }

    public List<Expression> Arguments { get; }

    public override string TokenLiteral()
    {
        return Token.Literal;
    }

    public override string ToString()
    {
        var arguments = string.Join(", ", Arguments.Select(argument => argument.ToString()));
        return $"{Function}({arguments})";
    }
}
