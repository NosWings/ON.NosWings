using System.Collections.Generic;
using NosSharp.Logs;
using OpenNos.GameObject.Item.Instance;

namespace OpenNos.GameObject.Logs.Classes
{
    internal class TradeLoggable : AbstractLoggable
    {
        public TradeLoggable() : base("TradeLogs")
        {
        }

        internal class TradeCharacter
        {
            internal TradeCharacter(CharacterLog character)
            {
                Character = character;
            }

            public CharacterLog Character { get; }

            public long Gold { get; set; }

            public IEnumerable<ItemInstance> Items { get; set; }
        }

        public TradeCharacter FirstCharacter { get; set; }

        public TradeCharacter SecondCharacter { get; set; }
    }
}