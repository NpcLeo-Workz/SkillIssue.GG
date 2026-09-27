# Persistence Integration Tests

SkillIssue.GG uses integration tests to verify the Entity Framework Core persistence layer against a real PostgreSQL database.

These tests use Testcontainers for .NET to start an isolated PostgreSQL container for the duration of the test run.

## Why Real PostgreSQL

The persistence tests intentionally do not use the EF Core InMemory provider.

A real PostgreSQL instance is required to verify behavior such as:

- Npgsql mappings
- PostgreSQL `integer[]` columns
- relational foreign keys
- cascade delete behavior
- unique constraints
- migration compatibility

## Prerequisites

- .NET 10 SDK
- Docker Desktop
- Docker engine running
- WSL 2 / virtualization support enabled on Windows

Verify Docker:

```powershell
docker version
docker ps
```

Both commands should complete successfully before running the integration tests.

## Test Project

Persistence integration tests are located in:

```text
tests/SkillIssue.GG.Infrastructure.IntegrationTests
```

The project references:

```text
src/SkillIssue.GG.Infrastructure
```

and uses:

```text
Testcontainers.PostgreSql
Microsoft.EntityFrameworkCore.Design
```

## PostgreSQL Test Container

The shared test fixture starts a PostgreSQL 18 container using Testcontainers.

The test database is isolated from the developer's local `skillissuegg` database.

The test fixture:

1. Starts a PostgreSQL container.
2. Builds an EF Core `SkillIssueDbContext`.
3. Applies the existing EF Core migrations.
4. Runs persistence tests against the container.
5. Disposes the container after the test run.

No local database credentials are required.

## Shared xUnit Fixture

The PostgreSQL container is shared through an xUnit collection fixture.

Tests that require PostgreSQL use:

```csharp
[Collection("PostgreSQL")]
```

This allows the integration-test classes to use the shared `PostgreSqlFixture`.

## Current Coverage

The persistence integration tests verify:

### Player

- Player entities can be inserted and loaded.
- `Puuid` uniqueness is enforced by PostgreSQL.

### Match and MatchParticipant

- Matches can be persisted.
- Participants can be persisted with their match.
- Match participants can be loaded through the Match participant collection.
- Match deletion cascades to participants.
- `(MatchId, ParticipantId)` uniqueness is enforced.
- Nullable match end fields persist correctly.

### PostgreSQL Arrays

Participant item IDs are persisted as:

```text
integer[]
```

Participant rune IDs are persisted as:

```text
integer[]
```

Item ID duplicates are preserved.

Rune IDs persist correctly.

### Reference Data

Persistence is verified for:

- Champion
- Item
- Rune
- Patch

Database uniqueness is verified for:

- `Champion.RiotChampionId`
- `Item.RiotItemId`
- `Rune.RiotRuneId`
- `Patch.Version`

## Running Integration Tests

Run only the persistence integration tests:

```powershell
dotnet test tests/SkillIssue.GG.Infrastructure.IntegrationTests/SkillIssue.GG.Infrastructure.IntegrationTests.csproj
```

Run all tests in the solution:

```powershell
dotnet test SkillIssue.GG.slnx
```

Run a full build:

```powershell
dotnet build SkillIssue.GG.slnx
```

## Docker Troubleshooting

### Docker is unavailable

If Testcontainers reports:

```text
DockerUnavailableException
```

verify that Docker Desktop is running:

```powershell
docker version
docker ps
```

### Virtualization support not detected

On Windows, verify virtualization is enabled in firmware and that WSL 2 can start.

Check:

```powershell
wsl --status
wsl --version
```

Ensure the Virtual Machine Platform Windows feature is enabled.

If required:

```powershell
wsl --install --no-distribution
```

Then restart Windows.

### Docker endpoint errors

If Testcontainers cannot connect to:

```text
npipe://./pipe/docker_engine
```

Docker Desktop is usually not running or the Docker engine has not finished starting.

Wait until Docker Desktop reports that the engine is running, then retry the tests.

## Test Isolation

Integration tests must not:

- use the developer's local `skillissuegg` database
- depend on committed credentials
- depend on manually created test data
- leave persistent test infrastructure behind

The PostgreSQL container is disposable and should be recreated automatically for each test run.

## Adding New Persistence Tests

When adding new persistence behavior:

1. Add the test to the persistence integration-test project.
2. Use the shared PostgreSQL fixture.
3. Exercise the actual EF Core/Npgsql mapping.
4. Prefer database-level verification for constraints and relationships.
5. Run the integration-test project.
6. Run the full solution test suite.

## Final Verification

Before persistence integration-test changes are merged:

```powershell
dotnet build SkillIssue.GG.slnx
dotnet test SkillIssue.GG.slnx
```

Both commands should succeed.