using OpenNos.Domain;

namespace OpenNos.GameObject
{
    public class ArenaMember
    {
        public ClientSession Session { get; set; }
        public long? GroupId { get; set; }
        public EventType ArenaType { get; set; }
        public int Time { get; set; }
    }
}