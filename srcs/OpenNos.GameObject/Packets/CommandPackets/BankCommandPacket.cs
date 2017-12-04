using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.CommandPackets
{
    [PacketHeader("$Bank")]
    public class BankCommandPacket
    {
        [PacketIndex(0)]
        public string Subcommand { get; set; }

        [PacketIndex(1)]
        public long? Amount { get; set; }

        [PacketIndex(2)]
        public string Target { get; set; }

        public static string ReturnHelp()
        {
            return "$Bank [Balance/Deposit/Top/Transfer/Withdraw] <Amount> <Target>";
        }
    }
}
