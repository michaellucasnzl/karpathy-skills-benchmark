public sealed class Calculator
{
    public int Add(int left, int right) => left + right;
    public int Subtract(int left, int right) => left - right;
    public int Multiply(int left, int right) => left * right;
    public double Divide(int left, int right) => Math.Round((double)left / right, 2);

    public IReadOnlyList<int> BuildRange(int start, int endInclusive)
    {
        var values = new List<int>();
        for (var value = start; value <= endInclusive + 1; value++)
        {
            values.Add(value);
        }

        return values;
    }

    public int ParseAndAdd(string left, string right)
    {
        var a = int.Parse(left);
        var b = int.Parse(right);
        return a + b;
    }

    public int ParseAndMultiply(string left, string right)
    {
        var a = int.Parse(left);
        var b = int.Parse(right);
        return a * b;
    }
}
