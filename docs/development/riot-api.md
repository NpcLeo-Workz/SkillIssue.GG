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

## Match-V5 History Integration

SkillIssue.GG uses Riot Match-V5 to retrieve match IDs for a player using their PUUID.

This integration retrieves match history identifiers only. Fetching individual match details is implemented separately.

### Application Contract

The Application layer defines:

```text
src/SkillIssue.GG.Application/Riot/Interfaces/IRiotMatchHistoryService.cs
```

The contract accepts:

```text
puuid
start
count
CancellationToken
```

and returns a read-only collection of Riot match IDs.

The Application layer does not depend on Riot HTTP types or Infrastructure DTOs.

### Infrastructure Implementation

The Match-V5 history implementation is located at:

```text
src/SkillIssue.GG.Infrastructure/Riot/Match/RiotMatchHistoryService.cs
```

The service reuses the shared:

```text
RiotApiClient
```

for authentication and HTTP behavior.

### Match History Lookup

The Match-V5 history endpoint is:

```text
GET /lol/match/v5/matches/by-puuid/{puuid}/ids
```

Pagination is supplied using:

```text
start
count
```

Example:

```text
GET /lol/match/v5/matches/by-puuid/{puuid}/ids?start=0&count=20
```

### Regional Routing

Match-V5 uses a regional routing host.

The regional route comes from:

```text
RiotApi:RegionalRoute
```

For example:

```text
europe
```

produces requests against:

```text
https://europe.api.riotgames.com
```

The implementation does not use `PlatformRoute` for Match-V5.

### PUUID Encoding

The PUUID is URL-encoded before being included in the request path.

This prevents unsafe path construction and ensures special characters are handled correctly.

### Pagination

The service supports:

```text
start
count
```

The default values are:

```text
start = 0
count = 20
```

Validation is performed before sending a request.

Current rules:

- `start` must not be negative
- `count` must be greater than zero
- `count` must not exceed 100

Invalid pagination values fail before contacting Riot.

### Response Format

Match-V5 returns a JSON array of Riot match IDs.

Example:

```json
[
  "EUW1_1234567890",
  "EUW1_1234567891"
]
```

These values are returned to the Application layer as:

```text
IReadOnlyList<string>
```

An empty response:

```json
[]
```

is valid and returns an empty collection.

A successful response containing null or invalid match IDs is treated as invalid.

Malformed JSON also fails explicitly.

### Error Handling

Failed Riot API responses continue to use the shared `RiotApiException` behavior.

Examples include:

```text
400 Bad Request
403 Forbidden
404 Not Found
429 Too Many Requests
5xx responses
```

Detailed rate-limit and retry behavior remains outside the scope of the current implementation.

### Cancellation

Match history lookup supports `CancellationToken`.

Cancellation is propagated through:

```text
IRiotMatchHistoryService
    ↓
RiotMatchHistoryService
    ↓
RiotApiClient
    ↓
HttpClient
```

### Testing

Match-V5 history behavior is tested without contacting the real Riot API.

Current tests cover:

- Successful match history retrieval
- Multiple match IDs
- Empty match history
- Correct Match-V5 endpoint
- Regional routing
- PUUID encoding
- Default pagination
- Custom pagination
- Invalid PUUID
- Negative `start`
- Invalid `count`
- Failed Riot API responses
- Malformed JSON
- Null responses
- Invalid match IDs
- Cancellation propagation

The tests do not require a real Riot API key.

### Verification

Run the Match-V5 history tests:

```powershell
dotnet test tests/SkillIssue.GG.Infrastructure.IntegrationTests/SkillIssue.GG.Infrastructure.IntegrationTests.csproj --filter "FullyQualifiedName~RiotMatchHistoryServiceTests"
```

Then verify the full solution:

```powershell
dotnet build SkillIssue.GG.slnx
dotnet test SkillIssue.GG.slnx
```

## Match-V5 Details Integration

SkillIssue.GG uses Riot Match-V5 to retrieve full match details for a specific Riot match ID.

This integration is responsible for HTTP retrieval, Riot response deserialization, response validation, and mapping into Application-facing models.

Mapping into Domain entities and persistence are handled separately.

### Application Contract

The Application layer defines:

```text
src/SkillIssue.GG.Application/Riot/Interfaces/IRiotMatchService.cs
```

The service accepts:

```text
matchId
CancellationToken
```

and returns:

```text
RiotMatchDetails
```

The Application layer does not depend on Riot HTTP types or Infrastructure DTOs.

### Application Models

Application-facing models are located under:

