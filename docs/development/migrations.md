# Entity Framework Core Migrations

SkillIssue.GG uses Entity Framework Core with PostgreSQL through Npgsql.

This document describes how to inspect, apply, and verify database migrations in the local development environment.

## Prerequisites

Before applying migrations:

- PostgreSQL must be installed and running
- The local `skillissuegg` database must exist
- The `skillissuegg_dev` database user must exist
- The local PostgreSQL connection string must be configured through .NET user secrets
- The EF Core CLI must be available

Verify the EF Core CLI:

```powershell
dotnet ef --version
```

## Project Configuration

The EF Core `DbContext` is located in:

```text
src/SkillIssue.GG.Infrastructure
```

The ASP.NET Core startup project is:

```text
src/SkillIssue.GG.Web
```

Because the `DbContext` and startup project are separate, EF Core commands should specify both projects explicitly.

## Inspect Available Migrations

To list migrations without connecting to the database:

```powershell
dotnet ef migrations list --no-connect `
  --project src/SkillIssue.GG.Infrastructure/SkillIssue.GG.Infrastructure.csproj `
  --startup-project src/SkillIssue.GG.Web/SkillIssue.GG.Web.csproj
```

This is useful for confirming which migrations exist in the codebase.

## Apply Migrations

Apply all pending migrations to the configured local PostgreSQL database:

```powershell
dotnet ef database update `
  --project src/SkillIssue.GG.Infrastructure/SkillIssue.GG.Infrastructure.csproj `
  --startup-project src/SkillIssue.GG.Web/SkillIssue.GG.Web.csproj
```

For a new database, EF Core may initially log a failed query against:

```text
__EFMigrationsHistory
```

This can occur because the migration history table does not exist yet.

If the command continues and ends successfully, EF Core will create the history table and apply the migration.

Always check the final command result before treating the initial history-table lookup as an error.

## Verify Applied Migrations

After applying migrations:

```powershell
dotnet ef migrations list `
  --project src/SkillIssue.GG.Infrastructure/SkillIssue.GG.Infrastructure.csproj `
  --startup-project src/SkillIssue.GG.Web/SkillIssue.GG.Web.csproj
```

Applied migrations should no longer be marked as pending.

## Verify the PostgreSQL Schema

Connect to the local development database:

```powershell
psql -h localhost -U skillissuegg_dev -d skillissuegg
```

List the tables:

```sql
\dt
```

The initial schema should contain:

```text
players
matches
match_participants
champions
items
runes
patches
__EFMigrationsHistory
```

## Inspect Table Definitions

Use the PostgreSQL describe command to inspect tables:

```sql
\d match_participants
```

The `match_participants` table should include:

- `MatchId`
- `ParticipantId`
- PostgreSQL `integer[]` storage for item IDs
- PostgreSQL `integer[]` storage for rune IDs
- A foreign key from `MatchId` to `matches.Id`
- Cascade delete behavior
- A unique index covering `(MatchId, ParticipantId)`

Other tables can be inspected with:

```sql
\d players
\d matches
\d champions
\d items
\d runes
\d patches
```

## Verify Migration History

Query the migration history table:

```sql
SELECT * FROM "__EFMigrationsHistory";
```

The applied `InitialCreate` migration should be present.

Exit PostgreSQL with:

```sql
\q
```

## Final Verification

After applying and verifying migrations, run:

```powershell
dotnet build SkillIssue.GG.slnx
dotnet test SkillIssue.GG.slnx
```

Both commands should succeed before the migration work is considered complete.

## Local Migration Workflow

The normal local workflow is:

1. Update `development`.
2. Create or switch to the relevant feature or infrastructure branch.
3. Inspect available migrations.
4. Apply pending migrations.
5. Verify the resulting PostgreSQL schema.
6. Verify migration history.
7. Build the solution.
8. Run the test suite.

## Notes

- Do not commit database credentials.
- Do not modify an already-shared migration merely to change an existing deployed schema.
- Schema changes should normally be introduced through new migrations.
- Production migration deployment is handled separately from the local development workflow.
- Database seeding is not part of the migration process unless explicitly implemented.