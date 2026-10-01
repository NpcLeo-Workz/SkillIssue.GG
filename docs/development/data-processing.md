# Data Processing

## Overview

Data Processing transforms external Riot game data into valid SkillIssue.GG Domain entities.

The processing boundary sits between Game Data Acquisition and the Statistics Engine.

```text
Riot API
   ↓
Game Data Acquisition
   ↓
Application Riot models
   ↓
Data Processing
   ↓
SkillIssue.GG Domain entities
   ↓
Statistics Engine
```

Game Data Acquisition is responsible for retrieving Riot data.

Data Processing is responsible for normalizing and validating that data as it is transformed into SkillIssue.GG Domain entities.

The Statistics Engine is responsible for calculations such as win rate, KDA, CS/min, gold/min, and champion statistics.

## Riot-to-Domain transformation

Match data retrieved from Riot is represented in the Application layer by `RiotMatchDetails` and `RiotMatchParticipant`.

`RiotMatchDomainMapper` transforms this representation into the Domain model:

```text
RiotMatchDetails
      ↓
RiotMatchDomainMapper
      ↓
Match
 └── MatchParticipant
       ├── ItemIds
       └── RuneIds
```

Domain constructors and methods remain responsible for enforcing Domain invariants.

The mapper does not bypass Domain validation.

## Match processing

The following Riot match values are preserved when creating a `Match`:

- Riot match ID
- Riot game ID
- data version
- game version
- game mode
- game type
- map ID
- queue ID
- platform ID
- game creation time
- game start time
- game end time
- duration
- end-of-game result

`EndedAt` and `EndOfGameResult` may be absent and are preserved as `null`.

The full Riot game version is stored in `Match.GameVersion`.

## Patch handling

Data Processing does not create a direct relationship between `Match` and `Patch`.

A match retains the full Riot game version, for example:

```text
16.15.123.4567
```

Patch matching is based on the major and minor version components:

```text
16.15
```

This behavior is represented by the Domain rather than by a `Patch` foreign key or navigation property.

## Participant processing

Each Riot participant is transformed into a `MatchParticipant` belonging to the newly created `Match`.

The following values are mapped:

- player PUUID
- participant ID
- team ID
- champion ID
- team position
- kills
- deaths
- assists
- gold earned
- gold spent
- total minions killed
- neutral minions killed
- vision score
- wards placed
- wards killed
- total damage dealt to champions
- total damage taken
- time played
- win/loss

Each `MatchParticipant` receives the Domain `Match.Id` of its owning match.

### Champion data

Participants store the Riot champion ID.

`ChampionName` from the Riot representation is not copied into `MatchParticipant`.

Champion metadata is represented separately by the `Champion` Domain entity.

### Damage data

The Riot Application model contains both:

```text
TotalDamageDealt
TotalDamageDealtToChampions
```

The Domain participant currently stores:

```text
TotalDamageDealtToChampions
TotalDamageTaken
```

`TotalDamageDealt` is therefore intentionally not mapped into the Domain model.

## Item processing

Riot item IDs are added to `MatchParticipant` through the Domain `AddItem` behavior.

Processing rules are:

- negative item IDs are invalid
- item ID `0` represents an empty item slot and is ignored
- duplicate non-zero item IDs are allowed
- item ordering is preserved
- item IDs remain Riot IDs

A participant does not contain navigation properties to `Item` entities.

For example:

```text
Riot item slots:
[3071, 0, 3047, 0, 6333]

Domain ItemIds:
[3071, 3047, 6333]
```
Riot participant item IDs are processed through `MatchParticipant.AddItem`.

The processing rules are:

- positive Riot item IDs are preserved without conversion
- item ID `0` represents an empty inventory slot and is ignored
- multiple empty slots are allowed and ignored
- negative item IDs are invalid
- duplicate non-zero item IDs are allowed
- the ordering of non-zero item IDs is preserved
- an empty item collection is valid
- item processing does not affect rune processing

Item IDs remain Riot identifiers. Data Processing does not resolve them to `Item` entities or perform item metadata lookup.

## Rune processing

Riot rune IDs are added to `MatchParticipant` through the Domain `AddRune` behavior.

Processing rules are:

- rune IDs must be positive
- rune ID `0` is invalid
- negative rune IDs are invalid
- duplicate rune IDs are invalid
- duplicate rune IDs are rejected with `InvalidOperationException`
- rune IDs remain Riot IDs

A participant does not contain navigation properties to `Rune` entities.

## Validation

Data Processing relies on Domain constructors and Domain methods to enforce invariants.

Invalid external data must not silently create invalid Domain entities.

Examples include:

- missing participant PUUIDs
- invalid item IDs
- invalid rune IDs
- duplicate rune IDs
- invalid match chronology
- invalid Domain identifiers

When Riot data violates a Domain invariant, the transformation is expected to fail rather than bypass the invariant.

More detailed handling of incomplete or optional Riot data may be introduced by later Data Processing work.

## Duplicate detection

Duplicate match persistence is prevented using the Riot match ID.

Duplicate detection currently occurs as part of the match import workflow before a mapped match is persisted.

