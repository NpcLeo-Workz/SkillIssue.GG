using Microsoft.EntityFrameworkCore;
using SkillIssue.GG.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace SkillIssue.GG.Infrastructure.IntegrationTests;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container =
        new PostgreSqlBuilder("postgres:18")
            .WithDatabase("skillissuegg_tests")
            .WithUsername("skillissuegg_test")
            .WithPassword("skillissuegg_test")
            .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public SkillIssueDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SkillIssueDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new SkillIssueDbContext(options);
    }
}
