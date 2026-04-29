namespace frances;

// El REPL lee codigo del usuario, lo parsea y lo evalua.
// El Environment se conserva entre entradas para que las variables sigan vivas.
public static class Repl
{
    // Inicia el ciclo interactivo del lenguaje.
    public static void Start()
    {
        var env = new Environment();

        while (true)
        {
            // Prompt de entrada.
            Console.Write(">> ");
            var source = Console.ReadLine();

            // Permite salir escribiendo salir() o cerrando la entrada.
            if (source is null || source == "salir()")
            {
                break;
            }

            if (source == "ayuda()")
            {
                PrintHelp();
                continue;
            }

            source = ReadMultilineSource(source);
            Runner.RunSource(source, env, printFinalResult: true);
        }
    }

    // Lee lineas adicionales cuando hay llaves abiertas.
    private static string ReadMultilineSource(string source)
    {
        var openBraces = Count(source, '{') - Count(source, '}');

        while (openBraces > 0)
        {
            Console.Write(".. ");
            var nextLine = Console.ReadLine();
            if (nextLine is null)
            {
                break;
            }

            source += System.Environment.NewLine + nextLine;
            openBraces += Count(nextLine, '{') - Count(nextLine, '}');
        }

        return source;
    }

    private static int Count(string source, char target)
    {
        return source.Count(character => character == target);
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Comandos:");
        Console.WriteLine("  salir()  termina el REPL");
        Console.WriteLine("  ayuda()  muestra esta ayuda");
        Console.WriteLine();
        Console.WriteLine("Ejemplos:");
        Console.WriteLine("  let x = 10;");
        Console.WriteLine("  print(x * 2);");
        Console.WriteLine("  let doble = function(n) { return n * 2; };");
        Console.WriteLine("  doble(5);");
    }
}
