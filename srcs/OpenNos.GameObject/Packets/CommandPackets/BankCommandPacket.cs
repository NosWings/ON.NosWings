using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;

namespace OpenNos.GameObject.Packets.CommandPackets
{
    [PacketHeader("$Bank")]
    public class BankCommandPacket
    {
        [PacketIndex(0)]
        public string Subcommand { get; set; }

        [PacketIndex(1)]
        public long? Amount { get; set; }

        public static string ReturnHelp()
        {
            return "$Bank [Withdraw/Deposit/Balance] <Amount>";
        }
    }
}
