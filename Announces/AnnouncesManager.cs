using BCA.Common;
using BCA.Network.Packets.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace AstralBot.Announces
{
    public class AnnouncesManager
    {
        private Bot _bot;
        private BotCommands _commands => _bot.Commands;
        public List<Announce> Announces;

        public AnnouncesManager(Bot bot)
        {
            _bot = bot;
            Announces = new List<Announce>();
            ReadAnnounces();
        }

        public void CreateAnnounce(PlayerInfo creator, string txt, short repetition, short interval)
        {
            Announce a = new Announce(creator, txt, repetition, interval);
            a.SendAnnounce += AnnounceTick;
            a.EndAnnounce += AnnounceClosed;
            Announces.Add(a);
            SaveAnnounces();
        }

        private void AnnounceClosed(Announce a)
        {
            Announces.Remove(a);
            SaveAnnounces();
        }

        private void AnnounceTick(string announce)
        {
            _commands.SendMessage(announce, ChatMessageType.Information);
        }

        public void DeleteAnnounce(int id)
        {
            if (id < Announces.Count)
                Announces.RemoveAt(id);
            SaveAnnounces();
        }

        public void ReadAnnounces()
        {
            if (File.Exists("annonces.json"))
                Announces = JsonConvert.DeserializeObject<List<Announce>>(File.ReadAllText("annonces.json"));
        }

        public void SaveAnnounces()
        {
            File.WriteAllText("annonces.json", JsonConvert.SerializeObject(Announces));
        }
    }
}
