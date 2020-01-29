using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Janet
{
    public class JanetContext : DbContext
    {
        public DbSet<Game> Games { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Player> Players { get; set; }

        public JanetContext(DbContextOptions<JanetContext> options)
            : base(options)
        { }
    }

    public class Game
    {
        public Guid GameId { get; set; }
        public string Name { get; set; }

        public List<Session> Sessions { get; } = new List<Session>();
    }

    public class Session
    {
        public Guid SessionId { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        // public bool AllowSpectators { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public Guid GameId { get; set; }
        public Game Game { get; set; }
        public List<Player> Players { get; } = new List<Player>();
    }

    public class Player
    {
        public Guid PlayerId { get; set; }
        public string Name { get; set; }
        public bool Operator { get; set; }

        public Guid SessionId { get; set; }
        public Session Session { get; set; }
    }
}