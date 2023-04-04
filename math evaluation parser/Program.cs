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
    Regex ifThereIsASubstract = new(@"[\d]-[\d]");
    
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

    if (ifThereIsASubstract.IsMatch(expression))
    {
        var matchingExpression = ifThereIsASubstract.Match(expression);
        var newExpression = matchingExpression.Value.Insert(matchingExpression.Value.IndexOf("-"), "+");
        expression = expression.Replace(matchingExpression.Value, newExpression);
    }

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
    var functionsInCleanExpression = allowedFuncs.Where(func => expression.Contains(func));
    if (functions.Any())
    {
        expression = ParseFunction(expression, functionsInCleanExpression, oper.Matches(expression));
    }
    if (expression.Contains("Error"))
    {
        return "Error";
    }
    var splitExpression = SplitExpression(expression);
    while (splitExpression.Any(exp => oper.Match(exp).Length > 0))
    {
        if (Array.IndexOf(splitExpression,"&") != -1)
        {
            var indexOfOperation = Array.IndexOf(splitExpression,"&");
            var result = DoMath(splitExpression[indexOfOperation], splitExpression[indexOfOperation - 1], splitExpression[indexOfOperation + 1]);
            var stringExpression = string.Join("", new string[] { splitExpression[indexOfOperation - 1], splitExpression[indexOfOperation], splitExpression[indexOfOperation + 1] });
            expression = expression.Replace(stringExpression, result);
            splitExpression = SplitExpression(expression);
        } else if (Array.IndexOf(splitExpression, "*") != -1)
        {
            var indexOfOperation = Array.IndexOf(splitExpression, "*");
            var result = DoMath(splitExpression[indexOfOperation], splitExpression[indexOfOperation - 1], splitExpression[indexOfOperation + 1]);
            var stringExpression = string.Join("", new string[] { splitExpression[indexOfOperation - 1], splitExpression[indexOfOperation], splitExpression[indexOfOperation + 1] });
            expression = expression.Replace(stringExpression, result);
            splitExpression = SplitExpression(expression);
        } else if (Array.IndexOf(splitExpression, "/") != -1)
        {
            var indexOfOperation = Array.IndexOf(splitExpression, "/");
            if (splitExpression[indexOfOperation - 1] == "0" || splitExpression[indexOfOperation + 1] == "0")
            {
                return "Error";
            }
            var result = DoMath(splitExpression[indexOfOperation], splitExpression[indexOfOperation - 1], splitExpression[indexOfOperation + 1]);
            var stringExpression = string.Join("", new string[] { splitExpression[indexOfOperation - 1], splitExpression[indexOfOperation], splitExpression[indexOfOperation + 1] });
            expression = expression.Replace(stringExpression, result);
            splitExpression = SplitExpression(expression);
        } else if (Array.IndexOf(splitExpression, "+") != -1)
        {
            var indexOfOperation = Array.IndexOf(splitExpression, "+");
            var result = DoMath(splitExpression[indexOfOperation], splitExpression[indexOfOperation - 1], splitExpression[indexOfOperation + 1]);
            var stringExpression = string.Join("", new string[] { splitExpression[indexOfOperation - 1], splitExpression[indexOfOperation], splitExpression[indexOfOperation + 1] });
            expression = expression.Replace(stringExpression, result);
            splitExpression = SplitExpression(expression);
        }
    }
    return expression;
}
static string[] SplitExpression(string expression)
{
    Regex oper = new(@"\+|\*|\/|&");
    var mathValues = oper.Split(expression);
    var operators = oper.Matches(expression);
    string[] splitExpression = new string[mathValues.Length + operators.Count];
    int countMathValues = 0;
    int countOperatorValues = 0;

    while (countMathValues + countOperatorValues < splitExpression.Length)
    {
        splitExpression[countMathValues + countOperatorValues] = mathValues[countMathValues++];
        if (countOperatorValues != operators.Count)
        {
            splitExpression[countMathValues + countOperatorValues] = operators[countOperatorValues++].Value;
        }
    }
    return splitExpression;
}
static string ParseFunction(string expression, IEnumerable<string> funcs, MatchCollection operators)
{
    foreach (var func in funcs)
    {
        var startOfFunc = expression.IndexOf(func);
        string function = expression.Substring(startOfFunc, func.Length);
        string value = "";
        if (operators.Count == 3)
        {
            var endOfFunc = expression.IndexOf(function[^1]) + 1;
            var oneBeforeOperator = endOfFunc < operators[1].Index ? operators[1].Index - 1 : expression.Length;
            value = expression.Substring(endOfFunc, oneBeforeOperator - endOfFunc == 0 ? 1 : oneBeforeOperator - endOfFunc);
        }
        else if (operators.Count == 2)
        {
            var notDashOperator = operators.Where(oper => oper.Value != "-").First();
            var endOfFunc = expression.IndexOf(function[^1]) + 1;
            var oneBeforeOperator = endOfFunc < notDashOperator.Index ? notDashOperator.Index - 1 : expression.Length;
            value = expression.Substring(endOfFunc, oneBeforeOperator - endOfFunc == 0 ? 1 : oneBeforeOperator - endOfFunc);
        }
        else if (operators.Count == 1)
        {
            var endOfFunc = expression.IndexOf(function[^1]) + 1;
            var oneBeforeOperator = startOfFunc < operators.First().Index ? operators.First().Index - 1 : expression.Length;
            value = expression.Substring(endOfFunc, oneBeforeOperator - endOfFunc == 0 ? 1 : oneBeforeOperator - endOfFunc + 1);
        } else
        {
            value = expression[(expression.IndexOf(function[^1]) + 1)..];
        }
        expression = expression.Replace(function + value, EvaluateFunction(function, value));
    }
    return expression;
}
static string EvaluateFunction(string function, string value)
{   
    switch (function)
    {
        case "log":
            {
                return Math.Log10(double.Parse(value)).ToString();
            }
        case "ln":
            {
                return Math.Log(double.Parse(value)).ToString();
            }
        case "exp":
            {
                return Math.Exp(double.Parse(value)).ToString();
            }
        case "sqrt":
            {
                if (double.Parse(value) < 0) 
                {
                    return "Error";
                }
                return Math.Sqrt(double.Parse(value)).ToString();
            }
        case "abs":
            {
                return Math.Abs(double.Parse(value)).ToString();
            }
        case "atan":
            {
                return Math.Atanh(double.Parse(value)).ToString();
            }
        case "acos":
            {
                return Math.Acos(double.Parse(value)).ToString();
            }
        case "asin":
            {
                return Math.Asin(double.Parse(value)).ToString();
            }
        case "sinh":
            {
                return Math.Sinh(double.Parse(value)).ToString();
            }
        case "cosh":
            {
                return Math.Cosh(double.Parse(value)).ToString();
            }
        case "tanh":
            {
                return Math.Tanh(double.Parse(value)).ToString();
            }
        case "tan":
            {
                return Math.Tan(double.Parse(value)).ToString();
            }
        case "sin":
            {
                return Math.Sin(double.Parse(value)).ToString();
            } 
        case "cos":
            {
                return Math.Cos(double.Parse(value)).ToString();
            } 
    }
    return "Error";
}
static string EvaluateBrackets(string expression)
{
    string[] allowedFuncs = new string[] { "log", "ln", "exp", "sqrt", "abs", "atan", "acos", "asin", "sinh", "cosh", "tanh", "tan", "sin", "cos" };
    var functionsInExpression = allowedFuncs.Where(func => expression.Contains(func));
    Regex oper = new(@"\+|\*|\/|&|-");
    var operators = oper.Matches(expression);
    if (functionsInExpression.Any()) 
    {
        expression = ParseFunction(expression, functionsInExpression, operators);
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
        if (numbers.Any(num => num == ""))
        {
            return expression;
        }
        numbers[1] = numbers[1].Insert(0, operators.First().Value);
        return DoMath("+", numbers[0], numbers[1]);
    }

    numbers = expression.Split(operators.First().Value);
    if (numbers.Any(num => num == ""))
    {
        return expression;
    }
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
Console.WriteLine(Eval("(19--2*1e-3)&2"));



