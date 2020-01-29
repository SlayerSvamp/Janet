using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Janet.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly JanetContext _context;

        public ApiController(JanetContext context) => _context = context;

        private IActionResult SessionToModel(Session session)
        {
            return Ok(new
            {
                GameId = session.GameId,
                SessionId = session.SessionId,
                Name = session.Name,
                State = session.State,
                Created = session.Created,
                Updated = session.Updated,
                Players = session.Players.Select(player => new
                {
                    PlayerId = player.PlayerId,
                    Name = player.Name,
                    Op = player.Operator,
                }),
            });
        }

        [HttpGet("guid")]
        public string GetGuid() => Guid.NewGuid().ToString();

        [HttpGet("game/{gameId}/{sessionId}/{playerId}")]
        public IActionResult GetSession(Guid gameId, Guid sessionId, Guid playerId)
        {
            var session = _context.Sessions
                .Include(session => session.Players)
                .Where(session => session.GameId == gameId)
                .FirstOrDefault(session => session.SessionId == sessionId);

            if (session == null)
                return NotFound();

            if (/*!session.AllowSpectators && */ !session.Players.Any(player => player.PlayerId == playerId))
                return Unauthorized();

            return SessionToModel(session);
        }

        [HttpGet("game/new/{gameId}/{sessionName}/{playerName}")]
        public IActionResult CreateSession(Guid gameId, string sessionName, string playerName)
        {
            var game = _context.Games.Find(gameId);
            if (game == null)
                return NotFound("Game does not exist");

            var now = DateTime.Now;
            var session = new Session
            {
                Name = sessionName,
                Created = now,
                Updated = now,
            };

            var player = new Player
            {
                Name = playerName,
                Operator = true,
            };

            game.Sessions.Add(session);
            session.Players.Add(player);
            _context.SaveChanges();

            return SessionToModel(session);
        }

        [HttpGet("game/join/{gameId}/{sessionId}/{playerName}")]
        public IActionResult JoinSession(Guid gameId, Guid sessionId, string playerName)
        {
            if (playerName.Length == 0)
                return BadRequest();

            var session = _context.Sessions
                .Include(session => session.Players)
                .FirstOrDefault(session => session.GameId == gameId && session.SessionId == sessionId);

            if (session == null)
                return NotFound();

            session.Players.Add(new Player
            {
                Name = playerName,
                Operator = false,
            });

            session.Updated = DateTime.Now;

            _context.SaveChanges();

            return SessionToModel(session);
        }
    }
}