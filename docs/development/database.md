# Local PostgreSQL Database Setup

SkillIssue.GG uses PostgreSQL as its relational database and Entity Framework Core with Npgsql for database access.

This document describes how to set up the local PostgreSQL development environment.

## Prerequisites

- PostgreSQL 18 or another compatible PostgreSQL version
- .NET 10 SDK
- `psql` command-line client

The SkillIssue.GG Infrastructure project already contains the required EF Core and Npgsql dependencies.

## 1. Verify PostgreSQL

Verify that the PostgreSQL command-line client is available:

```powershell
psql --version
```

Verify that the PostgreSQL Windows service is running:

```powershell
Get-Service *postgres*
```

If `psql` is installed but is not recognized, add the PostgreSQL `bin` directory to your PATH.

For PostgreSQL 18 installed in the default location:

```text
C:\Program Files\PostgreSQL\18\bin
```

Restart the terminal after modifying PATH.

## 2. Connect as the PostgreSQL Administrator

Connect to the local PostgreSQL server:

```powershell
psql -U postgres
```

Enter the administrator password configured during PostgreSQL installation.

## 3. Create the Development User

Create a dedicated PostgreSQL user for SkillIssue.GG development:

```sql
CREATE USER skillissuegg_dev WITH PASSWORD 'your-local-password';
```

Do not use a real project password in documentation or source control.

## 4. Create the Development Database

Create the local database and make the development user its owner:

```sql
CREATE DATABASE skillissuegg
    OWNER skillissuegg_dev;
```

Exit `psql`:

```sql
\q
```

## 5. Verify the Development User

Connect using the newly created development user:

```powershell
psql -h localhost -U skillissuegg_dev -d skillissuegg
```

A successful connection should open a prompt similar to:

```text
skillissuegg=>
```

Exit with:

```sql
\q
```

## 6. Configure the Application Connection String

Local database credentials must not be committed to Git.

SkillIssue.GG uses .NET user secrets for the local PostgreSQL connection string.

From the repository root:

```powershell
dotnet user-secrets set "ConnectionStrings:PostgreSQL" "Host=localhost;Port=5432;Database=skillissuegg;Username=skillissuegg_dev;Password=YOUR_PASSWORD" --project src/SkillIssue.GG.Web
```

Replace `YOUR_PASSWORD` with the password assigned to `skillissuegg_dev`.

Verify the configured user secrets:

```powershell
dotnet user-secrets list --project src/SkillIssue.GG.Web
```

The configuration should contain:

```text
ConnectionStrings:PostgreSQL = Host=localhost;Port=5432;Database=skillissuegg;Username=skillissuegg_dev;Password=...
```

Never commit the actual password or connection string containing credentials.

## 7. Verify EF Core Configuration

The EF Core migrations can be inspected without applying them:

```powershell
dotnet ef migrations list --no-connect `
  --project src/SkillIssue.GG.Infrastructure/SkillIssue.GG.Infrastructure.csproj `
  --startup-project src/SkillIssue.GG.Web/SkillIssue.GG.Web.csproj
```

The initial migration should be listed.

Applying database migrations is handled separately and is not part of the local PostgreSQL setup.

## Local Development Configuration

The default local configuration is:

| Setting | Value |
| --- | --- |
| Host | `localhost` |
| Port | `5432` |
| Database | `skillissuegg` |
| User | `skillissuegg_dev` |
| Password | Stored in .NET user secrets |

## Security

- Never commit database passwords.
- Never place real credentials in documentation.
- Use .NET user secrets for local development credentials.
- Use the dedicated `skillissuegg_dev` user rather than the PostgreSQL `postgres` superuser for application access.
- Production credentials must use a separate configuration mechanism.

## Troubleshooting

### `psql` is not recognized

Locate `psql.exe`:

```powershell
Get-ChildItem "C:\Program Files\PostgreSQL" -Recurse -Filter psql.exe -ErrorAction SilentlyContinue | Select-Object FullName
```

Add the appropriate PostgreSQL `bin` directory to PATH and restart the terminal.

### PostgreSQL service is not running

Check the service:

```powershell
Get-Service *postgres*
```

Start the appropriate PostgreSQL service if necessary.

### Authentication fails

Verify that:

- PostgreSQL is running.
- The username is `skillissuegg_dev`.
- The database is `skillissuegg`.
- The password matches the password used when creating the development user.
- The connection string in .NET user secrets contains the correct credentials.

### `__EFMigrationsHistory` does not exist

This is expected before the initial EF Core migration has been applied.

Database schema creation and migration application are handled by the database migration workflow.

## Riot Match Persistence

Mapped Riot matches are persisted through an Application-facing repository abstraction.

The persistence flow is:

```text
Riot Match-V5
    ↓
Infrastructure Riot client
    ↓
Application Riot models
    ↓
RiotMatchDomainMapper
    ↓
Match aggregate
    ↓
IMatchRepository
    ↓
MatchRepository
    ↓
SkillIssueDbContext
    ↓
PostgreSQL
```

### Repository Abstraction

