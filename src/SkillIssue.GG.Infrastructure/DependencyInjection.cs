using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkillIssue.GG.Infrastructure.Persistence;

namespace SkillIssue.GG.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'PostgreSQL' is not configured.");
        }

        services.AddDbContext<SkillIssueDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}
