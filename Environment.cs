namespace frances;

// =============================================================================
// Environment.cs - Entorno de variables
//
// El Environment es la memoria dinamica del interprete: guarda asociaciones
// nombre -> RuntimeObject. Tambien soporta scopes anidados mediante Outer.
//
// Ejemplo:
//   globalEnv -> { x: 10 }
//   funcEnv   -> { y: 5, outer: globalEnv }
//
// Si funcEnv.Get("x") no encuentra x localmente, busca en globalEnv.
// =============================================================================

// Tabla de simbolos usada durante la ejecucion.
public sealed class Environment
{
    private readonly Dictionary<string, RuntimeObject> _store = new(StringComparer.Ordinal);
    private readonly Environment? _outer;

    public Environment(Environment? outer = null)
    {
        _outer = outer;
    }

    // Busca una variable en el scope actual y, si no existe, en el scope externo.
    public RuntimeObject? Get(string name)
    {
        if (_store.TryGetValue(name, out var value))
        {
            return value;
        }

        return _outer?.Get(name);
    }

    // Define o reemplaza una variable en el scope actual.
    public RuntimeObject Set(string name, RuntimeObject value)
    {
        _store[name] = value;
        return value;
    }

    public override string ToString()
    {
        var items = string.Join(", ", _store.Select(item => $"{item.Key}={item.Value.Inspect()}"));
        return $"Environment({{{items}}})";
    }

    // Crea un entorno hijo que conserva acceso al entorno externo.
    public static Environment NewEnclosedEnvironment(Environment outer)
    {
        return new Environment(outer);
    }
}
