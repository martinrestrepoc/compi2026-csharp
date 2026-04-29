namespace frances;

// =============================================================================
// Evaluator.cs - Evaluador del interprete
//
// Recorre el AST y produce RuntimeObject. Esta etapa implementa la base:
// literales, variables, print, operadores prefijos, operadores infijos,
// strings, truthiness y errores basicos.
//
// Flujo:
//   Codigo fuente -> Lexer -> Parser -> AST -> Evaluator -> RuntimeObject
// =============================================================================

public static class Evaluator
{
    // Punto de entrada del evaluador. Despacha segun el tipo concreto de nodo.
    public static RuntimeObject Evaluate(Node? node, Environment env)
    {
        if (node is null)
        {
            return NewError("nodo nulo");
        }

        return node switch
        {
            ProgramNode program => EvalProgram(program, env),
            ExpressionStatement statement => Evaluate(statement.Expression, env),
            BlockStatement statement => EvalBlockStatement(statement, env),
            LetStatement statement => EvalLetStatement(statement, env),
            ReturnStatement statement => EvalReturnStatement(statement, env),
            PrintStatement statement => EvalPrintStatement(statement, env),
            WhileStatement statement => EvalWhileStatement(statement, env),
            ForStatement statement => EvalForStatement(statement, env),
            BreakStatement => RuntimeObjects.BREAK,
            ContinueStatement => RuntimeObjects.CONTINUE,

            IntegerLiteral literal => new IntegerObject(literal.Value),
            FloatLiteral literal => new FloatObject(literal.Value),
            StringLiteral literal => new StringObject(literal.Value),
            BooleanLiteral literal => NativeBoolToBooleanObject(literal.Value),
            Identifier identifier => EvalIdentifier(identifier, env),
            PrefixExpression expression => EvalPrefixExpression(expression, env),
            InfixExpression expression => EvalInfixExpression(expression, env),
            IfExpression expression => EvalIfExpression(expression, env),
            FunctionLiteral expression => new FunctionObject(expression.Parameters, expression.Body, env, expression.Name),
            CallExpression expression => EvalCallExpression(expression, env),

            _ => NewError($"nodo no soportado todavia: {node.GetType().Name}")
        };
    }

    // Evalua todas las sentencias del programa en orden.
    private static RuntimeObject EvalProgram(ProgramNode program, Environment env)
    {
        RuntimeObject result = RuntimeObjects.NULL;

        foreach (var statement in program.Statements)
        {
            result = Evaluate(statement, env);

            if (result is ReturnValueObject returnValue)
            {
                return returnValue.Value;
            }

            if (result is ErrorObject)
            {
                return result;
            }
        }

        return result;
    }

    // Evalua un bloque y propaga senales de control sin desenvolverlas.
    private static RuntimeObject EvalBlockStatement(BlockStatement block, Environment env)
    {
        RuntimeObject result = RuntimeObjects.NULL;

        foreach (var statement in block.Statements)
        {
            result = Evaluate(statement, env);

            if (result.Type is ObjectType.RETURN
                or ObjectType.ERROR
                or ObjectType.BREAK
                or ObjectType.CONTINUE)
            {
                return result;
            }
        }

        return result;
    }

    // Evalua let <nombre> = <expresion>; y guarda el valor en el environment.
    private static RuntimeObject EvalLetStatement(LetStatement statement, Environment env)
    {
        var value = Evaluate(statement.Value, env);
        if (IsError(value))
        {
            return value;
        }

        env.Set(statement.Name.Value, value);
        return RuntimeObjects.NULL;
    }

    // Evalua return <expresion>; envolviendo el valor para propagacion posterior.
    private static RuntimeObject EvalReturnStatement(ReturnStatement statement, Environment env)
    {
        var value = Evaluate(statement.ReturnValue, env);
        return IsError(value) ? value : new ReturnValueObject(value);
    }

    // Evalua print(<expresion>); escribiendo en consola.
    private static RuntimeObject EvalPrintStatement(PrintStatement statement, Environment env)
    {
        var value = Evaluate(statement.Value, env);
        if (IsError(value))
        {
            return value;
        }

        Console.WriteLine(value.Inspect());
        return RuntimeObjects.NULL;
    }

