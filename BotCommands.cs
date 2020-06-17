using AstralBot.Configuration;
using AstralBot.Network;
using BCA.Common;
using BCA.Network.Packets.Enums;
using BCA.Network.Packets.Standard.FromClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstralBot
{
    public class BotCommands
    {
        private Bot _bot;
        private GameClient _client => _bot.Client;
        private BotConfig _config => _bot.BotConfig;

        public BotCommands(Bot bot)
        {
            _bot = bot;
        }

        public void SendAuthentification()
        {
            string encryptKey = File.ReadAllText("rsa_publickey.xml");
            _client.Send(PacketType.Login, new StandardClientLogin
            {
                Username = _config.Username,
                Password = CryptoManager.Encryption(_config.Password, encryptKey),
                HID = ""
            });
        }

        public void SendMessage(string msg, ChatMessageType cmt = ChatMessageType.Standard)
        {
            _client.Send(PacketType.ChatMessage, new StandardClientChatMessage
            {
                Message = msg,
                Type = cmt
            });
        }

        public void SendKick(PlayerInfo target, string reason)
        {
            _client.Send(PacketType.Kick, new StandardClientKick
            {
                Reason = reason,
                Target = target
            });
        }

        public void SendAskAnimation()
        {
            _client.Send(PacketType.AskAnimations, new StandardClientAskAnimations { Offset = 0 });
        }
    }
}
