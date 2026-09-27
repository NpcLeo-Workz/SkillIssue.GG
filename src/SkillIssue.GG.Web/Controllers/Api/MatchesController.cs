using Microsoft.AspNetCore.Mvc;
using SkillIssue.GG.Application.Matches.Interfaces;
using SkillIssue.GG.Web.Mapping;
using SkillIssue.GG.Web.Models.Matches;

namespace SkillIssue.GG.Web.Controllers.Api;

[ApiController]
[Route("api/matches")]
public sealed class MatchesController : ControllerBase
{
    private readonly IMatchQueryService _matchQueryService;

    public MatchesController(IMatchQueryService matchQueryService)
    {
        _matchQueryService = matchQueryService;
    }

    [HttpGet("{riotMatchId}")]
    [ProducesResponseType<PlayerMatchResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerMatchResponse>> GetByRiotMatchId(
        string riotMatchId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(riotMatchId))
        {
            return BadRequest();
        }

        var match = await _matchQueryService.GetByRiotMatchIdAsync(
            riotMatchId,
            cancellationToken);

        if (match is null)
        {
            return NotFound();
        }

        return Ok(PlayerMatchResponseMapper.Map(match));
    }
}
