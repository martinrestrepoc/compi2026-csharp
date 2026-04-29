using frances;

// Punto de entrada del programa.
//
// Uso:
//   dotnet run
//   dotnet run -- examples/test_script.hdp
if (args.Length > 0)
{
    return Runner.RunFile(args[0]);
}

Console.WriteLine("prueba \"frances\" el nuevo lenguaje de programacion (.hdp)");
Repl.Start();
return 0;