Data Processing must preserve `RiotMatchId` so that this identity remains available to the persistence/import pipeline.

Further consolidation of duplicate detection belongs to later Data Processing work.

## Responsibilities

### Game Data Acquisition

Responsible for:

- Riot API communication
- account lookup
- match history retrieval
- match details retrieval
- synchronization and import orchestration

### Data Processing

Responsible for:

- Riot-to-Domain transformation
- match normalization
- participant processing
- item processing
- rune processing
- transformation validation
- duplicate-data handling rules

### Statistics Engine

Responsible for calculations derived from Domain data, including:

- win/loss statistics
- win rate
- KDA
- CS
- CS/min
- gold statistics
- gold/min
- game duration statistics
- champion statistics

Statistical calculations do not belong in the Riot-to-Domain mapper.

## Testing

The Riot-to-Domain transformation contract is covered by Application tests for `RiotMatchDomainMapper`.

Coverage includes:

- match field mapping
- participant field mapping
- multiple participants
- nullable match completion fields
- item processing
- empty item slots
- duplicate items
- invalid item IDs
- rune processing
- invalid rune IDs
- duplicate rune IDs
- empty item and rune collections
- Domain validation propagation

### Match identity

Riot match identity is preserved during processing.

`RiotMatchId` is passed directly into the `Match` Domain entity. Missing, empty, or whitespace-only match IDs are rejected by the Domain.

Data Processing does not generate replacement identifiers or silently correct invalid Riot match identity values.

### Match chronology

Match timestamps are preserved from the Riot representation and passed into the Domain without being silently corrected.

The Domain enforces the following chronology:

```text
GameCreatedAt <= StartedAt
StartedAt <= EndedAt     (when EndedAt is present)
```

Therefore:

- game creation after game start is invalid
- game end before game start is invalid
- `EndedAt` may be absent
- invalid chronology causes transformation to fail

Data Processing does not reorder, replace, or manufacture timestamps to make malformed external data valid.

### Match duration

`RiotMatchDetails.Duration` is preserved when constructing the Domain `Match`.

Data Processing does not recalculate duration from `StartedAt` and `EndedAt`.

This is intentional because the Riot-provided duration is part of the external match representation and `EndedAt` may also be absent.

Valid duration values therefore follow:

```text
RiotMatchDetails.Duration
        ↓
Match.Duration
```

The full Riot game version is preserved exactly during Riot-to-Domain processing. Data Processing does not truncate the version to its major/minor components.

For example:

```text
Riot GameVersion: 16.15.123.4567
Domain GameVersion: 16.15.123.4567
Patch comparison: 16.15
```

Patch comparison is a Domain concern and does not modify the stored match version.

### Participant identity and ownership

Riot participant identity is preserved when constructing a Domain `MatchParticipant`.

The following values are mapped directly:

- `Puuid` → `PlayerPuuid`
- `ParticipantId` → `ParticipantId`
- `TeamId` → `TeamId`

Each participant receives the `Id` of the newly constructed Domain `Match`:

```text
Match.Id
   ↓
MatchParticipant.MatchId
```

Mapped participants are added through `Match.AddParticipant`.

This preserves `Match` aggregate ownership and ensures aggregate-level validation, including duplicate participant ID detection, is not bypassed.

Data Processing does not generate replacement participant identifiers or silently correct invalid participant identity data.

### Champion and position data

`ChampionId` is preserved as the Riot champion identifier.

Riot `ChampionName` is intentionally not copied into `MatchParticipant`. Champion metadata is represented separately by the `Champion` Domain entity.

`TeamPosition` is preserved according to the Riot representation.

Data Processing does not perform lane inference, role classification, or champion-statistics calculations.

### Participant statistics source data

Data Processing preserves the raw participant values required by later statistical analysis.

This includes:

- kills, deaths, and assists
- gold earned and gold spent
- total minions killed and neutral minions killed
- vision score
- wards placed and wards killed
- total damage dealt to champions
- total damage taken
- time played
- win/loss result

These values are stored as Domain data. Data Processing does not calculate derived statistics such as KDA, CS, CS/min, gold/min, or win rate.

Derived calculations belong to the Statistics Engine.

### Total damage processing

The Riot Application model contains both:

```text
TotalDamageDealt
TotalDamageDealtToChampions
```

`MatchParticipant` currently stores `TotalDamageDealtToChampions` but does not contain a `TotalDamageDealt` property.

Therefore, `TotalDamageDealt` is intentionally not mapped during Riot-to-Domain processing.

Changing Riot `TotalDamageDealt` must not affect the Domain participant's `TotalDamageDealtToChampions` or `TotalDamageTaken` values.

### Participant validation

Participant validation remains a Domain responsibility.

`RiotMatchDomainMapper` constructs `MatchParticipant` instances using Riot values and adds them through the `Match` aggregate.

If participant data violates an existing Domain invariant, the validation exception is allowed to propagate.

Data Processing does not:

- clamp invalid numeric values
- manufacture replacement PUUIDs
- replace participant identifiers
- bypass `Match.AddParticipant`
- suppress Domain validation failures

