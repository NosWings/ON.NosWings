using System;
using System.Collections.Generic;

namespace OpenNos.GameObject.Logs.Classes
{
    internal class TradeLog
    {   
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

        public TradeLog()
        {
            Date = DateTime.Now;
        }

        public DateTime Date { get; }

        public TradeCharacter FirstCharacter { get; set; }

        public TradeCharacter SecondCharacter { get; set; }
    }
}