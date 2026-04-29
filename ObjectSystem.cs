using System.Globalization;

namespace frances;

// =============================================================================
// ObjectSystem.cs - Sistema de objetos en tiempo de ejecucion
//
// Cuando el Evaluator recorra el AST, cada expresion producira un RuntimeObject.
// Este archivo define los valores que el lenguaje puede manejar en runtime:
//
//   IntegerObject  -> 42
//   FloatObject    -> 3.14
//   BooleanObject  -> true / false
//   StringObject   -> "hola"
//   NullObject     -> ausencia de valor
//   ReturnValue    -> senal interna para propagar return
//   ErrorObject    -> error en tiempo de ejecucion
//   FunctionObject -> funcion con parametros, cuerpo y entorno lexico
// =============================================================================

// Categorias de objetos que existen durante la ejecucion del lenguaje.
public enum ObjectType
{
    INTEGER,
    FLOAT,
    BOOLEAN,
    STRING,
    NULL,
    RETURN,
    ERROR,
    FUNCTION,
    BREAK,
    CONTINUE
}

// Clase base para todos los valores runtime del lenguaje.
public abstract class RuntimeObject
{
    public abstract ObjectType Type { get; }

    // Representacion legible del valor. El REPL y print usaran este metodo.
    public abstract string Inspect();

    public override string ToString()
    {
        return Inspect();
    }
}

// Valor entero del lenguaje.
public sealed class IntegerObject : RuntimeObject
{
    public IntegerObject(long value)
    {
        Value = value;
    }

    public long Value { get; }

    public override ObjectType Type => ObjectType.INTEGER;

    public override string Inspect()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}

// Valor decimal del lenguaje.
public sealed class FloatObject : RuntimeObject
{
    public FloatObject(double value)
    {
        Value = value;
    }

    public double Value { get; }

    public override ObjectType Type => ObjectType.FLOAT;

    public override string Inspect()
    {
        var formatted = Value.ToString("G", CultureInfo.InvariantCulture);

        if (!formatted.Contains('.') && !formatted.Contains('E') && !formatted.Contains('e'))
        {
            formatted += ".0";
        }

        return formatted;
    }
}

// Valor booleano del lenguaje.
public sealed class BooleanObject : RuntimeObject
{
    public BooleanObject(bool value)
    {
        Value = value;
    }

    public bool Value { get; }

    public override ObjectType Type => ObjectType.BOOLEAN;

    public override string Inspect()
    {
        return Value ? "true" : "false";
    }
}

// Valor string del lenguaje.
public sealed class StringObject : RuntimeObject
{
    public StringObject(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public override ObjectType Type => ObjectType.STRING;

    public override string Inspect()
    {
        return Value;
    }
}

// Representa ausencia de valor.
public sealed class NullObject : RuntimeObject
{
    public override ObjectType Type => ObjectType.NULL;

    public override string Inspect()
    {
        return "null";
    }
}

// Senal interna para propagar return hasta salir de una funcion.
public sealed class ReturnValueObject : RuntimeObject
{
    public ReturnValueObject(RuntimeObject value)
    {
        Value = value;
    }

    public RuntimeObject Value { get; }

    public override ObjectType Type => ObjectType.RETURN;

    public override string Inspect()
    {
        return Value.Inspect();
    }
}

// Representa un error en tiempo de ejecucion.
public sealed class ErrorObject : RuntimeObject
{
    public ErrorObject(string message)
    {
        Message = message;
    }

    public string Message { get; }

    public override ObjectType Type => ObjectType.ERROR;

    public override string Inspect()
    {
        return $"ERROR: {Message}";
    }
}

// Objeto funcion. Guarda parametros, cuerpo y entorno donde fue creada.
public sealed class FunctionObject : RuntimeObject
{
    public FunctionObject(
        List<Identifier> parameters,
        BlockStatement body,
        Environment env,
        string name = "")
    {
        Parameters = parameters;
        Body = body;
        Env = env;
        Name = name;
    }

    public List<Identifier> Parameters { get; }

    public BlockStatement Body { get; }

    public Environment Env { get; }

    public string Name { get; }

    public override ObjectType Type => ObjectType.FUNCTION;

    public override string Inspect()
    {
        var parameters = string.Join(", ", Parameters.Select(parameter => parameter.ToString()));
        var name = string.IsNullOrEmpty(Name) ? string.Empty : $" {Name}";
        return $"function{name}({parameters}) {{ ... }}";
    }
}

// Senal interna para salir de un ciclo.
public sealed class BreakSignal : RuntimeObject
{
    public override ObjectType Type => ObjectType.BREAK;

    public override string Inspect()
    {
        return "break";
    }
}

// Senal interna para saltar a la siguiente iteracion de un ciclo.
public sealed class ContinueSignal : RuntimeObject
{
    public override ObjectType Type => ObjectType.CONTINUE;

    public override string Inspect()
    {
        return "continue";
    }
}

// Singletons runtime reutilizables por el evaluator.
public static class RuntimeObjects
{
    public static readonly BooleanObject TRUE = new(true);
    public static readonly BooleanObject FALSE = new(false);
    public static readonly NullObject NULL = new();
    public static readonly BreakSignal BREAK = new();
    public static readonly ContinueSignal CONTINUE = new();
}
