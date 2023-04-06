using System.Data.SqlTypes;
using System.Globalization;
using System.Text.RegularExpressions;
var ev = new Evaluate();

Console.WriteLine(ev.Eval("sin(cos(--12--2*1e+2))"));
public class Evaluate
{
    public string Eval(string expression)
    {
        Regex functions = new(@"[a-z]{2,4}");
        string[] allowedFunctions = new string[] { "log", "ln", "exp", "sqrt", "abs", "atan", "acos", "asin", "sinh", "cosh", "tanh", "tan", "sin", "cos" };
        var funcsInExpression = allowedFunctions.Where(func => expression.Contains(func));
        if (!InitialParsing(ref expression))
        {
            return "Error";
        }
        while (expression.IndexOf('(') != -1)
        {
            (int open, int amount) = (expression.LastIndexOf("("), expression[expression.LastIndexOf("(")..].IndexOf(")") + 1);
            var brackets = expression.Substring(open, amount);
            var result = "";
            try
            {
                result = CalculateExpression(brackets[1..^1]);
            }
            catch (Exception ex)
            {
                return $"Error {ex.Message}";
            }

            if (funcsInExpression.Any())
            {
                foreach (var func in funcsInExpression)
                {
                    int indexOfFunc = expression.IndexOf(func);
                    if (open - indexOfFunc >= 2 && open - indexOfFunc <= 4)
                    {
                        result = EvaluateFunction(func, result);
                        expression = expression.Replace(func, "");
                    }
                }
            }
            if (result == "Error")
            {
                return result;
            }
            expression = expression.Replace(brackets, result);
        }
        try
        {
            return CalculateExpression(expression);
        }
        catch (Exception ex)
        {
            return $"Error {ex.Message}";
        }

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
    static string CalculateExpression(string expression)
    {
        Regex oper = new(@"\+|\*|\/|&");
        var splitExpression = SplitExpression(expression);
        while (oper.IsMatch(string.Join("", splitExpression)))
        {
            if (Array.IndexOf(splitExpression, "&") != -1)
            {
                int index = Array.IndexOf(splitExpression, "&");
                var result = DoMath(splitExpression[index], splitExpression[index - 1], splitExpression[index + 1]);
                expression = expression.Replace(string.Join("", new string[] { splitExpression[index - 1], splitExpression[index], splitExpression[index + 1] }), result);
            }
            else if (Array.IndexOf(splitExpression, "*") != -1)
            {
                int index = Array.IndexOf(splitExpression, "*");
                var result = DoMath(splitExpression[index], splitExpression[index - 1], splitExpression[index + 1]);
                expression = expression.Replace(string.Join("", new string[] { splitExpression[index - 1], splitExpression[index], splitExpression[index + 1] }), result);
            }
            else if (Array.IndexOf(splitExpression, "/") != -1)
            {
                int index = Array.IndexOf(splitExpression, "/");
                var result = DoMath(splitExpression[index], splitExpression[index - 1], splitExpression[index + 1]);
                expression = expression.Replace(string.Join("", new string[] { splitExpression[index - 1], splitExpression[index], splitExpression[index + 1] }), result);
            }
            else if (Array.IndexOf(splitExpression, "+") != -1)
            {
                int index = Array.IndexOf(splitExpression, "+");
                var result = DoMath(splitExpression[index], splitExpression[index - 1], splitExpression[index + 1]);
                expression = expression.Replace(string.Join("", new string[] { splitExpression[index - 1], splitExpression[index], splitExpression[index + 1] }), result);
            }
            splitExpression = SplitExpression(expression);
        }
        Console.WriteLine(expression);
        return expression;
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
        Regex regex = new(@"\s*");
        return regex.Replace(str, "");
    }
    static bool InitialParsing(ref string expression)
    {
        Regex whiteSpaceInFunctionExpression = new(@"[a-z]+\s+[a-z]+");
        Regex operOrBracket = new(@"\+|\*|\/|&|\)");
        Regex scientific = new(@"\de\+?-?\d*");
        Regex substract = new("\\d+-\\d+");
        Regex multipleMinus = new(@"-{2,}");
        Regex multiplePlus = new(@"\+{2,}");

        if (expression.Where(ch => ch == '(').Count() != expression.Where(ch => ch == ')').Count())
        {
            return false;
        }

        if (whiteSpaceInFunctionExpression.IsMatch(expression))
        {
            return false;
        }

        expression = WhiteSpaceRemover(expression);

        if (scientific.IsMatch(expression))
        {
            var matches = scientific.Matches(expression);
            foreach (Match match in matches)
            {
                var scientificExpression = match.Value;
                string exponential = "";
                if (scientificExpression.Contains('-'))
                {
                    var numToParse = scientificExpression[(scientificExpression.IndexOf('-') + 1)..];
                    if (!int.TryParse(numToParse, out int amount))
                    {
                        return false;
                    }
                    exponential = $"0,{new('0', amount - 1)}1";

                }
                else
                {
                    string proxyExpression = scientificExpression;
                    if (scientificExpression.Contains('+'))
                    {
                        proxyExpression = scientificExpression.Remove(scientificExpression.IndexOf('+'), 1);
                    }
                    var numToParse = proxyExpression[(scientificExpression.IndexOf('e') + 1)..];
                    if (!int.TryParse(numToParse, out int amount))
                    {
                        return false;
                    }
                    exponential = $"{expression[match.Index]}{new('0', amount)}";
                }
                expression = expression.Replace(scientificExpression, exponential);
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
                expression = (match.Value.Length & 1) == 1 ? expression.Replace(match.Value, "-") : expression.Replace(match.Value, "");
            }
        }
        if (multiplePlus.IsMatch(expression))
        {
            return false;
        }
        for (int i = 0; i < expression.Length; i++)
        {
            if (i >= 1 && expression[i] == '(' && expression[i + 1] == '+')
            {
                return false;
            }
        }
        return true;
    }
    static double ParseString(string num) => double.Parse(num);
    static double Power(double left, double right) => Math.Pow(left, right);
    static double Multiply(double left, double right) => left * right;
    static double Divide(double left, double right)
    {
        if (left == 0 || right == 0)
        {
            throw new ArgumentException("Attempted to divide by zero");
        }
        return left / right;
    }
    static double Add(double left, double right) => left + right;
}





