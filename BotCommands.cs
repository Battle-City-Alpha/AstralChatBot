using AstralBot.Configuration;
using AstralBot.Helpers;
using AstralBot.Network;
using BCA.Common;
using BCA.Network.Packets.Enums;
using BCA.Network.Packets.Standard.FromClient;
using System.Collections.Generic;
using System.IO;

namespace AstralBot
{
    public class BotCommands
    {
        private Bot _bot;
        private GameClient _client => _bot.Client;
        private BotConfig _config => _bot.BotConfig;

        private List<Command> _commands;

        public BotCommands(Bot bot)
        {
            _bot = bot;

            LoadCommands();
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
        public void SendPrivateMessage(PlayerInfo target, string msg)
        {
            _client.Send(PacketType.PrivateMessage, new StandardClientPrivateMessage
            {
                Message = msg,
                Target = target
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
        public void SendMute(PlayerInfo target, int time, string reason)
        {
            _client.Send(PacketType.Mute, new StandardClientMute
            {
                Reason = reason,
                Target = target,
                Time = time
            });
        }
        public void SendBan(PlayerInfo target, int time, string reason)
        {
            _client.Send(PacketType.Ban, new StandardClientBan
            {
                Reason = reason,
                Target = target.Username,
                Time = time
            });
        }

        public void SendAskAnimation()
        {
            _client.Send(PacketType.AskAnimations, new StandardClientAskAnimations { Offset = 0 });
        }

        public void SendAskRanking()
        {
            _client.Send(PacketType.GetRanking, new StandardClientGetRanking { SeasonOffset = 0 });
        }

        public void SendHelp(PlayerInfo target)
        {
            string msg = "Liste des commandes :";
            SendPrivateMessage(target, msg);

            foreach (Command c in _commands)
                if (c.Rank < target.Rank)
                    SendPrivateMessage(target, c.ToString());
        }

        private void LoadCommands()
        {
            _commands = new List<Command>();

            AddCommand("!bonjour", "Le robot vous saluera.", PlayerRank.Joueurs);
            AddCommand("!anim", "Permet de connaître les animations du jour.", PlayerRank.Joueurs);
            AddCommand("!seen [joueur]", "Permet de connaître la dernière connexion d'un joueur.", PlayerRank.Joueurs);
            AddCommand("!ranking", "Permet de connaître le classement des trois premiers joueurs.", PlayerRank.Joueurs);

            AddCommand("!draw [joueur1] ... [joueurN] [nombre]", "Permet de tirer [nombre] joueurs parmis la liste transmise.", PlayerRank.Animateurs);
            AddCommand("!toss", "Effectue un pile ou face.", PlayerRank.Animateurs);
            AddCommand("!dice", "Effectue un lancé de dés.", PlayerRank.Animateurs);
            AddCommand("!timerstart [minutes] [secondes]", "Permet de lancer un timer.", PlayerRank.Animateurs);
            AddCommand("!timerstop", "Arrête le timer.", PlayerRank.Animateurs);
            AddCommand("!annonce [repetitions] [interval] [texte]", "Permet de faire une annonce de façon répétée toutes les [interval] minutes [repetitions] fois par le bot.", PlayerRank.Animateurs, true);
            AddCommand("!listeannonce", "Permet d'obtenir la liste des annonces.", PlayerRank.Animateurs, true);
            AddCommand("!supannonce [id]", "Permet de supprimer une annonce via son id, pour le connaître utiliser !listeannonce.", PlayerRank.Animateurs, true);
        }
        private void AddCommand(string name, string description, PlayerRank rank, bool privatemessage = false)
        {
            _commands.Add(new Command
            {
                Name = name,
                Description = description,
                Rank = rank,
                PrivateMessage = privatemessage
            });
        }
    }
}