The Application layer defines:

```text
src/SkillIssue.GG.Application/Matches/Interfaces/IMatchRepository.cs
```

The contract exposes:

```csharp
Task<bool> ExistsByRiotMatchIdAsync(
    string riotMatchId,
    CancellationToken cancellationToken = default);

Task AddAsync(
    Match match,
    CancellationToken cancellationToken = default);
```

The Application layer does not depend on EF Core or `SkillIssueDbContext`.

### Infrastructure Implementation

The repository implementation is located at:

```text
src/SkillIssue.GG.Infrastructure/Persistence/Repositories/MatchRepository.cs
```

It uses the existing:

```text
SkillIssueDbContext
```

to persist the complete `Match` aggregate.

`AddAsync` persists the aggregate and calls:

```csharp
SaveChangesAsync(...)
```

within the repository.

### Match Aggregate Persistence

The repository persists the supplied Domain `Match` without reconstructing it.

This preserves:

- Domain-generated Match ID
- Riot match ID
- Riot game ID
- Match metadata
- Match timestamps
- Duration
- Nullable completion fields

Participants already attached through:

```text
Match.AddParticipant(...)
```

are persisted with the aggregate.

### Participant Collections

Participant item and rune IDs continue to use the existing PostgreSQL primitive collection mappings.

```text
ItemIds -> integer[]
RuneIds -> integer[]
```

No join tables are introduced for these collections.

### Duplicate Detection

Duplicate checks use:

```text
Match.RiotMatchId
```

The repository exposes:

```csharp
ExistsByRiotMatchIdAsync(...)
```

which performs an existence query without loading the full aggregate.

The database uniqueness constraint remains the final protection against duplicate Riot match IDs.

Duplicate database violations are not silently swallowed.

### Cancellation

Repository operations accept:

```text
CancellationToken
```

and pass cancellation through to EF Core asynchronous operations.

### Integration Testing

Repository integration tests are located under:

```text
tests/SkillIssue.GG.Infrastructure.IntegrationTests/Persistence/Repositories/
```

The tests use the existing PostgreSQL Testcontainers fixture.

Coverage includes:

- Match aggregate persistence
- Match metadata persistence
- Nullable completion fields
- Participant persistence
- Multiple participants
- Item ID arrays
- Rune ID arrays
- Empty item arrays
- Empty rune arrays
- Riot match existence checks
- Duplicate Riot match protection
- Cancellation behavior

No real Riot API calls are made during persistence testing.

### Verification

Run:

```powershell
dotnet test tests/SkillIssue.GG.Infrastructure.IntegrationTests/SkillIssue.GG.Infrastructure.IntegrationTests.csproj
dotnet build SkillIssue.GG.slnx
dotnet test SkillIssue.GG.slnx
```

## Player Repository

Player persistence is exposed to the Application layer through:

```text
src/SkillIssue.GG.Application/Players/Interfaces/IPlayerRepository.cs
```

The Infrastructure implementation is located at:

```text
src/SkillIssue.GG.Infrastructure/Persistence/Repositories/PlayerRepository.cs
```

The repository uses the existing:

```text
SkillIssueDbContext
```

and PostgreSQL persistence configuration.

### Supported Operations

The repository currently supports:

```csharp
Task<Player?> GetByPuuidAsync(
    string puuid,
    CancellationToken cancellationToken = default);

Task AddAsync(
    Player player,
    CancellationToken cancellationToken = default);
```

### GetByPuuidAsync

`GetByPuuidAsync` retrieves a Player using Riot's PUUID as the lookup key.

Behavior:

```text
Matching Player exists
    -> returns Player

No matching Player exists
    -> returns null
```

The query uses:

```text
AsNoTracking()
```

because the repository is performing a read-only lookup.

Invalid PUUID values are rejected before database access:

```text
null
empty
whitespace
```

### AddAsync

`AddAsync` adds the supplied Player to the DbContext and immediately persists it using:

```text
SaveChangesAsync
```

A null Player is rejected before database access.

This follows the same narrow repository approach used by `MatchRepository`.

### Uniqueness

Player PUUID uniqueness is enforced by the existing database schema.

Attempting to persist multiple Players with the same PUUID results in a database update failure.

The repository does not silently ignore or overwrite duplicate Players.

### Architecture

The dependency direction remains:

```text
Application
    ↓
IPlayerRepository
    ↓ implemented by
Infrastructure
    ↓
PlayerRepository
    ↓
SkillIssueDbContext
    ↓
PostgreSQL
```

The Domain project remains persistence-agnostic and contains no EF Core dependencies.

### Dependency Injection

The repository is registered through the Infrastructure composition setup:

```text
IPlayerRepository
    ->
PlayerRepository
```

### Integration Testing

Player repository behavior is verified using the existing PostgreSQL Testcontainers infrastructure.

Tests cover:

```text
Player persistence
PUUID lookup
Unknown PUUID behavior
Field preservation
Duplicate PUUID constraints
Invalid PUUID validation
Null Player validation
Cancellation
```

These are database integration tests and require Docker to be available.