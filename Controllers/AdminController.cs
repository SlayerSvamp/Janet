using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Janet.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<AdminController> _logger;
        private readonly JanetContext _context;

        public AdminController(ILogger<AdminController> logger, JanetContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("game")]
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

        [HttpGet("game/{id}")]
        public IActionResult GetGame(Guid id)
        {
            var game = _context.Games.Find(id);
            if (game != null)
                return Ok(game);
            return NotFound();
        }

        [HttpGet("game/create/{name}")]
        public IActionResult CreateGame(string name)
        {
            if (name != null && name.Length > 0)
            {
                var game = new Game { Name = name };
                _context.Games.Add(game);
                _context.SaveChanges();

                return Ok(game);
            }
            return BadRequest("Game must have a name");
        }

        [HttpGet("game/delete/{id}")]
        public IActionResult DeleteGame(Guid id)
        {
            var game = _context.Games.Find(id);
            if (game != null)
            {
                _context.Games.Remove(game);
                _context.SaveChanges();
                return Ok();
            }
            return NotFound();
        }
    }
}
