using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KarpathySkillsBenchmark.Storage;

public sealed class BenchmarkDbContextFactory : IDesignTimeDbContextFactory<BenchmarkDbContext>
{
    public BenchmarkDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BenchmarkDbContext>();
        optionsBuilder.UseSqlite("Data Source=results/benchmark.db");
        return new BenchmarkDbContext(optionsBuilder.Options);
    }
}
