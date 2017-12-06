using NosSharp.Enums;
using OpenNos.GameObject.Networking;

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