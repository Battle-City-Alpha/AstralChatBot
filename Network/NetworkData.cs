using BCA.Network.Packets;
using BCA.Network.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstralBot.Network
{
    public class NetworkData
    {
        public PacketType Type { get; set; }
        public Packet Packet { get; set; }

        public NetworkData(PacketType type, Packet packet)
        {
            Type = type;
            Packet = packet;
        }
    }
}
