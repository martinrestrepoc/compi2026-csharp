# frances

`frances` es una implementacion en C# de un interprete educativo.

El interprete ejecuta codigo fuente con extension recomendada `.hdp`. El flujo completo es:

```txt
codigo fuente .hdp
  -> Lexer
  -> Parser
  -> AST
  -> Evaluator
  -> resultado / salida / error
```

La implementacion mantiene nombres internos claros y comentarios en espanol para estudiar cada etapa del interprete.

## Estado actual

- Lexer completo.
- Parser Pratt con precedencia de operadores.
- AST completo para sentencias y expresiones.
- Sistema de objetos runtime.
- Environment con scopes anidados.
- Evaluator tree-walking.
- REPL evaluador.
- Ejecucion de archivos `.hdp`.
- Ejemplos en `examples/`.

## Requisitos

Necesitas:

- .NET SDK 7.0 o compatible.
- Terminal con acceso a `dotnet`.

Verifica la instalacion:

```bash
dotnet --version
```

## Como ejecutar

Todos los comandos de esta seccion se ejecutan desde la raiz del repo clonado.

Iniciar el REPL:

```bash
dotnet run
```

Ejecutar un archivo `.hdp`:

```bash
dotnet run -- examples/test_script.hdp
```

Ejecutar el ejemplo completo:

```bash
dotnet run -- examples/final.hdp
```

## REPL

Si ejecutas el proyecto sin archivo, se abre un REPL:

```txt
prueba "frances" el nuevo lenguaje de programacion (.hdp)
>> 
```

Comandos especiales:

- `salir()` termina el REPL.
- `ayuda()` muestra ejemplos rapidos.

El REPL conserva el mismo `Environment` entre entradas:

```txt
>> let x = 10;
>> print(x * 5);
50
>> let doble = function(n) { return n * 2; };
>> doble(7);
14
```

Para bloques con `{ ... }`, el REPL permite entrada multilinea mientras haya llaves abiertas.

## Ejemplos .hdp

`examples/test_script.hdp`:

```txt
let x = 10;
let y = 5;
print("El resultado es:");
print(x * y);
```

Salida esperada:

```txt
El resultado es:
50
```

`examples/final.hdp` cubre aritmetica, strings, condicionales, ciclos, closures, factorial, fibonacci y negacion logica.

## Estructura

Archivos principales:

- `Program.cs`: punto de entrada. Si recibe un path, ejecuta archivo; si no, abre el REPL.
- `Runner.cs`: centraliza el pipeline `Lexer -> Parser -> Evaluator`.
- `Repl.cs`: ciclo interactivo con environment persistente.
- `Lexer.cs`: convierte texto fuente en tokens.
- `Token.cs`: representa un token individual.
- `TokenType.cs`: enum de tokens en MAYUSCULAS.
- `TokenLookup.cs`: distingue keywords de identificadores.
- `Parser.cs`: parser Pratt que construye el AST.
- `Ast.cs`: nodos del arbol sintactico abstracto.
- `ObjectSystem.cs`: valores runtime del lenguaje.
- `Environment.cs`: tabla de simbolos con scopes anidados.
- `Evaluator.cs`: interprete tree-walking que ejecuta el AST.

## Flujo interno

### 1. Program

`Program.cs` decide el modo de ejecucion:

```txt
con argumento    -> Runner.RunFile(path)
sin argumentos   -> Repl.Start()
```

### 2. Runner

`Runner.RunSource(...)` ejecuta el pipeline:

```txt
Lexer(source)
Parser(lexer)
parser.ParseProgram()
Evaluator.Evaluate(program, env)
```

Si hay errores de parseo, los escribe en `Console.Error`. Si hay errores runtime, imprime `Error en ejecucion`.

### 3. Lexer

`Lexer.NextToken()` lee caracter por caracter y produce tokens.

Reconoce:

- operadores: `+`, `-`, `*`, `/`, `%`, `^`
- comparaciones: `==`, `!=`, `<`, `<=`, `>`, `>=`
- logicos: `and`, `or`, `!`
- delimitadores: `,`, `;`, `(`, `)`, `{`, `}`
- literales: enteros, flotantes, strings, booleans
- keywords: `function`, `let`, `return`, `if`, `elseif`, `else`, `while`, `for`, `break`, `continue`, `print`
- comentarios de linea: `// comentario`

