using System;

namespace Janet.Contracts
{
    public class JoinSessionDTO
    {
        public Guid SessionId { get; set; }
        public string PlayerName { get; set; }
    }
}