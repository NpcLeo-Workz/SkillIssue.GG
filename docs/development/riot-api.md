# Riot API Configuration

SkillIssue.GG uses Riot Games APIs to retrieve account and match data.

This document explains how to configure Riot API access for local development.

## Prerequisites

- .NET 10 SDK
- A Riot Developer account
- A valid Riot API key

## Configuration Section

Riot API configuration is stored under the `RiotApi` section.

The application configuration should contain only non-secret defaults:

```json
{
  "RiotApi": {
    "ApiKey": "",
    "PlatformRoute": "euw1",
    "RegionalRoute": "europe"
  }
}
```

## HTTP Client Infrastructure

SkillIssue.GG uses ASP.NET Core's `HttpClientFactory` for communication with Riot APIs.

The shared client is implemented in:

```text
src/SkillIssue.GG.Infrastructure/Riot/Http/RiotApiClient.cs
```

It is registered through the Infrastructure dependency injection setup.

### HttpClientFactory

The Riot API client is registered as a typed `HttpClient`.

This avoids manually creating `HttpClient` instances and allows shared configuration to be applied consistently.

The registration also configures authentication for outgoing Riot requests.

### Authentication

Riot API requests use the following header:

```text
X-Riot-Token
```

The value comes from the configured `RiotApiOptions.ApiKey`.

The API key must never be:

- Hardcoded in source code
- Added to request URLs
- Written to logs
- Included in exception messages
- Included in test data using a real key
- Committed to source control

### Routing

Riot APIs use different routing hosts depending on the endpoint.

SkillIssue.GG currently stores both:

```text
PlatformRoute
RegionalRoute
```

The shared HTTP client does not use a single global Riot base address.

Endpoint-specific clients will determine which routing value is required.

Examples of future endpoint-specific clients include:

- Account-V1
- Match-V5

### Error Handling

Failed Riot API responses are represented by:

```text
src/SkillIssue.GG.Infrastructure/Riot/Http/RiotApiException.cs
```

The exception currently preserves the HTTP status code without exposing sensitive request information.

The exception does not contain:

- API keys
- Request headers
- Response bodies
- Sensitive request URLs

Detailed handling for individual Riot errors and rate limiting will be added separately.

### Cancellation

Riot HTTP requests support `CancellationToken`.

Cancellation is propagated to the underlying `HttpClient` request so callers can cancel ongoing Riot API operations.

### Testing

Shared Riot HTTP behavior is tested without contacting the real Riot API.

Current tests cover:

- Successful GET requests
- Failed requests producing `RiotApiException`
- Cancellation propagation
- Dependency injection registration
- `X-Riot-Token` header configuration

The tests do not require a real Riot API key.

### Verification

Run the Riot HTTP tests:

```powershell
dotnet test tests/SkillIssue.GG.Infrastructure.IntegrationTests/SkillIssue.GG.Infrastructure.IntegrationTests.csproj --filter "FullyQualifiedName~RiotApiClient"
```

Then verify the full solution:

```powershell
dotnet build SkillIssue.GG.slnx
dotnet test SkillIssue.GG.slnx
```