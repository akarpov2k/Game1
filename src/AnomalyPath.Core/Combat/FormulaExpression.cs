#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Localization;

namespace AnomalyPath.Core.Combat
{
    /// <summary>
    /// Универсальный вычислитель формул: разбирает выражение один раз при загрузке данных,
    /// в бою только считает. Поддерживает числа, имена, + − × ÷ %, скобки и функции
    /// <c>min</c>, <c>max</c>, <c>abs</c>, <c>floor</c>, <c>ceil</c>, <c>round</c>, <c>clamp</c>.
    /// </summary>
    public sealed class FormulaExpression
    {
        private readonly Node _root;
        private readonly IReadOnlyCollection<string> _usedNames;

        private FormulaExpression(Node root, string source, IReadOnlyCollection<string> usedNames)
        {
            _root = root;
            Source = source;
            _usedNames = usedNames;
        }

        /// <summary>Исходное выражение.</summary>
        public string Source { get; }

        /// <summary>Имена, которые встречаются в выражении.</summary>
        public IReadOnlyCollection<string> UsedNames => _usedNames;

        /// <summary>Разбирает выражение.</summary>
        /// <param name="expression">Текст выражения.</param>
        /// <param name="allowedNames">Имена переменных и констант, доступные формуле.</param>
        /// <param name="sourcePath">Путь для сообщения об ошибке.</param>
        /// <returns>Готовое к вычислению выражение.</returns>
        /// <exception cref="DataException">Синтаксис, неизвестное имя или функция.</exception>
        public static FormulaExpression Parse(string expression, IReadOnlyCollection<string> allowedNames, string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new DataException(sourcePath, CoreErrors.FormulaEmpty);
            }

            var parser = new Parser(expression, allowedNames, sourcePath);
            var root = parser.ParseAll();
            return new FormulaExpression(root, expression, parser.UsedNames);
        }

        /// <summary>Вычисляет выражение.</summary>
        /// <param name="variables">Значения переменных по именам.</param>
        /// <param name="constants">Значения констант баланса.</param>
        /// <returns>Результат.</returns>
        /// <exception cref="DataException">Имя есть в формуле, но значение не передано.</exception>
        public double Evaluate(IReadOnlyDictionary<string, double> variables, IReadOnlyDictionary<string, double> constants)
        {
            return _root.Evaluate(variables, constants);
        }

        private abstract class Node
        {
            public abstract double Evaluate(IReadOnlyDictionary<string, double> variables, IReadOnlyDictionary<string, double> constants);
        }

        private sealed class NumberNode : Node
        {
            private readonly double _value;

            public NumberNode(double value)
            {
                _value = value;
            }

            public override double Evaluate(IReadOnlyDictionary<string, double> variables, IReadOnlyDictionary<string, double> constants) => _value;
        }

        private sealed class NameNode : Node
        {
            private readonly string _name;

            public NameNode(string name)
            {
                _name = name;
            }

            public override double Evaluate(IReadOnlyDictionary<string, double> variables, IReadOnlyDictionary<string, double> constants)
            {
                if (variables.TryGetValue(_name, out var value))
                {
                    return value;
                }

                if (constants.TryGetValue(_name, out var constant))
                {
                    return constant;
                }

                throw new DataException(_name, CoreErrors.FormulaUnknownVariable, _name);
            }
        }

        private sealed class UnaryNode : Node
        {
            private readonly Node _operand;

            public UnaryNode(Node operand)
            {
                _operand = operand;
            }

            public override double Evaluate(IReadOnlyDictionary<string, double> variables, IReadOnlyDictionary<string, double> constants) =>
                -_operand.Evaluate(variables, constants);
        }

        private sealed class BinaryNode : Node
        {
            private readonly Node _left;
            private readonly Node _right;
            private readonly char _operation;

            public BinaryNode(Node left, char operation, Node right)
            {
                _left = left;
                _operation = operation;
                _right = right;
            }

            public override double Evaluate(IReadOnlyDictionary<string, double> variables, IReadOnlyDictionary<string, double> constants)
            {
                var left = _left.Evaluate(variables, constants);
                var right = _right.Evaluate(variables, constants);
                switch (_operation)
                {
                    case '+': return left + right;
                    case '-': return left - right;
                    case '*': return left * right;
                    case '/': return right == 0 ? 0 : left / right;
                    case '%': return right == 0 ? 0 : left % right;
                    default: return 0;
                }
            }
        }

        private sealed class FunctionNode : Node
        {
            private readonly string _name;
            private readonly List<Node> _arguments;

            public FunctionNode(string name, List<Node> arguments)
            {
                _name = name;
                _arguments = arguments;
            }

            public override double Evaluate(IReadOnlyDictionary<string, double> variables, IReadOnlyDictionary<string, double> constants)
            {
                var values = new double[_arguments.Count];
                for (var i = 0; i < _arguments.Count; i++)
                {
                    values[i] = _arguments[i].Evaluate(variables, constants);
                }

                switch (_name)
                {
                    case "min": return values[0] < values[1] ? values[0] : values[1];
                    case "max": return values[0] > values[1] ? values[0] : values[1];
                    case "abs": return Math.Abs(values[0]);
                    case "floor": return Math.Floor(values[0]);
                    case "ceil": return Math.Ceiling(values[0]);
                    case "round": return Math.Round(values[0], MidpointRounding.AwayFromZero);
                    case "clamp": return Math.Min(values[2], Math.Max(values[1], values[0]));
                    default: return 0;
                }
            }
        }

