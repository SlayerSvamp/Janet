using System;
using System.Collections.Generic;
using System.Linq;
using Janet.Contracts;
using Microsoft.AspNetCore.Http;
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
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        public IActionResult GetGuid() => Ok(Guid.NewGuid().ToString());

        [HttpGet("sessions/{gameId}")]
        [ProducesResponseType(typeof(List<Session>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetSessions([FromQuery]Guid gameId)
        {
            var session = _context.Sessions
                .Include(session => session.Players)
                .FirstOrDefault(session => session.GameId == gameId);

            if (session == null)
                return NotFound();

            return Ok(session);
        }

        [HttpGet("sessions/{sessionId}")]
        [ProducesResponseType(typeof(Session), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetSession([FromQuery]Guid sessionId)
        {
            var session = _context.Sessions
                .Include(session => session.Players)
                .FirstOrDefault(session => session.SessionId == sessionId);

            if (session == null)
                return NotFound();

            return Ok(session);
        }

        [HttpPost("sessions")]
        [ProducesResponseType(typeof(Session), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult CreateSession([FromBody]CreateSessionDTO dto)
        {
            var game = _context.Games.Find(dto.GameId);
            if (game == null)
                return NotFound();

            var now = DateTime.Now;
            var session = new Session
            {
                Name = dto.SessionName,
                State = dto.InitialState,
                Created = now,
                Updated = now,
            };

            var player = new Player
            {
                Name = dto.PlayerName,
                Operator = true,
            };

            game.Sessions.Add(session);
            session.Players.Add(player);
            _context.SaveChanges();

            return Ok(session);
        }

        [HttpPut("sessions/state")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateSession([FromBody]UpdateSessionDTO dto)
        {
            var session = _context.Sessions.Find(dto.SessionId);
            if (session == null)
                return NotFound();

            session.State = dto.State;
            session.Updated = DateTime.Now;

            _context.SaveChanges();

            return Ok();
        }

        [HttpPost("sessions/join")]
        [ProducesResponseType(typeof(Session), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult JoinSession([FromBody]JoinSessionDTO dto)
        {
            if (dto.PlayerName.Length == 0)
                return BadRequest();

            var session = _context.Sessions
                .Include(session => session.Players)
                .FirstOrDefault(session => session.SessionId == dto.SessionId);

            if (session == null)
                return NotFound();

            session.Players.Add(new Player
            {
                Name = dto.PlayerName,
                Operator = false,
            });

            session.Updated = DateTime.Now;

            _context.SaveChanges();

            return Ok(session);
        }

        [HttpDelete("sessions/leave")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult LeaveSession([FromBody]LeaveSessionDTO dto)
        {
            var player = _context.Players
                .Where(player => player.SessionId == dto.SessionId)
                .SingleOrDefault(player => player.PlayerId == dto.PlayerId);

            if (player == null)
                return NotFound();

            _context.Players.Remove(player);
            _context.SaveChanges();

            return Ok();
        }
    }
}