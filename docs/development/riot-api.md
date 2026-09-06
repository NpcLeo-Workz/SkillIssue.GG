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

## Account-V1 Integration

SkillIssue.GG uses Riot Account-V1 to resolve a Riot ID into a Riot account and obtain the player's PUUID.

The Account-V1 integration is implemented through an Application abstraction and an Infrastructure implementation.

### Application Contract

The Application layer defines:

```text
src/SkillIssue.GG.Application/Riot/Interfaces/IRiotAccountService.cs
```

The result model is:

```text
src/SkillIssue.GG.Application/Riot/Models/RiotAccount.cs
```

The Application layer does not depend on Riot HTTP DTOs or transport-specific types.

### Infrastructure Implementation

The Riot Account-V1 implementation is located at:

```text
src/SkillIssue.GG.Infrastructure/Riot/Account/RiotAccountService.cs
```

The external response DTO is located at:

```text
src/SkillIssue.GG.Infrastructure/Riot/Account/Dto/RiotAccountDto.cs
```

The Infrastructure DTO mirrors the Riot API response and is mapped into the Application `RiotAccount` model before crossing the boundary.

### Riot ID Lookup

A Riot ID consists of:

```text
gameName
tagLine
```

Example:

```text
Some Player#EUW
```

is represented as:

```text
gameName = Some Player
tagLine = EUW
```

The lookup uses:

```text
GET /riot/account/v1/accounts/by-riot-id/{gameName}/{tagLine}
```

### Regional Routing

Account-V1 uses a regional routing host.

The regional route comes from:

```text
RiotApi:RegionalRoute
```

For example:

```text
europe
```

produces a host such as:

```text
https://europe.api.riotgames.com
```

The Account-V1 implementation must not hardcode a specific regional route.

### URL Encoding

Both `gameName` and `tagLine` are URL-encoded before being included in the request path.

This allows Riot IDs containing spaces or other characters to be handled safely.

### Response Data

SkillIssue.GG currently uses the following Account-V1 response fields:

```text
puuid
gameName
tagLine
```

These values are deserialized into `RiotAccountDto` and then mapped to the Application `RiotAccount` model.

### Error Handling

The Account-V1 service reuses the shared `RiotApiClient`.

Failed Riot API responses continue to use the shared `RiotApiException` behavior.

Examples include:

```text
404 Not Found
403 Forbidden
429 Too Many Requests
5xx responses
```

Detailed retry and rate-limit behavior is handled separately.

A successful HTTP response that does not contain valid account data is treated as an invalid response rather than returning an incomplete account.

Malformed JSON also fails explicitly.

### Cancellation

Account lookup supports `CancellationToken`.

The token is propagated through:

```text
IRiotAccountService
    ↓
RiotAccountService
    ↓
RiotApiClient
    ↓
HttpClient
```

### Testing

Account-V1 behavior is tested without contacting the real Riot API.

Current tests cover:

- Successful account lookup
- PUUID deserialization
- Game name deserialization
- Tag line deserialization
- Correct Account-V1 request path
- Regional route usage
- URL encoding
- Failed Riot API responses
- Invalid successful responses
- Malformed JSON
- Invalid input
- Cancellation propagation

The tests do not require a real Riot API key.

### Verification

Run the Account-V1 tests:

```powershell
dotnet test tests/SkillIssue.GG.Infrastructure.IntegrationTests/SkillIssue.GG.Infrastructure.IntegrationTests.csproj --filter "FullyQualifiedName~RiotAccountServiceTests"
```

Then verify the full solution:

```powershell
dotnet build SkillIssue.GG.slnx
dotnet test SkillIssue.GG.slnx
```