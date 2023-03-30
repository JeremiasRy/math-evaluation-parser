using System.Linq;
using System.Text.RegularExpressions;


static string Eval(string expression)
{
    Regex oper = new(@"\+|\*|\/|&");
    Regex mathFuncs = new(@"[a-z]+\s*[a-z]*");
    Regex minusOper = new(@"-{2,}");
    Regex plusOper = new(@"\+{2,}");
    
    MatchCollection functions = mathFuncs.Matches(expression);
    foreach (Match match in functions)
    {
        if (match.Value.Contains(' '))
        {
            return "Error";
        };
    }
    expression = WhiteSpaceRemover(expression);
    MatchCollection minusOperators = minusOper.Matches(expression);
    foreach (Match match in minusOperators)
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
    //Evaluate brackets
    while (expression.IndexOf('(') != -1)
    {
        int lastOpen = expression.LastIndexOf('(');
        string subString = expression[lastOpen..];
        subString = subString[..(subString.IndexOf(')') + 1)];
        string result = EvaluateBrackets(subString[1..^1]);
        expression = expression.Replace(subString, result);
    }
    var mathOperations = oper.Matches(expression);
    string[] mathValues = oper.Split(expression);

    return "= 0";
}
static string EvaluateBrackets(string expression)
{
    Regex oper = new(@"\+|\*|\/|&|-");
    var operators = oper.Matches(expression);
    string[]? numbers = new string[2];
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

Eval("abs(-(-1 + (2 * (4--3)))&2)");
Eval("  ( ---(   2 + 3)    * (123 - 2222)) * 4 & 2");




