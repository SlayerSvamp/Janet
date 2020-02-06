using System;

namespace Janet.Contracts
{
    public class LeaveSessionDTO
    {
        public Guid SessionId { get; set; }
        public Guid PlayerId { get; set; }
    }
}