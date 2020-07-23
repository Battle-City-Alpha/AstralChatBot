using AstralBot.Announces;
using BCA.Common;
using BCA.Network.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AstralBot.Helpers
{
    public class MessageParser
    {
        private Bot _bot;
        private BotCommands _commands => _bot.Commands;

        public MessageParser(Bot bot)
        {
            _bot = bot;
        }

        public void ParseMessage(PlayerInfo sender, string msg)
        {
            string[] words = msg.Split(' ');
            switch (words[0])
            {
                case "bonjour":
                    HandleBonjour(sender);
                    break;
                case "k":
                case "kick":
                    HandleKick(words.Skip(1).ToArray(), sender);
                    break;
                case "seen":
                    HandleSeen(words.Skip(1).ToArray(), sender);
                    break;
                case "tirage":
                    HandleDraw(words.Skip(1).ToArray(), sender);
                    break;
                case "toss":
                    HandleToss(sender);
                    break;
                case "dice":
                    HandleDice(sender);
                    break;
                case "timerstart":
                    HandleTimerStart(words.Skip(1).ToArray(), sender);
                    break;
                case "timerstop":
                    HandleTimerStop(sender);
                    break;
                case "anim":
                    HandleAnimAsk(sender);
                    break;
            }
        }

        public void ParsePrivateMessage(PlayerInfo sender, string msg)
        {
            string[] words = msg.Split(' ');
            switch (words[0])
            {
                case "annonce":
                    HandleAnnounce(words.Skip(1).ToArray(), sender);
                    break;
                case "listeannonce":
                    HandleAnnouncesList(sender);
                    break;
                case "supannonce":
                    HandleDeleteAnnounce(words.Skip(1).ToArray(), sender);
                    break;
            }
        }

        public void HandleBonjour(PlayerInfo sender)
        {
            _commands.SendMessage(string.Format("Hello {0} !", sender.Username));
        }

        public void HandleKick(string[] msg, PlayerInfo sender)
        {
            if (sender.Rank < PlayerRank.Animateurs)
                return;
            PlayerInfo target = _bot.GetPlayer(msg[0]);
            if (target == null)
            {
                _commands.SendMessage(string.Format("{0} ne semble pas connecté...", msg[0]));
                return;
            }
            else
            {
                if (target.Rank >= sender.Rank)
                { _commands.SendKick(sender, string.Format("{0} calme toi...", sender.Username)); return; }
                string reason;
                if (msg.Length < 2)
                    reason = "Aucune.";
                else
                    reason = string.Join(" ", msg.Skip(1).ToArray());
                _commands.SendKick(target, reason);
            }
        }

        public void HandleSeen(string[] msg, PlayerInfo sender)
        {
            if (msg.Length != 1)
                return;
            DateTime seentime = _bot.SearchSeenDate(msg[0]);

            int d = (DateTime.Now - seentime).Days;
            int h = (DateTime.Now - seentime).Hours;
            int m = (DateTime.Now - seentime).Minutes;
            int s = (DateTime.Now - seentime).Seconds;
            string answer = string.Format("{0} a été appercu pour la dernière fois il y a", msg[0]);
            if (d != 0)
                answer += " " + d + "j" + h + "h" + m + "m";
            else if (h != 0)
                answer += " " + h + "h" + m + "m";
            else if (m != 0)
                answer += " " + m + "m";
            else
                answer += " " + s + "s";
            answer += ".";

            if (seentime == DateTime.Now)
                answer = string.Format("{0} est connecté !", msg[0]);
            else if (seentime.Year == 1)
                answer = string.Format("Je n'ai aucune nouvelle de {0}...", msg[0]);

            _commands.SendMessage(answer);
        }
        public void HandleAnimAsk(PlayerInfo sender)
        {
            _commands.SendAskAnimation();
        }

        public void HandleDice(PlayerInfo sender)
        {
            if (sender.Rank < PlayerRank.Animateurs)
                return;

            Random rd = new Random();

            _commands.SendMessage(rd.Next(1, 6).ToString());
        }
        public void HandleToss(PlayerInfo sender)
        {
            if (sender.Rank < PlayerRank.Animateurs)
                return;

            Random rd = new Random();
            int result = rd.Next(2);
            _commands.SendMessage(result == 0 ? "Face" : "Pile");
        }
        public void HandleDraw(string[] msg, PlayerInfo sender)
        {
            if (sender.Rank < PlayerRank.Animateurs)
                return;

            if (msg.Length < 2)
                return;

            Random rd = new Random();

            short n = -1;
            if (!short.TryParse(msg.Last(), out n))
                return;
            string answer = "Résultat du tirage au sort : ";
            List<int> alreadydraw = new List<int>();
            alreadydraw.Add(-1);
            int index = -1;
            for (int i = 0; i < n; i++)
            {
                while (alreadydraw.Contains(index))
                    index = rd.Next(msg.Length - 2);

                answer += msg[index] + " ";
                alreadydraw.Add(index);
            }

            _commands.SendMessage(answer);
        }
        public void HandleTimerStart(string[] msg, PlayerInfo sender)
        {
            if (sender.Rank < PlayerRank.Animateurs)
                return;

            if (msg.Length != 2)
                return;

            short m = 0;
            short s = 0;
            if (!short.TryParse(msg[0], out m) || !short.TryParse(msg[1], out s))
                return;

            TimeSpan interval = TimeSpan.FromMinutes(m) + TimeSpan.FromSeconds(s);
            _bot.SetTimer(interval);
        }
        public void HandleTimerStop(PlayerInfo sender)
        {
            if (sender.Rank < PlayerRank.Animateurs)
                return;
            if (!_bot.BotSetableTimer.Enabled)
                return;

            _bot.StopTimer();
        }

        public void HandleAnnounce(string[] msg, PlayerInfo sender)
        {
            if (sender.Rank < PlayerRank.Animateurs)
                return;

            if (msg.Length < 3)
                return;

            short repetition = -1;
            if (!short.TryParse(msg[0], out repetition))
                return;

            short interval = -1;
            if (!short.TryParse(msg[1], out interval))
                return;

            _bot.AnnouncesManager.CreateAnnounce(sender, string.Join(" ", msg.Skip(2)), repetition, interval);
            _commands.SendPrivateMessage(sender, "Ton annonce est créée !");
        }
        public void HandleAnnouncesList(PlayerInfo sender)
        {
            if (sender.Rank < PlayerRank.Animateurs)
                return;

            _commands.SendPrivateMessage(sender, "Liste des annonces : ");

            short index = 0;
            foreach (Announce a in _bot.AnnouncesManager.Announces)
            {
                _commands.SendPrivateMessage(sender, index + " - " + a.ToString());
                index++;
            }

            _commands.SendPrivateMessage(sender, "Fin de la liste des annonces.");
        }
        public void HandleDeleteAnnounce(string[] msg, PlayerInfo sender)
        {
            if (sender.Rank < PlayerRank.Animateurs)
                return;

            if (msg.Length > 1)
                return;

            short id = -1;
            if (!short.TryParse(msg[0], out id))
                return;
            _bot.AnnouncesManager.DeleteAnnounce(id);

            _commands.SendPrivateMessage(sender, "L'annonce a été supprimé !");
        }
    }
}