    // Evalua while (<condicion>) { <cuerpo> }.
    private static RuntimeObject EvalWhileStatement(WhileStatement statement, Environment env)
    {
        RuntimeObject result = RuntimeObjects.NULL;

        while (true)
        {
            var condition = Evaluate(statement.Condition, env);
            if (IsError(condition))
            {
                return condition;
            }

            if (!IsTruthy(condition))
            {
                break;
            }

            result = Evaluate(statement.Body, env);

            if (ReferenceEquals(result, RuntimeObjects.BREAK))
            {
                return RuntimeObjects.NULL;
            }

            if (ReferenceEquals(result, RuntimeObjects.CONTINUE))
            {
                continue;
            }

            if (result is ReturnValueObject or ErrorObject)
            {
                return result;
            }
        }

        return result is BreakSignal or ContinueSignal ? RuntimeObjects.NULL : result;
    }

    // Evalua for (<init>; <condicion>; <update>) { <cuerpo> }.
    private static RuntimeObject EvalForStatement(ForStatement statement, Environment env)
    {
        if (statement.Init is not null)
        {
            var initResult = Evaluate(statement.Init, env);
            if (IsError(initResult))
            {
                return initResult;
            }
        }

        while (true)
        {
            if (statement.Condition is not null)
            {
                var condition = Evaluate(statement.Condition, env);
                if (IsError(condition))
                {
                    return condition;
                }

                if (!IsTruthy(condition))
                {
                    break;
                }
            }

            var result = Evaluate(statement.Body, env);

            if (ReferenceEquals(result, RuntimeObjects.BREAK))
            {
                return RuntimeObjects.NULL;
            }

            if (result is ReturnValueObject or ErrorObject)
            {
                return result;
            }

            if (statement.Update is not null)
            {
                var updateResult = Evaluate(statement.Update, env);
                if (IsError(updateResult))
                {
                    return updateResult;
                }
            }
        }

        return RuntimeObjects.NULL;
    }

    // Busca un identificador en el environment.
    private static RuntimeObject EvalIdentifier(Identifier identifier, Environment env)
    {
        var value = env.Get(identifier.Value);
        return value ?? NewError($"variable no definida: '{identifier.Value}'");
    }

    // Evalua expresiones prefijas como !x o -x.
    private static RuntimeObject EvalPrefixExpression(PrefixExpression expression, Environment env)
    {
        var right = Evaluate(expression.Right, env);
        if (IsError(right))
        {
            return right;
        }

        return expression.Operator switch
        {
            "!" => EvalBangOperator(right),
            "-" => EvalMinusPrefixOperator(right),
            _ => NewError($"operador prefijo desconocido: '{expression.Operator}'")
        };
    }

    // Evalua expresiones infijas como x + y o x == y.
    private static RuntimeObject EvalInfixExpression(InfixExpression expression, Environment env)
    {
        var left = Evaluate(expression.Left, env);
        if (IsError(left))
        {
            return left;
        }

        var right = Evaluate(expression.Right, env);
        if (IsError(right))
        {
            return right;
        }

        return EvalInfixOperator(expression.Operator, left, right);
    }

    // Evalua if / elseif / else. Si ninguna rama aplica, retorna null.
    private static RuntimeObject EvalIfExpression(IfExpression expression, Environment env)
    {
        var condition = Evaluate(expression.Condition, env);
        if (IsError(condition))
        {
            return condition;
        }

        if (IsTruthy(condition))
        {
            return Evaluate(expression.Consequence, env);
        }

        foreach (var (alternativeCondition, alternativeBlock) in expression.Alternatives)
        {
            var alternativeValue = Evaluate(alternativeCondition, env);
            if (IsError(alternativeValue))
            {
                return alternativeValue;
            }

            if (IsTruthy(alternativeValue))
            {
                return Evaluate(alternativeBlock, env);
            }
        }

        return expression.ElseBlock is not null
            ? Evaluate(expression.ElseBlock, env)
            : RuntimeObjects.NULL;
    }

    // Evalua una llamada a funcion: funcion(arg1, arg2).
    private static RuntimeObject EvalCallExpression(CallExpression expression, Environment env)
    {
        var function = Evaluate(expression.Function, env);
        if (IsError(function))
        {
            return function;
        }

        if (function is not FunctionObject functionObject)
        {
            return NewError($"'{expression.Function}' no es una funcion, es {function.Type}");
        }

        var args = EvalExpressions(expression.Arguments, env);
        if (args.Count == 1 && IsError(args[0]))
        {
            return args[0];
        }

        if (args.Count != functionObject.Parameters.Count)
        {
            return NewError(
                $"numero de argumentos incorrecto: esperados {functionObject.Parameters.Count}, recibidos {args.Count}");
        }

        var functionEnv = ExtendFunctionEnv(functionObject, args);
        var evaluated = Evaluate(functionObject.Body, functionEnv);

        return UnwrapReturnValue(evaluated);
    }

