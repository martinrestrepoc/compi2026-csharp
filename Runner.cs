namespace frances;

// =============================================================================
// Runner.cs - Flujo completo de ejecucion
//
// Centraliza el pipeline del interprete:
//
//   source -> Lexer -> Parser -> AST -> Evaluator -> RuntimeObject
//
// Program.cs lo usa para ejecutar archivos .hdp y Repl.cs lo usa para evaluar
// entradas interactivas conservando el mismo Environment.
// =============================================================================

public static class Runner
{
    // Lee y ejecuta un archivo fuente. La extension recomendada del lenguaje es .hdp.
    public static int RunFile(string filepath)
    {
        if (!File.Exists(filepath))
        {
            Console.Error.WriteLine($"Error: El archivo '{filepath}' no existe.");
            return 1;
        }

        var source = File.ReadAllText(filepath);
        var env = new Environment();
        var result = RunSource(source, env, printFinalResult: true);

        return result is ErrorObject ? 1 : 0;
    }

    // Ejecuta codigo fuente usando el Environment recibido.
    public static RuntimeObject RunSource(string source, Environment env, bool printFinalResult)
    {
        var lexer = new Lexer(source);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();

        if (parser.Errors.Count > 0)
        {
            Console.Error.WriteLine("Errores de parseo:");
            foreach (var error in parser.Errors)
            {
                Console.Error.WriteLine($"  - {error}");
            }

            return new ErrorObject("errores de parseo");
        }

        var result = Evaluator.Evaluate(program, env);

        if (result is ErrorObject)
        {
            Console.Error.WriteLine($"Error en ejecucion: {result.Inspect()}");
            return result;
        }

        if (printFinalResult && result.Type != ObjectType.NULL)
        {
            Console.WriteLine(result.Inspect());
        }

        return result;
    }
}
