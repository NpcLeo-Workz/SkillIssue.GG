using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SkillIssue.GG.Application.Riot.Interfaces;
using SkillIssue.GG.Application.Riot.Models;

namespace SkillIssue.GG.Web.Tests.Controllers.Api;

public sealed class RiotSyncEndpointTests
{
    [Fact]
    public async Task Sync_InvalidRequest_ReturnsBadRequestWithoutCallingService()
    {
        var fakeService = new FakeSyncService();

        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");

                builder.UseSetting(
                    "ConnectionStrings:PostgreSQL",
                    "Host=localhost;Database=skillissuegg_test;Username=test;Password=test");

                builder.UseSetting(
                    "RiotApi:ApiKey",
                    "test-api-key");

                builder.UseSetting(
                    "RiotApi:PlatformRoute",
                    "euw1");

                builder.UseSetting(
                    "RiotApi:RegionalRoute",
                    "europe");

                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IRiotPlayerAndMatchSyncService>();

                    services.AddSingleton<IRiotPlayerAndMatchSyncService>(
                        fakeService);
                });
            });

        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        var request = new
        {
            GameName = "",
            TagLine = "",
            Region = "",
            Start = -1,
            Count = 101
        };

        var response = await client.PostAsJsonAsync(
            "/api/riot/sync", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(fakeService.WasCalled);
    }

    private sealed class FakeSyncService
        : IRiotPlayerAndMatchSyncService
    {
        public bool WasCalled { get; private set; }

        public Task<RiotPlayerAndMatchSyncResult> SyncAsync(
            string gameName,
            string tagLine,
            string region,
            int start = 0,
            int count = 20,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;

            return Task.FromResult(
                new RiotPlayerAndMatchSyncResult(
                    Guid.NewGuid(),
                    "test-puuid",
                    false,
                    0,
                    0,
                    0));
        }
    }
}
