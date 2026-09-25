using System.ComponentModel.DataAnnotations;

namespace SkillIssue.GG.Web.Models.Api;

public sealed record RiotSyncRequest(
    [Required]
    string GameName,

    [Required]
    string TagLine,

    [Required]
    string Region,

    [Range(0, int.MaxValue)]
    int Start = 0,

    [Range(1, 100)]
    int Count = 20);
