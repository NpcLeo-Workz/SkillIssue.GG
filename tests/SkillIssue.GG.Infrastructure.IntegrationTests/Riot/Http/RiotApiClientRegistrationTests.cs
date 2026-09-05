using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkillIssue.GG.Infrastructure.Riot.Http;

namespace SkillIssue.GG.Infrastructure.IntegrationTests.Riot.Http;

public sealed class RiotApiClientRegistrationTests
{
    [Fact]
    public void AddInfrastructure_RegistersRiotApiClient_WithRiotTokenHeader()
    {
        const string apiKey = "test-api-key";

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PostgreSQL"] =
                    "Host=localhost;Port=5432;Database=test;Username=test;Password=test",
                ["RiotApi:ApiKey"] = apiKey,
                ["RiotApi:PlatformRoute"] = "euw1",
                ["RiotApi:RegionalRoute"] = "europe"
            })
            .Build();

        var services = new ServiceCollection();

        services.AddInfrastructure(configuration);

        using var serviceProvider = services.BuildServiceProvider();

        var riotApiClient = serviceProvider.GetRequiredService<RiotApiClient>();

        var httpClientField = typeof(RiotApiClient)
            .GetField(
                "_httpClient",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);

        Assert.NotNull(httpClientField);

        var httpClient = Assert.IsType<HttpClient>(
            httpClientField.GetValue(riotApiClient));

        Assert.True(
            httpClient.DefaultRequestHeaders.TryGetValues(
                "X-Riot-Token",
                out var values));

        Assert.Equal(apiKey, Assert.Single(values));
    }
}
