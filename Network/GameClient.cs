using AstralBot.Helpers;
using BCA.Common;
using BCA.Network;
using BCA.Network.Helpers;
using BCA.Network.Packets;
using BCA.Network.Packets.Enums;
using BCA.Network.Packets.Standard.FromClient;
using BCA.Network.Packets.Standard.FromServer;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AstralBot.Network
{
    public class GameClient : BinaryClient
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Bot _bot;
        private MessageParser _parser => _bot.MessageParser;

        public event Action LoginSuccess;

        public event Action<PlayerInfo> UpdateHubPlayer;
        public event Action<PlayerInfo> RemoveHubPlayer;

        public event Action<Animation[]> GetAnimations;

        public GameClient(Bot bot) : base(new NetworkClient())
        {
            PacketReceived += GameClient_PacketReceived;
            Disconnected += Client_Disconnected;

            _bot = bot;
        }
        public void StartConnexion()
        {
            logger.Info("Attempt of connexion... IP : " + GetIp());

            Connect(IPAddress.Parse(GetIp()), 9100);

            while (this.IsConnected)
            {
                Update();
                Thread.Sleep(1);
            }

            logger.Info("Disconnection...");
            Console.ReadLine();
        }

        public string GetIp()
        {
            if (_bot.BotConfig.TestMode)
                return Bot.test_ip;
#if DEBUG
            return Bot.debug_ip;
#else
            return Bot.release_ip;
#endif
        }


        private void GameClient_PacketReceived(BinaryReader reader)
        {
            PacketType packetType = (PacketType)reader.ReadInt16();
            int size = reader.ReadInt32();
            string packet = CompressHelper.Unzip(reader.ReadBytes(size));

            logger.Trace("PACKET RECEIVED - {0} : {1}", packetType, packet);

            try
            {
                switch (packetType)
                {
                    case PacketType.ChatMessage:
                        OnChatMessage(JsonConvert.DeserializeObject<StandardServerChatMessage>(packet));
                        break;
                    case PacketType.Login:
                        OnLogin(JsonConvert.DeserializeObject<StandardServerLogin>(packet));
                        break;
                    case PacketType.AddHubPlayer:
                        OnAddHubPlayer(JsonConvert.DeserializeObject<StandardServerAddHubPlayer>(packet));
                        break;
                    case PacketType.RemoveHubPlayer:
                        OnRemoveHubPlayer(JsonConvert.DeserializeObject<StandardServerRemoveHubPlayer>(packet));
                        break;
                    case PacketType.UpdateHubPlayer:
                        OnUpdateHubPlayer(JsonConvert.DeserializeObject<StandardServerUpdateHubPlayer>(packet));
                        break;
                    case PacketType.PlayerList:
                        OnUpdateHubPlayerList(JsonConvert.DeserializeObject<StandardServerPlayerlist>(packet));
                        break;
                    case PacketType.Ping:
                        OnPing(JsonConvert.DeserializeObject<StandardServerPing>(packet));
                        break;
                    case PacketType.AskAnimations:
                        OnGetAnimations(JsonConvert.DeserializeObject<StandardServerGetAnimations>(packet));
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }
        private void Client_Disconnected(Exception ex)
        {
            logger.Fatal("DISCONNECTED -" + ex.ToString());
            Console.ReadLine();
        }

        public void Send(PacketType packetId, Packet packet)
        {
            logger.Trace("PACKET SEND - {0} : {1}", packetId, packet);
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((short)packetId);
                    byte[] toSend = CompressHelper.Zip(JsonConvert.SerializeObject(packet));
                    writer.Write((int)toSend.Length);
                    writer.Write(toSend);
                }
                Send(stream.ToArray());
            }
        }
        public void Send(NetworkData data)
        {
            Send(data.Type, data.Packet);
        }

        public void OnPing(StandardServerPing packet)
        {
            this.Send(PacketType.Ping, new StandardClientPong { });
        }

        private void OnLogin(StandardServerLogin packet)
        {
            if (!packet.Success)
            {
                switch (packet.Reason)
                {
                    case LoginFailReason.Banned:
                        logger.Error(string.Format("Vous êtes banni jusqu'au {0}. Raison : {1}.", packet.EndSanction, packet.SanctionReason));
                        break;
                    case LoginFailReason.InvalidCombinaison:
                        logger.Error("La combinaison utilisateur/mot de passe est invalide.");
                        break;
                    case LoginFailReason.UsernameDoesntExist:
                        logger.Error("Le nom d'utilisateur n'existe pas.");
                        break;
                    case LoginFailReason.DisabledAccount:
                        logger.Error(string.Format("Votre compte est désactivé. Raison : {0}", packet.SanctionReason));
                        break;
                    case LoginFailReason.UserAlreadyConnected:
                        logger.Error("Quelqu'un est déja connecté sur votre compte. Tentez de vous reconnecter maintenant.");
                        break;
                    case LoginFailReason.Maintenance:
                        logger.Error("Une maintenance est en cours." + Environment.NewLine + "Raison: " + packet.MaintenanceReason + Environment.NewLine + "Temps estimé: " + packet.MaintenanceTimeEstimation.ToString() + "h.");
                        break;
                }
            }
            else
            {
                LoginSuccess?.Invoke();
            }

            logger.Info(_bot.BotConfig.Username + " connected to the server.");
        }
        private void OnChatMessage(StandardServerChatMessage packet)
        {

            ChatMessageType type = packet.Type;
            PlayerInfo sender = packet.Player;
            string msg = packet.Message;

            switch (type)
            {
                case ChatMessageType.Standard:
                    if (msg.StartsWith("!"))
                        _parser.ParseMessage(sender, msg.Substring(1));
                    break;
                case ChatMessageType.Animation:
                    break;
                case ChatMessageType.Information:
                    break;
                case ChatMessageType.Greet:
                    break;
                case ChatMessageType.Staff:
                    break;
                default:
                    break;
            }
        }

        private void OnAddHubPlayer(StandardServerAddHubPlayer packet)
        {
            UpdateHubPlayer?.Invoke(packet.Infos);
        }
        private void OnRemoveHubPlayer(StandardServerRemoveHubPlayer packet)
        {
            RemoveHubPlayer?.Invoke(packet.Infos);
        }
        private void OnUpdateHubPlayer(StandardServerUpdateHubPlayer packet)
        {
            UpdateHubPlayer?.Invoke(packet.Player);
        }
        private void OnUpdateHubPlayerList(StandardServerPlayerlist packet)
        {
            foreach (PlayerInfo infos in packet.Userlist)
                UpdateHubPlayer?.Invoke(infos);
        }

        public void OnGetAnimations(StandardServerGetAnimations packet)
        {
            GetAnimations?.Invoke(packet.Animations);
        }
    }
}
