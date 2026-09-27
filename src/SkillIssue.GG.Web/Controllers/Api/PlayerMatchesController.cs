using Microsoft.AspNetCore.Mvc;
using SkillIssue.GG.Application.Matches.Interfaces;
using SkillIssue.GG.Web.Mapping;
using SkillIssue.GG.Web.Models.Matches;

namespace SkillIssue.GG.Web.Controllers.Api;

[ApiController]
[Route("api/players/{puuid}/matches")]
public sealed class PlayerMatchesController(
    IPlayerMatchHistoryService playerMatchHistoryService) : ControllerBase
{
    private readonly IPlayerMatchHistoryService _playerMatchHistoryService = playerMatchHistoryService;

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PlayerMatchResponse>>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PlayerMatchResponse>>> GetAsync(
        string puuid,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        if (string.IsNullOrWhiteSpace(puuid))
        {
            return BadRequest("PUUID must not be empty.");
        }

        if (skip < 0)
        {
            return BadRequest("Skip must be zero or greater.");
        }

        if (take is < 1 or > 100)
        {
            return BadRequest("Take must be between 1 and 100.");
        }

        var matches = await _playerMatchHistoryService.GetAsync(
            puuid,
            skip,
            take,
            HttpContext.RequestAborted);

        var response = matches
            .Select(PlayerMatchResponseMapper.Map)
            .ToArray();

        return Ok(response);
    }
}
