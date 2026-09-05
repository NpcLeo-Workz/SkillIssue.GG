using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkillIssue.GG.Infrastructure.Persistence;
using SkillIssue.GG.Infrastructure.Riot.Configuration;
using Microsoft.Extensions.Options;
using SkillIssue.GG.Infrastructure.Riot.Http;

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

        services.AddOptions<RiotApiOptions>()
            .Bind(configuration.GetSection(RiotApiOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ApiKey),
                "Riot API key is not configured.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.PlatformRoute),
                "Riot platform route is not configured.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.RegionalRoute),
                "Riot regional route is not configured.")
            .ValidateOnStart();

        services.AddHttpClient<RiotApiClient>((serviceProvider, httpClient) =>
        {
            var riotApiOptions = serviceProvider
                .GetRequiredService<IOptions<RiotApiOptions>>()
                .Value;

            httpClient.DefaultRequestHeaders.Add(
                "X-Riot-Token",
                riotApiOptions.ApiKey);
        });

        return services;
    }
}