```text
src/SkillIssue.GG.Application/Riot/Models/
```

Current models include:

```text
RiotMatchDetails
RiotMatchParticipant
```

These models expose the match and participant data required by later import and mapping steps.

### Infrastructure Implementation

The Match-V5 details implementation is located at:

```text
src/SkillIssue.GG.Infrastructure/Riot/Match/RiotMatchService.cs
```

Riot-specific DTOs are located under:

```text
src/SkillIssue.GG.Infrastructure/Riot/Match/Dto/
```

The DTOs mirror the Riot Match-V5 JSON structure needed by SkillIssue.GG.

### Match Details Endpoint

The Match-V5 details endpoint is:

```text
GET /lol/match/v5/matches/{matchId}
```

Example:

```text
GET /lol/match/v5/matches/EUW1_1234567890
```

### Regional Routing

Match-V5 uses a regional routing host.

The route comes from:

```text
RiotApi:RegionalRoute
```

For example:

```text
europe
```

produces requests against:

```text
https://europe.api.riotgames.com
```

`PlatformRoute` is not used for Match-V5.

### Match ID Encoding

The Riot match ID is URL-encoded before being inserted into the request path.

This keeps request construction safe and prevents special characters from altering the endpoint path.

### Match Fields

The Match-V5 response is mapped into Application data including:

```text
metadata.dataVersion
metadata.matchId

info.gameId
info.gameVersion
info.gameMode
info.gameType
info.mapId
info.queueId
info.platformId
info.gameCreation
info.gameStartTimestamp
info.gameEndTimestamp
info.gameDuration
info.endOfGameResult
```

Riot timestamps expressed as Unix milliseconds are converted into:

```text
DateTimeOffset
```

Game duration is converted into:

```text
TimeSpan
```

### Participant Fields

Participant data currently includes:

```text
puuid
participantId
teamId
championId
championName
teamPosition

kills
deaths
assists

goldEarned
goldSpent

totalMinionsKilled
neutralMinionsKilled

visionScore
wardsPlaced
wardsKilled

totalDamageDealt
totalDamageDealtToChampions
totalDamageTaken

timePlayed
win
```

Participant `timePlayed` is converted from seconds into:

```text
TimeSpan
```

`championName` is retained in the Application-facing Riot model even though the normalized Domain model uses `ChampionId` rather than storing the champion name on `MatchParticipant`.

### Items

Match-V5 exposes item slots as:

```text
item0
item1
item2
item3
item4
item5
item6
```

The Infrastructure mapping flattens these values into:

```text
IReadOnlyList<int> ItemIds
```

Item ID `0` represents an empty slot and is excluded.

Item order is preserved.

### Runes

Selected runes are obtained from the nested Match-V5 perk structure:

```text
perks
  └── styles
      └── selections
          └── perk
```

The selected `perk` values are flattened into:

```text
IReadOnlyList<int> RuneIds
```

Non-positive rune IDs are ignored.

Duplicate rune IDs are removed during the Infrastructure mapping step.

### Nullable Fields

The current integration allows nullable Riot match completion fields:

```text
gameEndTimestamp
endOfGameResult
```

These map to:

```text
DateTimeOffset? EndedAt
string? EndOfGameResult
```

This supports payloads where completion-related information is unavailable.

### Response Validation

Successful HTTP responses are validated before being returned to the Application layer.

The current implementation requires:

```text
metadata
metadata.matchId
metadata.dataVersion
info
info.participants
```

At least one participant must be present.

Participants must currently contain valid:

```text
puuid
participantId
championId
```

Invalid successful responses fail explicitly instead of creating incomplete Application models.

### Error Handling

Unsuccessful Riot HTTP responses continue to use the shared:

```text
RiotApiException
```

Examples include:

```text
400 Bad Request
403 Forbidden
404 Not Found
429 Too Many Requests
5xx responses
```

Malformed JSON surfaces as a JSON deserialization error.

Advanced rate-limit and retry behavior remains outside the scope of the current integration.

### Cancellation

Match details retrieval supports `CancellationToken`.

Cancellation is propagated through:

```text
IRiotMatchService
    ↓
RiotMatchService
    ↓
RiotApiClient
    ↓
HttpClient
```

### Dependency Injection

The Infrastructure implementation is registered as:

```text
IRiotMatchService
    → RiotMatchService
```

Consumers depend on the Application abstraction rather than the concrete Infrastructure implementation.

### Testing

Match-V5 details behavior is tested without contacting Riot.

Current tests cover:

