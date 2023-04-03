using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Schema;

static string Eval(string expression)
{
    string[] allowedFuncs = new string[] { "log", "ln", "exp", "sqrt", "abs", "atan", "acos", "asin", "sinh", "cosh", "tanh", "tan", "sin", "cos" };
    Regex oper = new(@"\+|\*|\/|&");
    Regex mathFuncs = new(@"([a-z]+[a-z]+[a-z]?[a-z]?)");
    Regex minusOper = new(@"-{2,}");
    Regex plusOper = new(@"\+{2,}");
    
    MatchCollection functions = mathFuncs.Matches(expression);
    foreach (Match match in functions.Cast<Match>())
    {
        if (!allowedFuncs.Any(allowed => allowed == match.Value))
        {
            return "Error";
        }
    }
    expression = WhiteSpaceRemover(expression);
    MatchCollection minusOperators = minusOper.Matches(expression);

    foreach (Match match in minusOperators.Cast<Match>())
    {
        if ((match.Value.Length & 1) == 1)
        {
            expression = expression.Replace(match.Value, "-");
        } else
        {
            expression = expression.Replace(match.Value, "+");
        }
    }

    if (plusOper.IsMatch(expression))
    {
        return "Error";
    }

    if (expression.Where(ch => ch == '(').Count() != expression.Where(ch => ch == ')').Count())
    {
        return "Error";
    }
    //Evaluate brackets
    while (expression.IndexOf('(') != -1)
    {
        string subString = expression[expression.LastIndexOf('(')..];
        subString = subString[..(subString.IndexOf(')') + 1)];
        string result = EvaluateBrackets(subString[1..^1]);
        expression = expression.Replace(subString, result);
    }
    string[] mathValues = oper.Split(expression);

    return "= 0";
}
static string EvaluateFunction(string function, string value)
{   
    switch (function)
    {
        case "sin":
            {
                return Math.Sin(double.Parse(value)).ToString();
            } 
        case "cos":
            {
                return Math.Cos(double.Parse(value)).ToString();
            }
    }
    return "";
}
static string EvaluateBrackets(string expression)
{
    string[] allowedFuncs = new string[] { "log", "ln", "exp", "sqrt", "abs", "atan", "acos", "asin", "sinh", "cosh", "tanh", "tan", "sin", "cos" };
    var functionsInExpression = allowedFuncs.Where(func => expression.Contains(func));
    Regex oper = new(@"\+|\*|\/|&|-");
    var operators = oper.Matches(expression);
    if (functionsInExpression.Any()) 
    {
        foreach (var func in functionsInExpression)
        {
            var startOfFunc = expression.IndexOf(func);
            string function = expression.Substring(startOfFunc, func.Length);
            string value = "";
            if (operators.Count == 3)
            {
                var endOfFunc = expression.IndexOf(function[^1]) + 1;
                var oneBeforeOperator = endOfFunc < operators[1].Index ? operators[1].Index - 1 : expression.Length;
                value = expression.Substring(endOfFunc, oneBeforeOperator - endOfFunc == 0 ? 1 : oneBeforeOperator - endOfFunc);
            } else if (operators.Count == 2) 
            {
                var notDashOperator = operators.Where(oper => oper.Value != "-").First();
                var endOfFunc = expression.IndexOf(function[^1]) + 1;
                var oneBeforeOperator = endOfFunc < notDashOperator.Index ? notDashOperator.Index - 1 : expression.Length;
                value = expression.Substring(endOfFunc, oneBeforeOperator - endOfFunc == 0 ? 1 : oneBeforeOperator - endOfFunc);
            } else
            {
                var endOfFunc = expression.IndexOf(function[^1]) + 1;
                var oneBeforeOperator = startOfFunc < operators.First().Index ? operators.First().Index - 1 : expression.Length;
                value = expression.Substring(endOfFunc, oneBeforeOperator - endOfFunc == 0 ? 1 : oneBeforeOperator - endOfFunc);
            }
            expression = expression.Replace(function + value, EvaluateFunction(function, value));
        }
    }
    string[]? numbers = new string[2];
    operators = oper.Matches(expression);

    if (operators.Count == 3)
    {
        numbers = expression.Split(operators[1].Value);
        return DoMath(operators[1].Value, numbers[0], numbers[1]);
    }
    if (operators.Count == 2)
    {
        numbers = expression[1..].ToString().Split(operators.Last().Value);
        
        if (numbers is null)
        {
            throw new Exception("Failed miserably");
        }

        numbers[0] = numbers[0].Insert(0, operators.First().Value);

        if (operators.Last().Value == "-")
        {
            numbers[1] = numbers[1].Insert(0, operators.Last().Value);
            return DoMath("+", numbers[0], numbers[1]);
        }
        
        return DoMath(operators.Last().Value , numbers[0], numbers[1]);
    }

    if (operators.Count == 1 && operators.First().Value == "-") 
    {
        numbers = expression.Split(operators.First().Value);
        numbers[1] = numbers[1].Insert(0, operators.First().Value);
        return DoMath("+", numbers[0], numbers[1]);
    }

    numbers = expression.Split(operators.First().Value);
    return DoMath(operators.First().Value, numbers[0], numbers[1]);
}
static string DoMath(string oper, string left, string right)
{
    switch (oper)
    {
        case "+":
            {
                return Add(ParseString(left), ParseString(right)).ToString();
            }
        case "*":
            {
                return Multiply(ParseString(left), ParseString(right)).ToString();
            }
        case "/":
            {
                return Divide(ParseString(left), ParseString(right)).ToString();
            }
        case "&":
            {
                return Power(ParseString(left), ParseString(right)).ToString();
            }
            default:
            {
                throw new Exception("Operator was invalid");
            }
    }
}
static string WhiteSpaceRemover(string str)
{
    Regex regex = new (@"\s*");
    return regex.Replace(str, "");
}

static double ParseString(string num) => double.Parse(num);
static double Power(double left, double right) => Math.Pow(left, right);
static double Multiply(double left, double right) => left * right;
static double Divide(double left, double right) => left / right;
static double Add(double left, double right) => left + right;

Eval("sqrt (sin(2 + 3)*cos (1-2)) * 4 & 2");
Eval("  ( ---(   2 + 3)    * (123 - 2222)) * 4 & 2");




