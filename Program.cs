using frances;

// Punto de entrada del programa.
//
// Uso:
//   dotnet run --project compi2026-csharp
//   dotnet run --project compi2026-csharp -- examples/test_script.hdp
if (args.Length > 0)
{
    return Runner.RunFile(args[0]);
}

Console.WriteLine("prueba \"frances\" el nuevo lenguaje de programacion (.hdp)");
Repl.Start();
return 0;
