using System.Data.SqlTypes;
using System.Text.RegularExpressions;

static string Eval(string expression)
{
    Regex functions = new(@"[a-z]{2,4}");
    if (InitialParsing(expression) == "Error")
    {
        return "Error";
    }
    return "";
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
static string InitialParsing(string expression)
{
    Regex whiteSpaceInFunctionExpression = new(@"[a-z]+\s+[a-z]+");
    Regex operOrBracket = new(@"\*|\/|&|\)");
    Regex scientific = new("e");
    Regex substract = new("\\d+-\\d+");
    Regex multipleMinus = new(@"-{2,}");
    Regex multiplePlus = new(@"\+{2,}");

    if (expression.Where(ch => ch == '(').Count() != expression.Where(ch => ch == ')').Count())
    {
        return "Error";
    }
    if (whiteSpaceInFunctionExpression.IsMatch(expression))
    {
        return "Error";
    }
    if (scientific.IsMatch(expression))
    {
        var matches = scientific.Matches(expression);
        foreach (Match match in matches)
        {
            var scientificExpression = expression.Substring(match.Index, operOrBracket.Match(expression[match.Index..]).Index);
            string exponential = "";
            if (scientificExpression.Contains('-'))
            {
                var toParseInt = scientificExpression[(scientificExpression.IndexOf('-') + 1)..];
                int amount = int.Parse(toParseInt);
                exponential = $"{new('0', amount - 1)}1";
                exponential = exponential.Insert(0, "0,");
            } else
            {
                int containsPlusOperator = scientificExpression.IndexOf('+');
                if (containsPlusOperator != -1)
                {
                    scientificExpression = scientificExpression.Remove(containsPlusOperator, 1);
                }
                var toParseInt = scientificExpression[1..];
                int amount = int.Parse(toParseInt);
                exponential = new('0', amount);
                exponential = exponential.Insert(0, $"{expression[match.Index - 1]}");
            }
        }
    }
    if (substract.IsMatch(expression))
    {
        var matches = substract.Matches(expression);
        foreach (Match match in matches)
        {
            var newExpression = match.Value.Insert(match.Value.IndexOf("-"), "+");
            expression = expression.Replace(match.Value, newExpression);
        }
    }
    if (multipleMinus.IsMatch(expression)) 
    {
        var matches = multipleMinus.Matches(expression);
        foreach (Match match in matches)
        {
            expression = (match.Value.Length & 1) == 1 ? expression.Replace(match.Value, "-") : expression.Replace(match.Value, "+"); 
        }
    }
    return "";

}
static double ParseString(string num) => double.Parse(num);
static double Power(double left, double right) => Math.Pow(left, right);
static double Multiply(double left, double right) => left * right;
static double Divide(double left, double right) => left / right;
static double Add(double left, double right) => left + right;
Console.WriteLine(Eval("(-14--2*7e+3)&2"));



