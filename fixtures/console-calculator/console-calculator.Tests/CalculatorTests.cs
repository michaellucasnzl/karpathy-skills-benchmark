public sealed class CalculatorTests
{
    private readonly Calculator _calculator = new();

    [Fact]
    public void Add_Works() => Assert.Equal(5, _calculator.Add(2, 3));

    [Fact]
    public void Subtract_Works() => Assert.Equal(2, _calculator.Subtract(5, 3));

    [Fact]
    public void Multiply_Works() => Assert.Equal(15, _calculator.Multiply(5, 3));

    [Fact]
    public void Divide_Works() => Assert.Equal(2.5, _calculator.Divide(5, 2));

    [Fact]
    public void ParseAndMultiply_Works() => Assert.Equal(12, _calculator.ParseAndMultiply("3", "4"));
}
