
using System.Linq;
using System.Threading.Tasks;
using MelonLoader;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace MultiplayerMod
{
    public static class SteamIntegration
    {
        private static Lobby _lobby;
        public static bool inLobby;
        public static bool enabled;
        
        public static void Init()
        {
            if (enabled)
                return;
            
            SteamClient.Init(2263010);

            MelonLogger.Msg(SteamClient.IsValid ? $"Name: {SteamClient.Name}" : $"Not valid");
            if (!SteamClient.IsValid)
                return;

            SteamNetworking.AllowP2PPacketRelay(true);

            SteamMatchmaking.OnLobbyMemberJoined += PlayerManager.OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberDisconnected += PlayerManager.OnLobbyMemberDisconnected;
            SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
            
            SteamNetworking.OnP2PSessionRequest += OnP2PSessionRequest;

            SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;

            enabled = true;
        }

        public static void ReadPackets()
        {
            while (SteamNetworking.IsP2PPacketAvailable())
            {
                var packet = SteamNetworking.ReadP2PPacket();
                if (packet == null)
                    continue;

                var user = packet.Value.SteamId;
                var obj = NetworkHelper.ByteArrayToObject(packet.Value.Data);
                var objType = obj.GetType();
                switch (objType.Name)
                {
                    case "PlayerInfo":
                        PlayerManager.UpdatePlayerPos(user.ToString(), (PlayerInfo)obj);
                        break;
                    case "String":
                        var str = (string)obj;
                        if (str != "fire")
                            continue;

                        StartLavaFlow_Patch.dontNetwork = true;
                        PlayerManager.Volcano.StartLavaFlow();
                        StartLavaFlow_Patch.dontNetwork = false;
                        break;
                }
                
            }
        }

        public static void SendObj2All<T>(T obj, P2PSend netType = P2PSend.Unreliable)
        {
            foreach (var friend in _lobby.Members)
            {
                if (friend.Id == SteamClient.SteamId)
                    continue;
                var data = NetworkHelper.ObjectToByteArray(obj);
                SteamNetworking.SendP2PPacket(friend.Id, data, data.Length, 0, netType);
            }
        }

        private static void OnP2PSessionRequest(SteamId steamId)
        {
            if (_lobby.Members.All(m => m.Id != steamId))
                return;

            SteamNetworking.AcceptP2PSessionWithUser(steamId);
        }

        public static void ToggleLobby()
        {
            if (inLobby)
            {
                foreach (var friend in _lobby.Members)
                {
                    SteamNetworking.CloseP2PSessionWithUser(friend.Id);
                }
                
                _lobby.Leave();
                inLobby = false;

                PlayerManager.LobbyLeft();
                MelonLogger.Msg("Disconnected from lobby");
                return;
            }

#pragma warning disable CS4014
            CreateLobby();
#pragma warning restore CS4014
        }

        public static async Task CreateLobby()
        {
            var tempLobby = await SteamMatchmaking.CreateLobbyAsync(10);
            if (tempLobby == null)
            {
                MelonLogger.Msg("Failed to create lobby");
                return;
            }
            
            _lobby = (Lobby)tempLobby;
            _lobby.SetFriendsOnly();
            _lobby.SetJoinable(true);

            inLobby = true;

            MelonLogger.Msg($"Created lobby: {_lobby.Id.Value}");
        }

        private static async void OnGameLobbyJoinRequested(Lobby lobby, SteamId steamId)
        {
            var result = await lobby.Join();
            if (result != RoomEnter.Success)
            {
                MelonLogger.Msg($"Failed to join lobby: {result}");
                return;
            }

            inLobby = true;
            _lobby = lobby;
        }

        private static void OnLobbyEntered(Lobby lobby)
        {
            MelonLogger.Msg($"Joined lobby: {_lobby.Id.Value}, Owner: {_lobby.Owner.Name}");

            foreach (var friend in lobby.Members)
            {
                PlayerManager.OnLobbyMemberJoined(_lobby, friend);
            }
        }

    }
}