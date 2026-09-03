namespace SkillIssue.GG.Domain.Entities;

public class MatchParticipant
{
    public Guid Id { get; private set; }

    public Guid MatchId { get; private set; }

    public string PlayerPuuid { get; private set; }

    public int ParticipantId { get; private set; }

    public int TeamId { get; private set; }

    public int ChampionId { get; private set; }

    public string ChampionName { get; private set; }

    public string TeamPosition { get; private set; }

    public int Kills { get; private set; }

    public int Deaths { get; private set; }

    public int Assists { get; private set; }

    public int GoldEarned { get; private set; }

    public int GoldSpent { get; private set; }

    public int TotalMinionsKilled { get; private set; }

    public int NeutralMinionsKilled { get; private set; }

    public int VisionScore { get; private set; }

    public int WardsPlaced { get; private set; }

    public int WardsKilled { get; private set; }

    public int TotalDamageDealtToChampions { get; private set; }

    public int TotalDamageTaken { get; private set; }

    public TimeSpan TimePlayed { get; private set; }

    public bool Won { get; private set; }

    public bool PlayedChampion(Champion champion)
    {
        ArgumentNullException.ThrowIfNull(champion);

        return ChampionId == champion.RiotChampionId;
    }
    private readonly List<int> _itemIds = [];

    public IReadOnlyCollection<int> ItemIds => _itemIds;

    private readonly List<int> _runeIds = [];

    public IReadOnlyCollection<int> RuneIds => _runeIds;

    private MatchParticipant()
    {
        PlayerPuuid = string.Empty;
        ChampionName = string.Empty;
        TeamPosition = string.Empty;
    }

    public MatchParticipant(
        Guid matchId,
        string playerPuuid,
        int participantId,
        int teamId,
        int championId,
        string championName,
        string teamPosition,
        int kills,
        int deaths,
        int assists,
        int goldEarned,
        int goldSpent,
        int totalMinionsKilled,
        int neutralMinionsKilled,
        int visionScore,
        int wardsPlaced,
        int wardsKilled,
        int totalDamageDealtToChampions,
        int totalDamageTaken,
        TimeSpan timePlayed,
        bool won)
    {
        if (matchId == Guid.Empty)
        {
            throw new ArgumentException(
                "Match ID is required.",
                nameof(matchId));
        }

        if (string.IsNullOrWhiteSpace(playerPuuid))
        {
            throw new ArgumentException(
                "Player PUUID is required.",
                nameof(playerPuuid));
        }

        if (participantId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(participantId));
        }

        if (teamId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(teamId));
        }

        if (championId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(championId));
        }

        if (string.IsNullOrWhiteSpace(championName))
        {
            throw new ArgumentException(
                "Champion name is required.",
                nameof(championName));
        }

        if (kills < 0 ||
            deaths < 0 ||
            assists < 0)
        {
            throw new ArgumentOutOfRangeException(
                "KDA values cannot be negative.");
        }

        if (goldEarned < 0 ||
            goldSpent < 0)
        {
            throw new ArgumentOutOfRangeException(
                "Gold values cannot be negative.");
        }

        if (totalMinionsKilled < 0 ||
            neutralMinionsKilled < 0)
        {
            throw new ArgumentOutOfRangeException(
                "Minion counts cannot be negative.");
        }

        if (visionScore < 0 ||
            wardsPlaced < 0 ||
            wardsKilled < 0)
        {
            throw new ArgumentOutOfRangeException(
                "Vision values cannot be negative.");
        }

        if (totalDamageDealtToChampions < 0 ||
            totalDamageTaken < 0)
        {
            throw new ArgumentOutOfRangeException(
                "Damage values cannot be negative.");
        }

        if (timePlayed <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timePlayed));
        }

        Id = Guid.NewGuid();
        MatchId = matchId;
        PlayerPuuid = playerPuuid;
        ParticipantId = participantId;
        TeamId = teamId;
        ChampionId = championId;
        ChampionName = championName;
        TeamPosition = teamPosition;
        Kills = kills;
        Deaths = deaths;
        Assists = assists;
        GoldEarned = goldEarned;
        GoldSpent = goldSpent;
        TotalMinionsKilled = totalMinionsKilled;
        NeutralMinionsKilled = neutralMinionsKilled;
        VisionScore = visionScore;
        WardsPlaced = wardsPlaced;
        WardsKilled = wardsKilled;
        TotalDamageDealtToChampions = totalDamageDealtToChampions;
        TotalDamageTaken = totalDamageTaken;
        TimePlayed = timePlayed;
        Won = won;
    }

    public void AddItem(int riotItemId)
    {
        if (riotItemId < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(riotItemId),
                "Riot item ID cannot be negative.");
        }

        if (riotItemId == 0)
        {
            return;
        }

        _itemIds.Add(riotItemId);
    }
    public void AddRune(int riotRuneId)
    {
        if (riotRuneId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(riotRuneId),
                "Riot rune ID must be greater than zero.");
        }

        if (_runeIds.Contains(riotRuneId))
        {
            throw new InvalidOperationException(
                "Rune has already been added to this participant.");
        }

        _runeIds.Add(riotRuneId);
    }
}
