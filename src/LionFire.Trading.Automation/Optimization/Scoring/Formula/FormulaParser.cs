namespace LionFire.Trading.Automation.Optimization.Scoring.Formula;

/// <summary>
/// Parses formula strings into AST nodes.
/// </summary>
/// <remarks>
/// Grammar:
///   formula     = expression
///   expression  = term (('+' | '-') term)*
///   term        = factor (('*' | '/') factor)*
///   factor      = number | identifier | function_call | '(' expression ')'
///   function    = identifier '(' args ')'
///   args        = (condition | expression) (',' (condition | expression))*
///   condition   = comparison (('&&' | '||') comparison)*
///   comparison  = metric_or_value ('<' | '>' | '<=' | '>=' | '==' | '!=') metric_or_value
/// </remarks>
public class FormulaParser
{
    private List<Token> _tokens = null!;
    private int _position;

    public AstNode Parse(string formula)
    {
        var tokenizer = new Tokenizer(formula);
        _tokens = tokenizer.Tokenize();
        _position = 0;

        var result = ParseExpression();

        if (Current.Type != TokenType.EndOfInput)
        {
            throw new FormulaException($"Unexpected token '{Current.Value}' at position {Current.Position}");
        }

        return result;
    }

    private Token Current => _tokens[_position];

    private Token Consume(TokenType expected)
    {
        if (Current.Type != expected)
        {
            throw new FormulaException($"Expected {expected} but got {Current.Type} at position {Current.Position}");
        }
        return _tokens[_position++];
    }

    private bool Match(TokenType type)
    {
        if (Current.Type == type)
        {
            _position++;
            return true;
        }
        return false;
    }

    private AstNode ParseExpression()
    {
        var left = ParseTerm();

        while (Current.Type is TokenType.Plus or TokenType.Minus)
        {
            var op = Current.Type;
            _position++;
            var right = ParseTerm();
            left = new BinaryOpNode(left, op, right);
        }

        return left;
    }

    private AstNode ParseTerm()
    {
        var left = ParseFactor();

        while (Current.Type is TokenType.Multiply or TokenType.Divide)
        {
            var op = Current.Type;
            _position++;
            var right = ParseFactor();
            left = new BinaryOpNode(left, op, right);
        }

        return left;
    }

    private AstNode ParseFactor()
    {
        // Number
        if (Current.Type == TokenType.Number)
        {
            var value = double.Parse(Current.Value);
            _position++;
            return new NumberNode(value);
        }

        // Negative number
        if (Current.Type == TokenType.Minus)
        {
            _position++;
            var factor = ParseFactor();
            if (factor is NumberNode num)
            {
                return new NumberNode(-num.Value);
            }
            return new BinaryOpNode(new NumberNode(0), TokenType.Minus, factor);
        }

        // Identifier (function call or metric)
        if (Current.Type == TokenType.Identifier)
        {
            var name = Current.Value;
            _position++;

            // Function call
            if (Current.Type == TokenType.LeftParen)
            {
                _position++; // consume '('
                var args = ParseArguments();
                Consume(TokenType.RightParen);
                return new FunctionNode(name, args);
            }

            // Metric reference
            return new MetricNode(name);
        }

        // Parenthesized expression
        if (Current.Type == TokenType.LeftParen)
        {
            _position++;
            var expr = ParseExpression();
            Consume(TokenType.RightParen);
            return expr;
        }

        throw new FormulaException($"Unexpected token '{Current.Value}' at position {Current.Position}");
    }

    private List<AstNode> ParseArguments()
    {
        var args = new List<AstNode>();

        if (Current.Type == TokenType.RightParen)
        {
            return args; // Empty argument list
        }

        // First argument - could be condition or expression
        args.Add(ParseConditionOrExpression());

        while (Current.Type == TokenType.Comma)
        {
            _position++;
            args.Add(ParseConditionOrExpression());
        }

        return args;
    }

    private AstNode ParseConditionOrExpression()
    {
        // Try to parse as condition first
        var left = ParsePrimaryForCondition();

        // Check for comparison operator
        if (IsComparisonOperator(Current.Type))
        {
            var op = Current.Type;
            _position++;
            var right = ParsePrimaryForCondition();

            var comparison = new ComparisonNode(left, op, right);

            // Check for logical operators
            while (Current.Type is TokenType.And or TokenType.Or)
            {
                var logicalOp = Current.Type;
                _position++;
                var nextComparison = ParseComparisonOnly();
                return new LogicalNode(comparison, logicalOp, nextComparison);
            }

            return comparison;
        }

        // Not a condition, must be an expression - continue parsing
        return ParseExpressionContinuation(left);
    }

    private AstNode ParsePrimaryForCondition()
    {
        // Handle negative numbers
        if (Current.Type == TokenType.Minus)
        {
            _position++;
            var inner = ParsePrimaryForCondition();
            if (inner is NumberNode num)
            {
                return new NumberNode(-num.Value);
            }
            return new BinaryOpNode(new NumberNode(0), TokenType.Minus, inner);
        }

        if (Current.Type == TokenType.Number)
        {
            var value = double.Parse(Current.Value);
            _position++;
            return new NumberNode(value);
        }

        if (Current.Type == TokenType.Identifier)
        {
            var name = Current.Value;
            _position++;

            // Could be function call
            if (Current.Type == TokenType.LeftParen)
            {
                _position++;
                var args = ParseArguments();
                Consume(TokenType.RightParen);
                return new FunctionNode(name, args);
            }

            return new MetricNode(name);
        }

        if (Current.Type == TokenType.LeftParen)
        {
            _position++;
            var expr = ParseConditionOrExpression();
            Consume(TokenType.RightParen);
            return expr;
        }

        throw new FormulaException($"Unexpected token '{Current.Value}' at position {Current.Position}");
    }

    private AstNode ParseComparisonOnly()
    {
        var left = ParsePrimaryForCondition();

        if (!IsComparisonOperator(Current.Type))
        {
            throw new FormulaException($"Expected comparison operator at position {Current.Position}");
        }

        var op = Current.Type;
        _position++;
        var right = ParsePrimaryForCondition();

        return new ComparisonNode(left, op, right);
    }

    private AstNode ParseExpressionContinuation(AstNode left)
    {
        // Handle term operators
        while (Current.Type is TokenType.Multiply or TokenType.Divide)
        {
            var op = Current.Type;
            _position++;
            var right = ParseFactor();
            left = new BinaryOpNode(left, op, right);
        }

        // Handle expression operators
        while (Current.Type is TokenType.Plus or TokenType.Minus)
        {
            var op = Current.Type;
            _position++;
            var right = ParseTerm();
            left = new BinaryOpNode(left, op, right);
        }

        return left;
    }

    private static bool IsComparisonOperator(TokenType type)
    {
        return type is TokenType.GreaterEqual or TokenType.LessEqual
            or TokenType.Greater or TokenType.Less
            or TokenType.Equal or TokenType.NotEqual;
    }
}
