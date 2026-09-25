using Microsoft.AspNetCore.Mvc;
using SkillIssue.GG.Application.Players.Interfaces;
using SkillIssue.GG.Web.Mapping;
using SkillIssue.GG.Web.Models.Players;

namespace SkillIssue.GG.Web.Controllers.Api;

[ApiController]
[Route("api/players")]
public sealed class PlayersController(IPlayerQueryService playerQueryService) : ControllerBase
{
    private readonly IPlayerQueryService _playerQueryService = playerQueryService;

    [HttpGet("{puuid}")]
    [ProducesResponseType<PlayerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerResponse>> GetAsync(
        string puuid,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(puuid))
        {
            return BadRequest("PUUID must not be empty.");
        }

        var player = await _playerQueryService.GetByPuuidAsync(
            puuid,
            cancellationToken);

        if (player is null)
        {
            return NotFound();
        }

        return Ok(PlayerResponseMapper.Map(player));
    }
}