    // Evalua argumentos de izquierda a derecha.
    private static List<RuntimeObject> EvalExpressions(List<Expression> expressions, Environment env)
    {
        var result = new List<RuntimeObject>();

        foreach (var expression in expressions)
        {
            var evaluated = Evaluate(expression, env);
            if (IsError(evaluated))
            {
                return new List<RuntimeObject> { evaluated };
            }

            result.Add(evaluated);
        }

        return result;
    }

    // Crea un entorno hijo y enlaza parametros con argumentos.
    private static Environment ExtendFunctionEnv(FunctionObject function, List<RuntimeObject> args)
    {
        var env = Environment.NewEnclosedEnvironment(function.Env);

        for (var i = 0; i < function.Parameters.Count; i += 1)
        {
            env.Set(function.Parameters[i].Value, args[i]);
        }

        return env;
    }

    // El cuerpo de una funcion retorna ReturnValueObject; la llamada expone solo el valor real.
    private static RuntimeObject UnwrapReturnValue(RuntimeObject obj)
    {
        return obj is ReturnValueObject returnValue ? returnValue.Value : obj;
    }

    // Implementa negacion logica con las reglas de truthiness del lenguaje.
    private static RuntimeObject EvalBangOperator(RuntimeObject right)
    {
        return IsTruthy(right) ? RuntimeObjects.FALSE : RuntimeObjects.TRUE;
    }

    // Implementa negacion numerica.
    private static RuntimeObject EvalMinusPrefixOperator(RuntimeObject right)
    {
        return right switch
        {
            IntegerObject integer => new IntegerObject(-integer.Value),
            FloatObject floatObject => new FloatObject(-floatObject.Value),
            _ => NewError($"operador '-' no soportado para {right.Type}")
        };
    }

    // Despacha operadores infijos segun los tipos de operandos.
    private static RuntimeObject EvalInfixOperator(string operatorLiteral, RuntimeObject left, RuntimeObject right)
    {
        if (left is IntegerObject leftInteger && right is IntegerObject rightInteger)
        {
            return EvalIntegerInfixExpression(operatorLiteral, leftInteger, rightInteger);
        }

        if (left is IntegerObject or FloatObject && right is IntegerObject or FloatObject)
        {
            return EvalFloatInfixExpression(operatorLiteral, NumericValue(left), NumericValue(right));
        }

        if (operatorLiteral == "and")
        {
            return NativeBoolToBooleanObject(IsTruthy(left) && IsTruthy(right));
        }

        if (operatorLiteral == "or")
        {
            return NativeBoolToBooleanObject(IsTruthy(left) || IsTruthy(right));
        }

        if (operatorLiteral == "==")
        {
            return NativeBoolToBooleanObject(ObjectsAreEqual(left, right));
        }

        if (operatorLiteral == "!=")
        {
            return NativeBoolToBooleanObject(!ObjectsAreEqual(left, right));
        }

        if (left is StringObject leftString && right is StringObject rightString)
        {
            return EvalStringInfixExpression(operatorLiteral, leftString, rightString);
        }

        if (left.Type != right.Type)
        {
            return NewError($"tipos incompatibles: {left.Type} {operatorLiteral} {right.Type}");
        }

        return NewError($"operador '{operatorLiteral}' no soportado entre {left.Type} y {right.Type}");
    }

    // Operaciones entre enteros.
    private static RuntimeObject EvalIntegerInfixExpression(
        string operatorLiteral,
        IntegerObject left,
        IntegerObject right)
    {
        var leftValue = left.Value;
        var rightValue = right.Value;

        return operatorLiteral switch
        {
            "+" => new IntegerObject(leftValue + rightValue),
            "-" => new IntegerObject(leftValue - rightValue),
            "*" => new IntegerObject(leftValue * rightValue),
            "/" => EvalIntegerDivision(leftValue, rightValue),
            "%" => rightValue == 0
                ? NewError("modulo por cero")
                : new IntegerObject(leftValue % rightValue),
            "^" => EvalIntegerPower(leftValue, rightValue),
            "==" => NativeBoolToBooleanObject(leftValue == rightValue),
            "!=" => NativeBoolToBooleanObject(leftValue != rightValue),
            "<" => NativeBoolToBooleanObject(leftValue < rightValue),
            "<=" => NativeBoolToBooleanObject(leftValue <= rightValue),
            ">" => NativeBoolToBooleanObject(leftValue > rightValue),
            ">=" => NativeBoolToBooleanObject(leftValue >= rightValue),
            _ => NewError($"operador '{operatorLiteral}' no soportado entre enteros")
        };
    }

