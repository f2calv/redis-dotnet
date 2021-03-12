using MessagePack;
using System;
namespace CasCap.Models
{
    [MessagePackObject(true)]
    public class MyObj
    {
        public Guid id { get; } = Guid.NewGuid();
        public DateTime date { get; } = DateTime.UtcNow;
        public string? str { get; set; }
    }
}