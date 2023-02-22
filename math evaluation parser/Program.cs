using System.Text.RegularExpressions;


static string Eval(string expression)
{
    while (expression.IndexOf('(') != -1)
    {
        int lastOpen = expression.LastIndexOf('(');
        string subString = expression.Substring(lastOpen);
        subString = subString.Substring(0, subString.IndexOf(')') + 1);
        string result = EvaluateBrackets(subString);
    }
    return "= 0";
}
static string EvaluateBrackets(string expression)
{
    Regex oper = new(@"\+|\-|\*|\\|&");
    var trimmed = WhiteSpaceRemover(expression.Substring(expression.IndexOf('(') + 1, expression.IndexOf(')') - 1));
    var firstNum = expression.IndexOf('(');
    return trimmed;
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

Eval("(-(2 + 3)* (1 + 2)) * 4 & 2");
Eval("abs(-(-1 + (2 * (4--3)))&2)");



    

