# Statistics Engine

The Statistics Engine calculates derived statistics from normalized SkillIssue.GG Domain data.

Statistics calculations do not depend directly on Riot API DTOs, EF Core, or persistence-specific models.

## Player Match Statistics

`PlayerMatchStatisticsCalculator` calculates a player's performance for a single Match using `Match` and `MatchParticipant`.

The result includes:

- Match ID
- Riot Match ID
- Player PUUID
- Champion ID
- win/loss
- kills
- deaths
- assists
- KDA
- CS
- CS/min
- gold earned
- gold/min
- game duration

## KDA

KDA is calculated as:

(Kills + Assists) / Deaths

When deaths are zero, KDA is defined as:

Kills + Assists

This keeps the result finite and avoids division by zero.

No presentation-specific rounding is performed by the Statistics Engine.

## CS

CS is calculated as:

TotalMinionsKilled + NeutralMinionsKilled

Both lane minions and neutral monsters therefore contribute to the player's CS statistic.

## CS per minute

CS/min is calculated as:

CS / participant time played in minutes

`MatchParticipant.TimePlayed` is used rather than Match duration because per-minute player statistics describe the participant's actual persisted playing time.

The Domain guarantees that participant time played is greater than zero.

## Gold per minute

Gold/min is calculated as:

GoldEarned / participant time played in minutes

`MatchParticipant.TimePlayed` is used as the denominator.

The Domain guarantees that participant time played is greater than zero.

## Game duration

Game duration is taken directly from `Match.Duration`.

The Statistics Engine does not recalculate duration from Match timestamps.

## Validation

A `MatchParticipant` supplied to the calculator must belong to the supplied `Match`.

The following relationship must hold:

participant.MatchId == match.Id

A participant belonging to another Match is rejected.

Other raw-data invariants remain responsibilities of the Domain.

## Architecture

The calculation path is:

Match + MatchParticipant
        ↓
PlayerMatchStatisticsCalculator
        ↓
PlayerMatchStatistics

The Statistics Engine operates on normalized Domain data.

It does not depend on:

- Riot API models
- Riot API clients
- EF Core
- SkillIssueDbContext
- HTTP or Web response models

Data Processing is responsible for producing valid normalized Domain data. The Statistics Engine is responsible for deriving statistics from that data.

## Precision

Rate calculations use floating-point arithmetic.

The Statistics Engine does not apply UI-specific rounding or formatting. Presentation layers may format calculated values when required.

## Aggregate Player Statistics

`PlayerStatisticsCalculator` aggregates multiple `PlayerMatchStatistics` values belonging to the same Player.

The result includes:

- games played
- wins
- losses
- win rate
- total kills
- total deaths
- total assists
- aggregate KDA
- total CS
- average CS
- aggregate CS/min
- total gold earned
- average gold earned
- aggregate gold/min
- total game duration
- average game duration

### Win rate

Win rate is calculated as:

Wins / GamesPlayed

Win rate is represented as a ratio between `0.0` and `1.0`.

For example:

0.50 = 50%

The Statistics Engine does not convert the ratio to a percentage or apply presentation-specific formatting.

### Aggregate KDA

Aggregate KDA is calculated from aggregate combat totals:

(TotalKills + TotalAssists) / TotalDeaths

Individual Match KDA values are not averaged.

When total deaths are zero:

KDA = TotalKills + TotalAssists

This follows the same zero-death convention as per-Match statistics.

### CS

Total CS is calculated as:

sum of Match CS

Average CS is calculated as:

TotalCs / GamesPlayed

### Aggregate CS per minute

Aggregate CS/min is duration-weighted.

It is calculated as:

TotalCs / TotalParticipantMinutes

where:

TotalParticipantMinutes =
    sum of PlayerMatchStatistics.TimePlayed

Individual Match CS/min values are not averaged.

Using total participant time prevents short and long Matches from receiving equal weight when calculating the aggregate rate.

### Gold

Total gold earned is calculated as:

sum of Match GoldEarned

Average gold earned is calculated as:

TotalGoldEarned / GamesPlayed

### Aggregate gold per minute

Aggregate gold/min is duration-weighted:

TotalGoldEarned / TotalParticipantMinutes

Individual Match gold/min values are not averaged.

### Game duration

Total game duration is:

sum of Match GameDuration

Average game duration is:

TotalGameDuration / GamesPlayed

Game duration and participant time remain separate concepts.

`GameDuration` describes the Match.

`TimePlayed` describes the Player's persisted participation duration and is used for Player per-minute statistics.

### Player validation

All supplied Match statistics must belong to the same Player PUUID.

A collection containing statistics for different Players is rejected rather than combined.

### Duplicate Matches

Each Match may contribute at most once to an aggregate Player result.

Duplicate `MatchId` values are rejected to prevent the same Match from being counted multiple times.

### Empty input

Aggregate Player statistics require at least one Match.

An empty collection is rejected because the Statistics Engine cannot determine a Player identity or produce meaningful Player statistics without Match data.

### Precision

Ratios and averages use floating-point arithmetic.

The Statistics Engine does not perform UI-specific rounding.