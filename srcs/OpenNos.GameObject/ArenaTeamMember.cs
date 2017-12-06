using System;
using NosSharp.Enums;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject
{
    public class ArenaTeamMember
    {
        public ClientSession Session { get; set; }
        public ArenaTeamType ArenaTeamType { get; set; }
        public byte? Order { get; set; }
        public bool Dead { get; set; }
        public DateTime? LastSummoned { get; set; }
        public byte SummonCount { get; set; }

        public ArenaTeamMember(ClientSession session, ArenaTeamType arenaTeamType, byte? order)
        {
            Session = session;
            ArenaTeamType = arenaTeamType;
            Order = order;
        }
    }
}