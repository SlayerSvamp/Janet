using System;

namespace Janet.Contracts
{
    public class UpdateSessionDTO
    {
        public Guid SessionId { get; set; }
        public string State { get; set; }
    }
}