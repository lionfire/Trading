namespace LionFire.Trading.Automation.Optimization.Scoring.Formula;

/// <summary>
/// Token types for the formula lexer.
/// </summary>
public enum TokenType
{
    Number,
    Identifier,
    LeftParen,
    RightParen,
    Comma,
    Plus,
    Minus,
    Multiply,
    Divide,
    GreaterEqual,
    LessEqual,
    Greater,
    Less,
    Equal,
    NotEqual,
    And,
    Or,
    EndOfInput
}

/// <summary>
/// Represents a token in the formula.
/// </summary>
public record Token(TokenType Type, string Value, int Position);

/// <summary>
/// Tokenizes formula strings into a sequence of tokens.
/// </summary>
public class Tokenizer
{
    private readonly string _input;
    private int _position;

    public Tokenizer(string input)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _position = 0;
    }

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();

        while (_position < _input.Length)
        {
            SkipWhitespace();
            if (_position >= _input.Length) break;

            var token = ReadNextToken();
            tokens.Add(token);
        }

        tokens.Add(new Token(TokenType.EndOfInput, "", _position));
        return tokens;
    }

    private void SkipWhitespace()
    {
        while (_position < _input.Length && char.IsWhiteSpace(_input[_position]))
            _position++;
    }

    private Token ReadNextToken()
    {
        var startPos = _position;
        var c = _input[_position];

        // Numbers
        if (char.IsDigit(c) || (c == '.' && _position + 1 < _input.Length && char.IsDigit(_input[_position + 1])))
        {
            return ReadNumber(startPos);
        }

        // Identifiers (function names, metric names)
        if (char.IsLetter(c) || c == '_')
        {
            return ReadIdentifier(startPos);
        }

        // Two-character operators
        if (_position + 1 < _input.Length)
        {
            var twoChar = _input.Substring(_position, 2);
            switch (twoChar)
            {
                case ">=":
                    _position += 2;
                    return new Token(TokenType.GreaterEqual, ">=", startPos);
                case "<=":
                    _position += 2;
                    return new Token(TokenType.LessEqual, "<=", startPos);
                case "==":
                    _position += 2;
                    return new Token(TokenType.Equal, "==", startPos);
                case "!=":
                    _position += 2;
                    return new Token(TokenType.NotEqual, "!=", startPos);
                case "&&":
                    _position += 2;
                    return new Token(TokenType.And, "&&", startPos);
                case "||":
                    _position += 2;
                    return new Token(TokenType.Or, "||", startPos);
            }
        }

        // Single-character operators
        _position++;
        return c switch
        {
            '(' => new Token(TokenType.LeftParen, "(", startPos),
            ')' => new Token(TokenType.RightParen, ")", startPos),
            ',' => new Token(TokenType.Comma, ",", startPos),
            '+' => new Token(TokenType.Plus, "+", startPos),
            '-' => new Token(TokenType.Minus, "-", startPos),
            '*' => new Token(TokenType.Multiply, "*", startPos),
            '/' => new Token(TokenType.Divide, "/", startPos),
            '>' => new Token(TokenType.Greater, ">", startPos),
            '<' => new Token(TokenType.Less, "<", startPos),
            _ => throw new FormulaException($"Unexpected character '{c}' at position {startPos}")
        };
    }

    private Token ReadNumber(int startPos)
    {
        var hasDecimal = false;
        while (_position < _input.Length)
        {
            var c = _input[_position];
            if (char.IsDigit(c))
            {
                _position++;
            }
            else if (c == '.' && !hasDecimal)
            {
                hasDecimal = true;
                _position++;
            }
            else
            {
                break;
            }
        }
        return new Token(TokenType.Number, _input[startPos.._position], startPos);
    }

    private Token ReadIdentifier(int startPos)
    {
        while (_position < _input.Length && (char.IsLetterOrDigit(_input[_position]) || _input[_position] == '_'))
        {
            _position++;
        }
        return new Token(TokenType.Identifier, _input[startPos.._position], startPos);
    }
}

/// <summary>
/// Exception thrown for formula parsing or evaluation errors.
/// </summary>
public class FormulaException : Exception
{
    public FormulaException(string message) : base(message) { }
    public FormulaException(string message, Exception inner) : base(message, inner) { }
}
