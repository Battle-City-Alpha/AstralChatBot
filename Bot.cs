using AstralBot.Announces;
using AstralBot.Configuration;
using AstralBot.Helpers;
using AstralBot.Network;
using BCA.Common;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace AstralBot
{
    public class Bot
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        //public static string debug_ip = "127.0.0.1";
        public static string debug_ip = "185.212.225.85";
        public static string test_ip = "185.212.226.12";
        public static string release_ip = "185.212.225.85";


        public BotConfig BotConfig { get; set; }
        public BotCommands Commands { get; set; }
        public MessageParser MessageParser { get; set; }
        public GameClient Client { get; set; }
        public AnnouncesManager AnnouncesManager { get; set; }

        public Dictionary<int, PlayerInfo> PlayersConnected { get; set; }
        public Dictionary<string, DateTime> SeenTime { get; set; }

        public Timer BotSetableTimer;

        public Animation[] AnimationsOfTheDay { get; set; }

        public Bot()
        {
            LoadConfig();
            LoadSeenTime();

            Commands = new BotCommands(this);
            MessageParser = new MessageParser(this);
            AnnouncesManager = new AnnouncesManager(this);

            Client = new GameClient(this);

            Client.Connected += Client_Connected;
            Client.LoginSuccess += Client_LoginSuccess;
            Client.UpdateHubPlayer += UpdatePlayer;
            Client.RemoveHubPlayer += RemovePlayer;
            Client.GetAnimations += GetAnimations;

            PlayersConnected = new Dictionary<int, PlayerInfo>();

            Client.StartConnexion();
        }

        private void Client_LoginSuccess()
        {

        }

        private void GetAnimations(Animation[] animations)
        {
            string answer = "Animations du jour : ";
            string permaanim = "Animations permanentes : ";
            animations = animations.OrderBy(x => x.StartDate).ToArray();
            foreach (Animation anim in animations)
            {
                if (anim.StartDate.Day == DateTime.Now.Day && anim.StartDate.Month == DateTime.Now.Month && anim.StartDate.Year == DateTime.Now.Year)
                    answer += "** " + anim.Name + " ** (" + anim.StartDate.ToString("HH:mm") + "), ";
                if (anim.Duration == -1)
                    permaanim += "** " + anim.Name + " ** , ";
            }
            answer = answer.Substring(0, answer.Length - 2) + ".";

            permaanim = permaanim.Substring(0, permaanim.Length - 3) + ".";
            string gofurther = "Pour plus d'informations, consultez le planning via le bouton * Animations * !";

            Commands.SendMessage(answer);
            //Commands.SendMessage(permaanim);
            Commands.SendMessage(gofurther);
        }

        private void Client_Connected()
        {
            Commands.SendAuthentification();
        }

        private void LoadConfig()
        {
            if (File.Exists(Path.Combine(Program.path, "config.json")))
                BotConfig = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText(Path.Combine(Program.path, "config.json")));
            else
                BotConfig = new BotConfig();

            logger.Info("Config loaded.");
            SaveConfig();
        }
        public void SaveConfig()
        {
            File.WriteAllText(Path.Combine(Program.path, "config.json"), JsonConvert.SerializeObject(BotConfig));
            logger.Info("Config saved.");
        }
        private void LoadSeenTime()
        {
            if (File.Exists(Path.Combine(Program.path, "seentime.json")))
                SeenTime = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(File.ReadAllText(Path.Combine(Program.path, "seentime.json")));
            else
                SeenTime = new Dictionary<string, DateTime>();
            logger.Info("SeenTime loaded.");
            SaveSeenTime();
        }
        public void SaveSeenTime()
        {
            File.WriteAllText(Path.Combine(Program.path, "seentime.json"), JsonConvert.SerializeObject(SeenTime));
            logger.Info("SeenTime saved.");
        }

        public void UpdatePlayer(PlayerInfo infos)
        {
            if (PlayersConnected.ContainsKey(infos.UserId))
                PlayersConnected[infos.UserId] = infos;
            else
                PlayersConnected.Add(infos.UserId, infos);
        }
        public void RemovePlayer(PlayerInfo infos)
        {
            if (PlayersConnected.ContainsKey(infos.UserId))
            {
                if (!SeenTime.ContainsKey(infos.Username.ToUpper()))
                    SeenTime.Add(infos.Username.ToUpper(), DateTime.Now);
                else
                    SeenTime[infos.Username.ToUpper()] = DateTime.Now;
                PlayersConnected.Remove(infos.UserId);
                SaveSeenTime();
            }
        }
        public PlayerInfo GetPlayer(string username)
        {
            foreach (var info in PlayersConnected)
                if (info.Value.Username.ToUpper() == username.ToUpper())
                    return info.Value;
            return null;
        }

        public DateTime SearchSeenDate(string username)
        {
            if (GetPlayer(username) != null)
                return DateTime.Now;

            if (SeenTime.ContainsKey(username.ToUpper()))
                return SeenTime[username.ToUpper()];
            else
                return new DateTime(1, 1, 1);
        }

        public void SetTimer(TimeSpan interval)
        {
            if (BotSetableTimer != null && BotSetableTimer.Enabled)
                BotSetableTimer.Enabled = false;

            BotSetableTimer = new Timer();
            BotSetableTimer.Interval = interval.TotalMilliseconds;
            BotSetableTimer.Elapsed += BotSetableTimer_Elapsed;
            BotSetableTimer.Start();

            Commands.SendMessage("Un timer de " + interval.Minutes + "m" + interval.Seconds + "s est lancé !");
        }
        private void BotSetableTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            StopTimer();
        }
        public void StopTimer()
        {
            Commands.SendMessage("Le temps est écoulé !");
            BotSetableTimer.Enabled = false;
        }
    }
}
