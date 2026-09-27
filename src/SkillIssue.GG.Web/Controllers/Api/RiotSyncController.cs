using Microsoft.AspNetCore.Mvc;
using SkillIssue.GG.Application.Riot.Interfaces;
using SkillIssue.GG.Web.Models.Api;

namespace SkillIssue.GG.Web.Controllers.Api;

[ApiController]
[Route("api/riot")]
public sealed class RiotSyncController(
    IRiotPlayerAndMatchSyncService syncService) : ControllerBase
{
    private readonly IRiotPlayerAndMatchSyncService _syncService = syncService;

    [HttpPost("sync")]
    [ProducesResponseType<RiotSyncResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RiotSyncResponse>> SyncAsync(
        [FromBody] RiotSyncRequest request)
    {
        var result = await _syncService.SyncAsync(
            request.GameName,
            request.TagLine,
            request.Region,
            request.Start,
            request.Count,
            HttpContext.RequestAborted);

        var response = new RiotSyncResponse(
            PlayerId: result.PlayerId,
            Puuid: result.Puuid,
            PlayerCreated: result.PlayerCreated,
            RequestedMatches: result.RequestedMatches,
            ImportedMatches: result.ImportedMatches,
            SkippedMatches: result.SkippedMatches);

        return Ok(response);
    }
}