- Successful match retrieval
- Correct Match-V5 endpoint
- Regional routing
- Match ID encoding
- Match metadata mapping
- Match info mapping
- Timestamp conversion
- Duration conversion
- Participant mapping
- Item ID mapping
- Rune ID mapping
- Nullable completion fields
- Missing metadata
- Missing match ID
- Missing data version
- Missing match info
- Missing participants
- Empty participants
- Invalid participant PUUID
- Invalid participant ID
- Invalid champion ID
- Malformed JSON
- Null responses
- Riot API errors
- Cancellation propagation

The tests use stubbed HTTP behavior.

They do not contact the real Riot API and do not require a real Riot API key.

### Verification

Run the Match-V5 details tests:

```powershell
dotnet test tests/SkillIssue.GG.Infrastructure.IntegrationTests/SkillIssue.GG.Infrastructure.IntegrationTests.csproj --filter "FullyQualifiedName~RiotMatchServiceTests"
```

Then verify the full solution:

```powershell
dotnet build SkillIssue.GG.slnx
dotnet test SkillIssue.GG.slnx
```

## Riot Match Domain Mapping

Match-V5 data is converted from the Application-facing Riot models into the existing SkillIssue.GG Domain entities before persistence.

The mapping layer does not depend directly on Riot HTTP responses or Infrastructure DTOs.

### Mapping Boundary

The current flow is:

```text
Riot Match-V5
    ↓
Infrastructure DTOs
    ↓
Application Riot models
    ↓
RiotMatchDomainMapper
    ↓
Domain entities
```

The mapper is located at:

```text
src/SkillIssue.GG.Application/Riot/Mapping/RiotMatchDomainMapper.cs
```

This keeps HTTP, JSON, and Riot-specific transport concerns outside the Domain.

### Match Mapping

`RiotMatchDetails` is mapped into the existing:

```text
Match
```

The mapping preserves:

```text
DataVersion
RiotMatchId
RiotGameId
GameVersion
GameMode
GameType
MapId
QueueId
PlatformId
GameCreatedAt
StartedAt
EndedAt
Duration
EndOfGameResult
```

A new Domain `Guid` is generated by the `Match` entity.

Nullable:

```text
EndedAt
EndOfGameResult
```

values are preserved.

### Participant Mapping

Each `RiotMatchParticipant` is mapped into:

```text
MatchParticipant
```

The mapping preserves Domain-supported participant data including:

```text
Puuid
ParticipantId
TeamId
ChampionId
TeamPosition

Kills
Deaths
Assists

GoldEarned
GoldSpent

TotalMinionsKilled
NeutralMinionsKilled

VisionScore
WardsPlaced
WardsKilled

TotalDamageDealtToChampions
TotalDamageTaken

TimePlayed
Won
```

Each participant receives the newly created Domain match ID.

Each participant also receives its own Domain-generated `Guid`.

`ChampionName` is not stored on the normalized Domain participant.

The Riot/Application model also currently exposes `TotalDamageDealt`, but the existing Domain model does not represent that statistic. The mapper therefore does not persist or map that value.

### Player Identity

Player identity remains PUUID-based.

```text
RiotMatchParticipant.Puuid
    ↓
MatchParticipant.PlayerPuuid
```

The mapper does not perform Player database lookups or introduce a Player navigation relationship.

### Items

Application item IDs are added using the Domain API:

```text
MatchParticipant.AddItem(...)
```

The mapper does not directly manipulate the participant's item backing collection.

Item reference entities are not resolved during this mapping step.

### Runes

Application rune IDs are added using:

```text
MatchParticipant.AddRune(...)
```

The mapper does not directly manipulate the participant's rune backing collection.

Rune reference entities are not resolved during this mapping step.

### Aggregate Construction

Mapped participants are attached to the match through:

```text
Match.AddParticipant(...)
```

The mapper does not directly manipulate the Match participant backing collection.

This ensures the existing aggregate invariants remain authoritative.

### Domain Validation

The mapper intentionally does not duplicate Domain validation.

Domain entities are created through their existing constructors and collection methods:

```text
new Match(...)
new MatchParticipant(...)
MatchParticipant.AddItem(...)
MatchParticipant.AddRune(...)
Match.AddParticipant(...)
```

Invalid source data that violates an existing Domain invariant is rejected by the Domain.

### Persistence

The mapper does not perform database operations.

Persistence, duplicate-match detection, and import orchestration are separate responsibilities and are implemented in later integration steps.

### Testing

Pure Application mapping tests are located in:

```text
tests/SkillIssue.GG.Application.Tests
```

The mapping tests cover:

