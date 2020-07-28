using BCA.Network.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstralBot.Helpers
{
    public class Command
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public PlayerRank Rank { get; set; }
        public bool PrivateMessage { get; set; }

        public override string ToString()
        {
            return (PrivateMessage ? "**[Commande privée]** - " : string.Empty) + Name + ":" + Description;
        }
    }
}
