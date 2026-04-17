# LauraSeFue en C#

Implementacion en C# del mismo flujo que existe en `compi2026`:

- `Program` imprime el banner inicial.
- `Repl` lee lineas hasta que el usuario escriba `salir()`.
- `Lexer` convierte la entrada en tokens y los imprime uno por uno.

## Ejecutar

```bash
dotnet run --project compi2026-csharp
```

## Flujo del REPL

```txt
Laura se fue Laura no esta :'(
>>10 + 3 * 2 - 1
Type Integer, Literal 10
Type Plus, Literal +
Type Integer, Literal 3
Type Multiply, Literal *
Type Integer, Literal 2
Type Minus, Literal -
Type Integer, Literal 1
>>salir()
```
