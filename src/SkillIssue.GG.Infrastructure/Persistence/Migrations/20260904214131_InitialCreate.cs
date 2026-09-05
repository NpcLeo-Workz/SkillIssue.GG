using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillIssue.GG.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "champions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RiotChampionId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_champions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RiotItemId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RiotMatchId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RiotGameId = table.Column<long>(type: "bigint", nullable: false),
                    DataVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GameVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GameMode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GameType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MapId = table.Column<int>(type: "integer", nullable: false),
                    QueueId = table.Column<int>(type: "integer", nullable: false),
                    PlatformId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GameCreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndOfGameResult = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "patches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataDragonVersion = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Puuid = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Region = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "runes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RiotRuneId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IconPath = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RuneTreeId = table.Column<int>(type: "integer", nullable: false),
                    RuneTreeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_runes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "match_participants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerPuuid = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParticipantId = table.Column<int>(type: "integer", nullable: false),
                    TeamId = table.Column<int>(type: "integer", nullable: false),
                    ChampionId = table.Column<int>(type: "integer", nullable: false),
                    TeamPosition = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Kills = table.Column<int>(type: "integer", nullable: false),
                    Deaths = table.Column<int>(type: "integer", nullable: false),
                    Assists = table.Column<int>(type: "integer", nullable: false),
                    GoldEarned = table.Column<int>(type: "integer", nullable: false),
                    GoldSpent = table.Column<int>(type: "integer", nullable: false),
                    TotalMinionsKilled = table.Column<int>(type: "integer", nullable: false),
                    NeutralMinionsKilled = table.Column<int>(type: "integer", nullable: false),
                    VisionScore = table.Column<int>(type: "integer", nullable: false),
                    WardsPlaced = table.Column<int>(type: "integer", nullable: false),
                    WardsKilled = table.Column<int>(type: "integer", nullable: false),
                    TotalDamageDealtToChampions = table.Column<int>(type: "integer", nullable: false),
                    TotalDamageTaken = table.Column<int>(type: "integer", nullable: false),
                    TimePlayed = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Won = table.Column<bool>(type: "boolean", nullable: false),
                    item_ids = table.Column<int[]>(type: "integer[]", nullable: false),
                    rune_ids = table.Column<int[]>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_match_participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_match_participants_matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_champions_Name",
                table: "champions",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_champions_RiotChampionId",
                table: "champions",
                column: "RiotChampionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_items_Name",
                table: "items",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_items_RiotItemId",
                table: "items",
                column: "RiotItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_match_participants_ChampionId",
                table: "match_participants",
                column: "ChampionId");

            migrationBuilder.CreateIndex(
                name: "IX_match_participants_MatchId_ParticipantId",
                table: "match_participants",
                columns: new[] { "MatchId", "ParticipantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_match_participants_PlayerPuuid",
                table: "match_participants",
                column: "PlayerPuuid");

            migrationBuilder.CreateIndex(
                name: "IX_matches_RiotGameId",
                table: "matches",
                column: "RiotGameId");

            migrationBuilder.CreateIndex(
                name: "IX_matches_RiotMatchId",
                table: "matches",
                column: "RiotMatchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_matches_StartedAt",
                table: "matches",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_patches_DataDragonVersion",
                table: "patches",
                column: "DataDragonVersion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_patches_Version",
                table: "patches",
                column: "Version",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_players_Name",
                table: "players",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_players_Puuid",
                table: "players",
                column: "Puuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_runes_Name",
                table: "runes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_runes_RiotRuneId",
                table: "runes",
                column: "RiotRuneId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_runes_RuneTreeId",
                table: "runes",
                column: "RuneTreeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "champions");

            migrationBuilder.DropTable(
                name: "items");

            migrationBuilder.DropTable(
                name: "match_participants");

            migrationBuilder.DropTable(
                name: "patches");

            migrationBuilder.DropTable(
                name: "players");

            migrationBuilder.DropTable(
                name: "runes");

            migrationBuilder.DropTable(
                name: "matches");
        }
    }
}
