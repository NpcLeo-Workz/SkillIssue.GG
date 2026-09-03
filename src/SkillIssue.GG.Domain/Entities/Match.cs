namespace SkillIssue.GG.Domain.Entities;

public class Match
{
    public Guid Id { get; private set; }

    public string RiotMatchId { get; private set; }

    public long RiotGameId { get; private set; }

    public string DataVersion { get; private set; }

    public string GameVersion { get; private set; }

    public string GameMode { get; private set; }

    public string GameType { get; private set; }

    public int MapId { get; private set; }

    public int QueueId { get; private set; }

    public string PlatformId { get; private set; }

    public DateTimeOffset GameCreatedAt { get; private set; }

    public DateTimeOffset StartedAt { get; private set; }

    public DateTimeOffset? EndedAt { get; private set; }

    public TimeSpan Duration { get; private set; }

    public string? EndOfGameResult { get; private set; }
    private readonly List<MatchParticipant> _participants = [];
    public IReadOnlyCollection<MatchParticipant> Participants => _participants;

    private Match()
    {
        RiotMatchId = string.Empty;
        DataVersion = string.Empty;
        GameVersion = string.Empty;
        GameMode = string.Empty;
        GameType = string.Empty;
        PlatformId = string.Empty;
    }

    public Match(
        string riotMatchId,
        long riotGameId,
        string dataVersion,
        string gameVersion,
        string gameMode,
        string gameType,
        int mapId,
        int queueId,
        string platformId,
        DateTimeOffset gameCreatedAt,
        DateTimeOffset startedAt,
        DateTimeOffset? endedAt,
        TimeSpan duration,
        string? endOfGameResult)
    {
        if (string.IsNullOrWhiteSpace(riotMatchId))
        {
            throw new ArgumentException(
                "Riot match ID is required.",
                nameof(riotMatchId));
        }

        if (riotGameId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(riotGameId),
                "Riot game ID must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(dataVersion))
        {
            throw new ArgumentException(
                "Data version is required.",
                nameof(dataVersion));
        }

        if (string.IsNullOrWhiteSpace(gameVersion))
        {
            throw new ArgumentException(
                "Game version is required.",
                nameof(gameVersion));
        }

        if (string.IsNullOrWhiteSpace(gameMode))
        {
            throw new ArgumentException(
                "Game mode is required.",
                nameof(gameMode));
        }

        if (string.IsNullOrWhiteSpace(gameType))
        {
            throw new ArgumentException(
                "Game type is required.",
                nameof(gameType));
        }

        if (mapId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(mapId),
                "Map ID must be greater than zero.");
        }

        if (queueId < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(queueId),
                "Queue ID cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(platformId))
        {
            throw new ArgumentException(
                "Platform ID is required.",
                nameof(platformId));
        }

        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duration),
                "Match duration must be greater than zero.");
        }

        if (endedAt.HasValue && endedAt.Value < startedAt)
        {
            throw new ArgumentException(
                "Match end time cannot be before the start time.",
                nameof(endedAt));
        }

        Id = Guid.NewGuid();
        RiotMatchId = riotMatchId;
        RiotGameId = riotGameId;
        DataVersion = dataVersion;
        GameVersion = gameVersion;
        GameMode = gameMode;
        GameType = gameType;
        MapId = mapId;
        QueueId = queueId;
        PlatformId = platformId;
        GameCreatedAt = gameCreatedAt;
        StartedAt = startedAt;
        EndedAt = endedAt;
        Duration = duration;
        EndOfGameResult = endOfGameResult;
    }
    public bool WasPlayedOnPatch(Patch patch)
    {
        ArgumentNullException.ThrowIfNull(patch);

        var parts = GameVersion.Split('.');

        if (parts.Length < 2)
        {
            return false;
        }

        var matchPatchVersion = $"{parts[0]}.{parts[1]}";

        return string.Equals(
            matchPatchVersion,
            patch.Version,
            StringComparison.Ordinal);
    }
    public void AddParticipant(MatchParticipant participant)
    {
        ArgumentNullException.ThrowIfNull(participant);

        if (participant.MatchId != Id)
        {
            throw new ArgumentException(
                "Participant belongs to a different match.",
                nameof(participant));
        }

        if (_participants.Any(x => x.ParticipantId == participant.ParticipantId))
        {
            throw new InvalidOperationException(
                "Participant has already been added to this match.");
        }

        _participants.Add(participant);
    }
}
