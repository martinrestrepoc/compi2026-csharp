namespace frances;

// El REPL lee una linea del usuario, la tokeniza y muestra
// cada token hasta que se alcance el fin de entrada.
public static class Repl
{
    // Token de referencia para detectar el fin del analisis.
    private static readonly Token EofToken = new(TokenType.EOF, string.Empty);

    // Inicia el ciclo interactivo del lenguaje.
    public static void Start()
    {
        while (true)
        {
            // Prompt de entrada.
            Console.Write(">>");
            var source = Console.ReadLine();

            // Permite salir escribiendo salir() o cerrando la entrada.
            if (source is null || source == "salir()")
            {
                break;
            }

            // Crea un lexer nuevo por cada linea ingresada.
            var lexer = new Lexer(source);
            Token token;

            do
            {
                // Obtiene e imprime los tokens generados.
                token = lexer.NextToken();
                if (token != EofToken)
                {
                    Console.WriteLine(token);
                }
            }
            while (token != EofToken);
        }
    }
}
