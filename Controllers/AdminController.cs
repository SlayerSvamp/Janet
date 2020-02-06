using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Janet.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Janet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<AdminController> _logger;
        private readonly JanetContext _context;

        public AdminController(ILogger<AdminController> logger, JanetContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// This is my very special description
        /// </summary>
        /// <example>10</example>
        [HttpGet("games")]
        [ProducesResponseType(typeof(List<Game>), StatusCodes.Status200OK)]
        public IActionResult ListGames()
        {
            var games = _context.Games
                .Include(x => x.Sessions)
                    .ThenInclude(x => x.Players)
                .Select(game => new
                {
                    GameId = game.GameId,
                    Game = game.Name,
                    Sessions = game.Sessions.Select(session => new
                    {
                        SessionId = session.SessionId,
                        Name = session.Name,
                        Created = session.Created,
                        Players = session.Players.Select(player => new
                        {
                            PlayerId = player.PlayerId,
                            Name = player.Name,
                            Op = player.Operator,
                        }).ToList(),
                    }).ToList(),
                }).ToList();

            return Ok(games);
        }

        [HttpGet("games/{gameId}")]
        [ProducesResponseType(typeof(Game), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetGame([FromQuery]Guid gameId)
        {
            var game = _context.Games.Find(gameId);
            if (game == null)
                return NotFound();

            return Ok(game);
        }

        [HttpPost("games")]
        [ProducesResponseType(typeof(Game), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreateGame([FromBody]CreateGameDTO dto)
        {
            if (string.IsNullOrEmpty(dto.GameName))
                return BadRequest();

            var game = new Game { Name = dto.GameName };
            _context.Games.Add(game);
            _context.SaveChanges();

            return Ok(game);
        }

        [HttpDelete("games/{gameId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteGame([FromQuery]Guid gameId)
        {
            var game = _context.Games.Find(gameId);
            if (game == null)
                return NotFound();

            _context.Games.Remove(game);
            _context.SaveChanges();

            return Ok();
        }
    }
}