- Match field mapping
- Riot match identifiers
- Match timestamps and duration
- Nullable match completion fields
- Participant mapping
- PUUID mapping
- Champion ID mapping
- Participant statistics supported by the Domain
- Item ID mapping
- Rune ID mapping
- Empty item and rune collections
- Multiple participants
- Generated Domain IDs
- Match ID propagation to participants
- Aggregate participant attachment
- Domain validation propagation
- Null mapper input

These tests require no:

```text
PostgreSQL
Docker
Riot API
HTTP
```

### Verification

Run the mapping tests:

```powershell
dotnet test tests/SkillIssue.GG.Application.Tests/SkillIssue.GG.Application.Tests.csproj
```

Then verify the complete solution:

```powershell
dotnet build SkillIssue.GG.slnx
dotnet test SkillIssue.GG.slnx
```

## Riot Match Import Orchestration

A single Riot match can now be imported through an Application-layer orchestration service.

The flow is:

```text
Riot match ID
    ↓
IMatchRepository.ExistsByRiotMatchIdAsync(...)
    ↓
Already exists?
    ├── Yes → return skipped result
    └── No
         ↓
IRiotMatchService.GetMatchAsync(...)
         ↓
RiotMatchDomainMapper.Map(...)
         ↓
IMatchRepository.AddAsync(...)
         ↓
Return imported result
```

### Import Contract

The Application layer defines:

```text
src/SkillIssue.GG.Application/Riot/Interfaces/IRiotMatchImportService.cs
```

with:

```csharp
Task<RiotMatchImportResult> ImportAsync(
    string matchId,
    CancellationToken cancellationToken = default);
```

The result model is:

```text
src/SkillIssue.GG.Application/Riot/Models/RiotMatchImportResult.cs
```

and contains:

```text
RiotMatchId
Imported
MatchId
```

For a newly imported match:

```text
Imported = true
MatchId = generated Domain Match ID
```

For an already persisted match:

```text
Imported = false
MatchId = null
```

### Application Service

The implementation is located at:

```text
src/SkillIssue.GG.Application/Riot/Services/RiotMatchImportService.cs
```

It coordinates:

```text
IRiotMatchService
IMatchRepository
RiotMatchDomainMapper
```

The service does not depend directly on:

```text
HttpClient
RiotApiClient
Infrastructure DTOs
SkillIssueDbContext
EF Core
Npgsql
```

### Duplicate Check

The import service checks:

```csharp
IMatchRepository.ExistsByRiotMatchIdAsync(...)
```

before retrieving Riot match details.

If the match already exists:

- no Riot API lookup is performed
- no mapping is performed
- no persistence is performed
- the import result reports `Imported = false`

The database uniqueness constraint on Riot match IDs remains the final protection against concurrent duplicate inserts.

### Match Retrieval

For a match that is not yet persisted, the service retrieves Match-V5 data through:

```text
IRiotMatchService
```

This keeps HTTP and Riot transport concerns behind the existing Application abstraction.

### Domain Mapping

The returned:

```text
RiotMatchDetails
```

is mapped using the existing:

```text
RiotMatchDomainMapper
```

The import service does not duplicate mapping logic.

### Persistence

The mapped Domain aggregate is persisted through:

```text
IMatchRepository
```

The import service does not access `SkillIssueDbContext` or EF Core directly.

### Cancellation

The supplied `CancellationToken` is passed through to:

```text
IMatchRepository.ExistsByRiotMatchIdAsync(...)
IRiotMatchService.GetMatchAsync(...)
IMatchRepository.AddAsync(...)
```

Cancellation exceptions are not swallowed.

### Dependency Injection

`IRiotMatchImportService` is registered through the existing Infrastructure composition registration alongside the other application-facing implementations.

### Testing

Pure Application tests are located under:

```text
tests/SkillIssue.GG.Application.Tests/Riot/Services/
```

Coverage includes:

- Importing a missing match
- Duplicate check before Riot retrieval
- Riot match retrieval
- Domain mapping
- Match persistence
- Imported result
- Generated Domain Match ID
- Already imported match behavior
- Skipping Riot lookup for existing matches
- Skipping persistence for existing matches
- Invalid match ID validation
- Riot service failure propagation
- Repository failure propagation
- Cancellation token propagation

These tests require no:

```text
PostgreSQL
Docker
HTTP
Riot API
```

### Scope

This service imports exactly one Riot match.

It does not:

```text
Retrieve match history
Import multiple matches
Synchronize a player's history
Run background jobs
Schedule imports
Calculate statistics
```

Those responsibilities remain separate application workflows.