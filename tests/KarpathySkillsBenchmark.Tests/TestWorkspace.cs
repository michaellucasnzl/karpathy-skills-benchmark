namespace KarpathySkillsBenchmark.Tests;

internal static class TestWorkspace
{
    public static string Create(string name)
    {
        var root = Path.Combine(Directory.GetCurrentDirectory(), ".testws", Shorten(name) + "-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(root);
        return root;
    }

    private static string Shorten(string value)
        => value.Length <= 18 ? value : value[..18];
}