    // Division entera exacta produce IntegerObject; si no es exacta produce FloatObject.
    private static RuntimeObject EvalIntegerDivision(long leftValue, long rightValue)
    {
        if (rightValue == 0)
        {
            return NewError("division por cero");
        }

        if (leftValue % rightValue == 0)
        {
            return new IntegerObject(leftValue / rightValue);
        }

        return new FloatObject((double)leftValue / rightValue);
    }

    // Potencia entera. Exponente negativo produce float.
    private static RuntimeObject EvalIntegerPower(long leftValue, long rightValue)
    {
        var result = Math.Pow(leftValue, rightValue);

        return rightValue < 0
            ? new FloatObject(result)
            : new IntegerObject((long)result);
    }

    // Operaciones numericas cuando hay flotantes o mezcla int/float.
    private static RuntimeObject EvalFloatInfixExpression(
        string operatorLiteral,
        double leftValue,
        double rightValue)
    {
        return operatorLiteral switch
        {
            "+" => new FloatObject(leftValue + rightValue),
            "-" => new FloatObject(leftValue - rightValue),
            "*" => new FloatObject(leftValue * rightValue),
            "/" => rightValue == 0.0
                ? NewError("division por cero")
                : new FloatObject(leftValue / rightValue),
            "%" => rightValue == 0.0
                ? NewError("modulo por cero")
                : new FloatObject(leftValue % rightValue),
            "^" => new FloatObject(Math.Pow(leftValue, rightValue)),
            "==" => NativeBoolToBooleanObject(leftValue == rightValue),
            "!=" => NativeBoolToBooleanObject(leftValue != rightValue),
            "<" => NativeBoolToBooleanObject(leftValue < rightValue),
            "<=" => NativeBoolToBooleanObject(leftValue <= rightValue),
            ">" => NativeBoolToBooleanObject(leftValue > rightValue),
            ">=" => NativeBoolToBooleanObject(leftValue >= rightValue),
            _ => NewError($"operador '{operatorLiteral}' no soportado entre flotantes")
        };
    }

    // Operaciones entre strings.
    private static RuntimeObject EvalStringInfixExpression(
        string operatorLiteral,
        StringObject left,
        StringObject right)
    {
        return operatorLiteral switch
        {
            "+" => new StringObject(left.Value + right.Value),
            "==" => NativeBoolToBooleanObject(left.Value == right.Value),
            "!=" => NativeBoolToBooleanObject(left.Value != right.Value),
            _ => NewError($"operador '{operatorLiteral}' no soportado entre strings")
        };
    }

    // Determina verdad/falsedad semantica del lenguaje.
    private static bool IsTruthy(RuntimeObject obj)
    {
        return obj switch
        {
            NullObject => false,
            BooleanObject boolean => boolean.Value,
            IntegerObject integer => integer.Value != 0,
            FloatObject floatObject => floatObject.Value != 0.0,
            StringObject stringObject => stringObject.Value.Length > 0,
            _ => true
        };
    }

    // Compara objetos por valor semantico.
    private static bool ObjectsAreEqual(RuntimeObject left, RuntimeObject right)
    {
        if (left is IntegerObject or FloatObject && right is IntegerObject or FloatObject)
        {
            return NumericValue(left) == NumericValue(right);
        }

        if (left.Type != right.Type)
        {
            return false;
        }

        return (left, right) switch
        {
            (BooleanObject leftBoolean, BooleanObject rightBoolean) => leftBoolean.Value == rightBoolean.Value,
            (StringObject leftString, StringObject rightString) => leftString.Value == rightString.Value,
            (NullObject, NullObject) => true,
            _ => ReferenceEquals(left, right)
        };
    }

    // Extrae valor double de IntegerObject o FloatObject.
    private static double NumericValue(RuntimeObject obj)
    {
        return obj switch
        {
            IntegerObject integer => integer.Value,
            FloatObject floatObject => floatObject.Value,
            _ => throw new InvalidOperationException($"Objeto no numerico: {obj.Type}")
        };
    }

    // Convierte bool nativo al singleton booleano del lenguaje.
    private static BooleanObject NativeBoolToBooleanObject(bool value)
    {
        return value ? RuntimeObjects.TRUE : RuntimeObjects.FALSE;
    }

    private static bool IsError(RuntimeObject obj)
    {
        return obj is ErrorObject;
    }

    private static ErrorObject NewError(string message)
    {
        return new ErrorObject(message);
    }
}
