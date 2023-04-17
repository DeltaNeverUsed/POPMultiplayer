using System;
using MelonLoader;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MultiplayerMod
{
    [Serializable]
    public class PlayerInfo
    {
        public SerializableVector3 Position;
        public SerializableQuaternion Rotation;
    }
    
    
    public static class PlayerManager
    {
        private static GameObject _playerObjects;

        public static Scr_LavaController Volcano;

        public static void Init()
        {
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

            foreach (var obj in roots)
            {
                if (obj.name == "FPSController")
                    obj.AddComponent<PlayerNetworkSender>();
                if (obj.name == "Island")
                    Volcano = obj.transform.Find("Volcano").Find("NewerLavaSetup").GetComponent<Scr_LavaController>();
            }
            
            if (_playerObjects != null)
                return;

            _playerObjects = new GameObject("PlayerObjects");
            Object.DontDestroyOnLoad(_playerObjects);
        }

        public static void OnLobbyMemberJoined(Lobby lobby, Friend friend)
        {
            if (friend.Id == SteamClient.SteamId)
                return;
            
            MelonLogger.Msg($"Player Joined: {friend.Name}, Id: {friend.Id.ToString()}");
            
            var player = GameObject.CreatePrimitive(PrimitiveType.Cube);
            player.AddComponent<RemotePlayer>();
            
            player.transform.SetParent(_playerObjects.transform);
            player.name = friend.Id.ToString();
        }
        public static void OnLobbyMemberDisconnected(Lobby lobby, Friend friend)
        {
            MelonLogger.Msg($"Player Disconnected: {friend.Name}");
            var id = friend.Id.ToString();
            
            foreach (Transform player in _playerObjects.transform)
            {
                if (player.name != id)
                    continue;
                
                Object.Destroy(player);
                break;
            }
        }

        public static void LobbyLeft()
        {
            foreach (Transform player in _playerObjects.transform)
            {
                Object.Destroy(player);
            }
        }

        public static void UpdatePlayerPos(string id, PlayerInfo playerInfo)
        {
            foreach (Transform player in _playerObjects.transform)
            {
                if (player.name != id)
                    continue;

                var rPlayer = player.GetComponent<RemotePlayer>(); // I know this is really bad, but i'm lazy
                rPlayer.targetPos = playerInfo.Position;
                rPlayer.targetRot = playerInfo.Rotation;
            }
        }
    }
}