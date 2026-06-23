using KarpathySkillsBenchmark.Storage.Entities;
using Microsoft.EntityFrameworkCore;

namespace KarpathySkillsBenchmark.Storage;

public sealed class RunRepository
{
    private readonly BenchmarkDbContext _dbContext;

    public RunRepository(BenchmarkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task InitializeAsync(CancellationToken cancellationToken)
        => _dbContext.Database.MigrateAsync(cancellationToken);

    public async Task SaveRunAsync(BenchmarkRun run, CancellationToken cancellationToken)
    {
        _dbContext.BenchmarkRuns.Add(run);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<BenchmarkRun?> GetRunAsync(string runId, CancellationToken cancellationToken)
        => _dbContext.BenchmarkRuns
            .Include(run => run.TaskRuns)
            .ThenInclude(taskRun => taskRun.Metrics)
            .SingleOrDefaultAsync(run => run.RunId == runId, cancellationToken);

    public Task<List<BenchmarkRun>> GetHistoryAsync(int take, CancellationToken cancellationToken)
        => _dbContext.BenchmarkRuns
            .Include(run => run.TaskRuns)
            .OrderByDescending(run => run.Timestamp)
            .Take(take)
            .ToListAsync(cancellationToken);
}