### 4. Parser

`Parser` convierte tokens en AST. Usa Pratt Parsing para expresiones, con precedencias:

```txt
LOWEST
OR
AND
EQUALS
LESSGREATER
SUM
PRODUCT
PREFIX
POWER
CALL
```

Ejemplo:

```txt
2 + 3 * 4
```

se parsea como:

```txt
2 + (3 * 4)
```

### 5. AST

`Ast.cs` define la estructura intermedia del programa.

Sentencias:

- `LetStatement`
- `ReturnStatement`
- `ExpressionStatement`
- `PrintStatement`
- `BlockStatement`
- `WhileStatement`
- `ForStatement`
- `BreakStatement`
- `ContinueStatement`

Expresiones:

- `Identifier`
- `IntegerLiteral`
- `FloatLiteral`
- `StringLiteral`
- `BooleanLiteral`
- `PrefixExpression`
- `InfixExpression`
- `IfExpression`
- `FunctionLiteral`
- `CallExpression`

### 6. Object System

`ObjectSystem.cs` define los valores que existen en runtime:

- `IntegerObject`
- `FloatObject`
- `BooleanObject`
- `StringObject`
- `NullObject`
- `ReturnValueObject`
- `ErrorObject`
- `FunctionObject`
- `BreakSignal`
- `ContinueSignal`

Tambien expone singletons reutilizables:

```txt
RuntimeObjects.TRUE
RuntimeObjects.FALSE
RuntimeObjects.NULL
RuntimeObjects.BREAK
RuntimeObjects.CONTINUE
```

### 7. Environment

`Environment` guarda variables:

```txt
nombre -> RuntimeObject
```

Tambien permite scopes anidados para funciones y closures:

```txt
funcEnv.Get("x")
  -> busca en funcEnv
  -> si no existe, busca en outer
```

### 8. Evaluator

`Evaluator.Evaluate(node, env)` recorre el AST.

Soporta:

- literales
- variables con `let`
- `print`
- operadores prefijos `!` y `-`
- operadores infijos
- strings
- `if / elseif / else`
- `while`
- `for`
- `break`
- `continue`
- `return`
- funciones
- closures
- recursividad

## Sintaxis soportada

Variables:

```txt
let x = 10;
let nombre = "Laura";
```

Aritmetica:

```txt
2 + 3 * 4;
17 % 5;
2 ^ 8;
```

Condicionales:

```txt
if (x > 0) {
    print("positivo");
} elseif (x == 0) {
    print("cero");
} else {
    print("negativo");
}
```

While:

```txt
let i = 0;
while (i < 5) {
    let i = i + 1;
    print(i);
}
```

For:

```txt
for (let i = 1; i <= 5; let i = i + 1) {
    print(i);
}
```

Funciones:

```txt
let doble = function(n) {
    return n * 2;
};

print(doble(7));
```

Closures:

```txt
let crear_multiplicador = function(factor) {
    return function(x) {
        return x * factor;
    };
};

let triple = crear_multiplicador(3);
print(triple(7));
```

Recursion:

```txt
let fact = function(n) {
    if (n <= 1) { return 1; }
    return n * fact(n - 1);
};

print(fact(5));
```


## Archivos generados

No se deben versionar `bin/` ni `obj/`.

El repo incluye `.gitignore` para ignorarlos:

```gitignore
bin/
obj/
```

Estos directorios se regeneran automaticamente con:

```bash
dotnet build
dotnet run
```

## Verificacion local

Desde la raiz del repo puedes verificar que el proyecto compila con:

```bash
dotnet build
```

Tambien puedes validar el flujo completo ejecutando:

```bash
dotnet run -- examples/test_script.hdp
dotnet run -- examples/final.hdp
```

Salida esperada de `examples/test_script.hdp`:

```txt
El resultado es:
50
```

El ejemplo `examples/final.hdp` debe terminar imprimiendo:

```txt
true
```
