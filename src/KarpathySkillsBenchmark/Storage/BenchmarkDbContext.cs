using KarpathySkillsBenchmark.Storage.Entities;
using Microsoft.EntityFrameworkCore;

namespace KarpathySkillsBenchmark.Storage;

public sealed class BenchmarkDbContext : DbContext
{
    public BenchmarkDbContext(DbContextOptions<BenchmarkDbContext> options)
        : base(options)
    {
    }

    public DbSet<BenchmarkRun> BenchmarkRuns => Set<BenchmarkRun>();

    public DbSet<TaskRunEntity> TaskRuns => Set<TaskRunEntity>();

    public DbSet<MetricEntity> Metrics => Set<MetricEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BenchmarkRun>().HasKey(run => run.RunId);
        modelBuilder.Entity<BenchmarkRun>()
            .HasMany(run => run.TaskRuns)
            .WithOne(taskRun => taskRun.BenchmarkRun)
            .HasForeignKey(taskRun => taskRun.BenchmarkRunId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TaskRunEntity>().HasKey(taskRun => taskRun.Id);
        modelBuilder.Entity<TaskRunEntity>()
            .HasMany(taskRun => taskRun.Metrics)
            .WithOne(metric => metric.TaskRun)
            .HasForeignKey(metric => metric.TaskRunEntityId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MetricEntity>().HasKey(metric => metric.Id);
    }
}
