using System;

namespace Janet.Contracts
{
    public class CreateSessionDTO
    {
        public Guid GameId { get; set; }
        public string SessionName { get; set; }
        public string PlayerName { get; set; }
        public string InitialState { get; set; }
    }
}