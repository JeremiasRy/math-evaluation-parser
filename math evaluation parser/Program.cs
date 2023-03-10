using System.Text.RegularExpressions;


static string Eval(string expression)
{
    while (expression.IndexOf('(') != -1)
    {
        int lastOpen = expression.LastIndexOf('(');
        string subString = expression.Substring(lastOpen);
        subString = subString.Substring(0, subString.IndexOf(')') + 1);
        string result = EvaluateBrackets(subString);
        expression = expression.Replace(subString, result);
    }
    return "= 0";
}
static string EvaluateBrackets(string expression)
{
    Regex oper = new(@"\+|\-|\*|\/|&");
    var trimmed = WhiteSpaceRemover(expression.Substring(expression.IndexOf('(') + 1, expression.IndexOf(')') - 1));
    string firstIsNegative = "";
    if (trimmed[0] == '-')
    {
        firstIsNegative = trimmed.Substring(1);
    }
    var mathOperation = oper.Match(trimmed[0] == '-' ? firstIsNegative : trimmed);
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
        case "-":
            {
                return Substract(ParseString(left), ParseString(right)).ToString();
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
static double Substract(double left, double right) => left - right;
static double Add(double left, double right) => left + right;

Eval("(-(2 + 3)* (123 + 2222)) * 4 & 2");
Eval("abs(-(-1 + (2 * (4--3)))&2)");



    