        private sealed class Parser
        {
            private readonly string _text;
            private readonly IReadOnlyCollection<string> _allowedNames;
            private readonly string _sourcePath;
            private int _position;
            private readonly HashSet<string> _usedNames = new HashSet<string>(StringComparer.Ordinal);

            /// <summary>Имена, которые встречаются в выражении.</summary>
            public IReadOnlyCollection<string> UsedNames => _usedNames;

            public Parser(string text, IReadOnlyCollection<string> allowedNames, string sourcePath)
            {
                _text = text;
                _allowedNames = allowedNames;
                _sourcePath = sourcePath;
            }

            public Node ParseAll()
            {
                var node = ParseAdditive();
                SkipSpaces();
                if (_position != _text.Length)
                {
                    throw new DataException(_sourcePath, CoreErrors.FormulaSyntaxError, _text);
                }

                return node;
            }

            private Node ParseAdditive()
            {
                var left = ParseMultiplicative();
                while (true)
                {
                    SkipSpaces();
                    if (Peek() != '+' && Peek() != '-')
                    {
                        return left;
                    }

                    var operation = _text[_position++];
                    left = new BinaryNode(left, operation, ParseMultiplicative());
                }
            }

            private Node ParseMultiplicative()
            {
                var left = ParseUnary();
                while (true)
                {
                    SkipSpaces();
                    if (Peek() != '*' && Peek() != '/' && Peek() != '%')
                    {
                        return left;
                    }

                    var operation = _text[_position++];
                    left = new BinaryNode(left, operation, ParseUnary());
                }
            }

            private Node ParseUnary()
            {
                SkipSpaces();
                if (Peek() == '-')
                {
                    _position++;
                    return new UnaryNode(ParseUnary());
                }

                if (Peek() == '+')
                {
                    _position++;
                    return ParseUnary();
                }

                return ParsePrimary();
            }

            private Node ParsePrimary()
            {
                SkipSpaces();
                if (_position >= _text.Length)
                {
                    throw new DataException(_sourcePath, CoreErrors.FormulaSyntaxError, _text);
                }

                var current = _text[_position];
                if (current == '(')
                {
                    _position++;
                    var inner = ParseAdditive();
                    Expect(')');
                    return inner;
                }

                if (char.IsDigit(current) || current == '.')
                {
                    return ParseNumber();
                }

                if (char.IsLetter(current) || current == '_')
                {
                    var name = ParseIdentifier();
                    SkipSpaces();
                    if (Peek() == '(')
                    {
                        return ParseCall(name);
                    }

                    _usedNames.Add(name);
                    return new NameNode(name);
                }

                throw new DataException(_sourcePath, CoreErrors.FormulaSyntaxError, _text);
            }

            private Node ParseCall(string name)
            {
                if (!IsKnownFunction(name))
                {
                    throw new DataException(_sourcePath, CoreErrors.FormulaUnknownFunction, name);
                }

                Expect('(');
                var arguments = new List<Node>();
                SkipSpaces();
                if (Peek() != ')')
                {
                    while (true)
                    {
                        arguments.Add(ParseAdditive());
                        SkipSpaces();
                        if (Peek() != ',')
                        {
                            break;
                        }

                        _position++;
                    }
                }

                Expect(')');
                var expected = name == "clamp" ? 3 : 1;
                var minArguments = name is "min" or "max" ? 2 : expected;
                if (arguments.Count != minArguments && !(name is "min" or "max" && arguments.Count >= 2))
                {
                    throw new DataException(_sourcePath, CoreErrors.FormulaSyntaxError, _text);
                }

                return new FunctionNode(name, arguments);
            }

            private static bool IsKnownFunction(string name) =>
                name is "min" or "max" or "abs" or "floor" or "ceil" or "round" or "clamp";

            private Node ParseNumber()
            {
                var start = _position;
                while (_position < _text.Length && (char.IsDigit(_text[_position]) || _text[_position] == '.'))
                {
                    _position++;
                }

                var text = _text.Substring(start, _position - start);
                if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                {
                    throw new DataException(_sourcePath, CoreErrors.FormulaSyntaxError, _text);
                }

                return new NumberNode(value);
            }

            private string ParseIdentifier()
            {
                var start = _position;
                while (_position < _text.Length && (char.IsLetterOrDigit(_text[_position]) || _text[_position] == '_'))
                {
                    _position++;
                }

                return _text.Substring(start, _position - start);
            }

            private void Expect(char symbol)
            {
                SkipSpaces();
                if (_position >= _text.Length || _text[_position] != symbol)
                {
                    throw new DataException(_sourcePath, CoreErrors.FormulaSyntaxError, _text);
                }

                _position++;
            }

            private char Peek() => _position < _text.Length ? _text[_position] : '\0';

            private void SkipSpaces()
            {
                while (_position < _text.Length && char.IsWhiteSpace(_text[_position]))
                {
                    _position++;
                }
            }
        }
    }
}
