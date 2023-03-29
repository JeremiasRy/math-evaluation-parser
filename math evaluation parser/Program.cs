using System.Linq;
using System.Text.RegularExpressions;


static string Eval(string expression)
{
    Regex oper = new(@"\+|\*|\/|&");
    Regex mathFuncs = new(@"[a-z]+\s*[a-z]*");
    Regex minusOper = new(@"-{2,}");
    
    MatchCollection functions = mathFuncs.Matches(expression);
    foreach (Match match in functions)
    {
        if (match.Value.Contains(' '))
        {
            return "Error";
        };
    }
    MatchCollection minusOperators = minusOper.Matches(expression);
    foreach (Match match in minusOperators)
    {
        if ((match.Value.Length & 1) == 1)
        {
            expression = expression.Replace(match.Value, "-");
        } else
        {
            expression = expression.Replace(match.Value, "");
        }
    }

    expression = WhiteSpaceRemover(expression);

    while (expression.IndexOf('(') != -1)
    {
        int lastOpen = expression.LastIndexOf('(');
        string subString = expression[lastOpen..];
        subString = subString[..(subString.IndexOf(')') + 1)];
        string result = EvaluateBrackets(subString);
        expression = expression.Replace(subString, result);
    }
    var mathOperations = oper.Matches(expression);
    string[] mathValues = oper.Split(expression);

    return "= 0";
}
static string EvaluateBrackets(string expression)
{
    Regex oper = new(@"\+|\*|\/|&");
    var trimmed = WhiteSpaceRemover(expression.Substring(expression.IndexOf('(') + 1, expression.IndexOf(')') - 1));
    var mathOperation = oper.Match(trimmed);
    var numbers = trimmed.Split(mathOperation.Value);
    
    return DoMath(mathOperation.Value, numbers[0], numbers[1]);
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

Eval("  ( ---(   2 + 3)    * (123 + 2222)) * 4 & 2");
Eval("abs(-(-1 + (2 * (4--3)))&2)");



    

