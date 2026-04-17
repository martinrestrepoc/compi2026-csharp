# frances

`frances` es una implementacion en C# de un lexer con REPL interactivo.

El objetivo del proyecto es leer una linea de codigo fuente, recorrerla caracter por caracter, convertirla en tokens y mostrarlos en consola. El proyecto replica el flujo del lexer base de `compi2026`, pero adaptado a C# y ampliado con mas tipos de tokens.

## Objetivo del proyecto

Este proyecto busca:

- Mostrar de manera clara como se hace el analisis lexico.
- Servir como base para extender el lenguaje `frances`.
- Permitir pruebas rapidas desde consola con un REPL sencillo.
- Tener una implementacion legible para estudiar compiladores.

## Flujo general

El funcionamiento del programa sigue este orden:

1. `Program.cs` arranca la aplicacion y muestra el mensaje inicial.
2. `Repl.cs` abre un ciclo interactivo con el prompt `>>`.
3. El usuario escribe una linea de entrada.
4. `Lexer.cs` recorre esa linea y va produciendo tokens.
5. Cada token se imprime en consola hasta llegar a `EOF`.
6. El proceso se repite hasta que el usuario escriba `salir()`.

## Estructura del proyecto

Los archivos principales son:

- `Program.cs`: punto de entrada de la aplicacion.
- `Repl.cs`: ciclo interactivo de lectura y visualizacion de tokens.
- `Lexer.cs`: analizador lexico que recorre el texto y produce tokens.
- `Token.cs`: representacion de un token individual.
- `TokenType.cs`: enum con las categorias de token reconocidas.
- `TokenLookup.cs`: tabla para identificar palabras reservadas.
- `compi2026-csharp.csproj`: configuracion del proyecto .NET.

## Requisitos

Para ejecutar este proyecto necesitas:

- .NET SDK 7.0 o compatible.
- Terminal con acceso al comando `dotnet`.

Puedes verificar tu instalacion con:

```bash
dotnet --version
```

## Como ejecutar el proyecto

Desde la carpeta padre `compiladores`:

```bash
dotnet run --project compi2026-csharp
```

O entrando directamente a la carpeta del proyecto:

```bash
cd compi2026-csharp
dotnet run
```

Cuando el programa inicia, muestra:

```txt
prueba "frances" el nuevo lenguaje de programacion
```

Luego queda esperando entrada del usuario con el prompt:

```txt
>>
```

## Como salir del REPL

Para cerrar la ejecucion del lenguaje, escribe:

```txt
salir()
```

Tambien se puede terminar cerrando la entrada estandar.

## Ejemplo de uso

Entrada:

```txt
>>10 + 3 * 2 - 1
```

Salida:

```txt
Type Integer, Literal 10
Type Plus, Literal +
Type Integer, Literal 3
Type Multiply, Literal *
Type Integer, Literal 2
Type Minus, Literal -
Type Integer, Literal 1
```

Otro ejemplo:

```txt
>>let valor == 10 != 20
Type Let, Literal let
Type Identifier, Literal valor
Type Eq, Literal ==
Type Integer, Literal 10
Type Dif, Literal !=
Type Integer, Literal 20
```

Otro ejemplo con keywords, flotantes y strings:

```txt
>>print true and false or break 3.14 "hola"
Type Print, Literal print
Type True, Literal true
Type And, Literal and
Type False, Literal false
Type Or, Literal or
Type Break, Literal break
Type Float, Literal 3.14
Type String, Literal hola
```

## Tokens reconocidos actualmente

El lexer ya reconoce las siguientes categorias:

### Operadores simples

- `+` -> `Plus`
- `-` -> `Minus`
- `*` -> `Multiply`
- `/` -> `Division`
- `%` -> `Mod`
- `^` -> `Pow`
- `<` -> `Lt`
- `>` -> `Gt`
- `=` -> `Assign`
- `!` -> `Negation`

### Operadores dobles

- `==` -> `Eq`
- `!=` -> `Dif`
- `<=` -> `Lte`
- `>=` -> `Gte`

### Operadores logicos y booleanos

- `and` -> `And`
- `or` -> `Or`
- `true` -> `True`
- `false` -> `False`

### Delimitadores

- `,` -> `Comma`
- `;` -> `Semicolon`
- `(` -> `LParen`
- `)` -> `RParen`
- `{` -> `LBrace`
- `}` -> `RBrace`

### Literales

- Enteros, por ejemplo: `10`, `25`, `300`
- Flotantes, por ejemplo: `3.14`, `20.5`
- Strings entre comillas dobles, por ejemplo: `"hola"`
- Identificadores alfanumericos con guion bajo, por ejemplo: `x`, `valor`, `mi_variable1`

### Palabras reservadas

- `function`
- `for`
- `let`
- `if`
- `else`
- `elseif`
- `while`
- `return`
- `break`
- `continue`
- `print`

### Tokens especiales

- `Eof`: fin de entrada
- `Illegal`: simbolo no reconocido

## Como funciona el lexer

La clase `Lexer` mantiene cuatro piezas principales de estado:

- `_source`: texto completo recibido como entrada.
- `_character`: caracter actual que se esta analizando.
- `_position`: posicion actual dentro del texto.
- `_readPosition`: posicion del siguiente caracter por leer.

Con ese estado, el lexer hace lo siguiente:

1. Ignora espacios en blanco y comentarios de linea con `SkipWhiteSpacesAndComments()`.
2. Observa el caracter actual.
3. Decide si corresponde a:
   - un operador de un caracter,
   - un operador de dos caracteres,
   - un numero entero o flotante,
   - un string,
   - un identificador o keyword,
   - o un simbolo ilegal.
4. Crea el token correspondiente.
5. Avanza para preparar la siguiente lectura.

## Comentarios soportados

El lexer ignora comentarios de una sola linea que empiecen con `//`.

Ejemplo:

```txt
let x = 5; // esto no se tokeniza
```

En ese caso, solo se generan tokens para:

```txt
let x = 5;
```

## Como funciona el REPL

La clase `Repl` implementa el ciclo interactivo del programa:

- imprime el prompt `>>`
- lee una linea escrita por el usuario
- crea un lexer nuevo para esa linea
- solicita tokens uno por uno con `NextToken()`
- imprime cada token hasta encontrar `Eof`

Esto permite probar el lexer inmediatamente sin archivos intermedios.
