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