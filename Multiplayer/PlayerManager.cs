using System;
using System.Collections.Generic;
using HarmonyLib;
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
    [Serializable]
    public class PlayerSelect
    {
        public int seed;
    }
    
    
    public static class PlayerManager
    {
        private static GameObject _playerObjects;

        private static List<GameObject> _villagers;

        public static Scr_LavaController Volcano;

        public static int playerSeed;

        public static void Init()
        {
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

            _villagers = new List<GameObject>();

            var temp = GameObject.FindObjectsOfType<VillagerCreator>();
            foreach (var villager in temp)
            {
                _villagers.Add(villager.gameObject);
            }

            foreach (var obj in roots)
            {
                if (obj.name == "FPSController")
                    obj.AddComponent<PlayerNetworkSender>();
                if (obj.name == "Island")
                    Volcano = obj.transform.Find("Volcano").Find("NewerLavaSetup").GetComponent<Scr_LavaController>();
            }
            
            if (_playerObjects != null)
                return;

            var rand = new System.Random();
            playerSeed = rand.Next();
            
            MelonLogger.Msg("Your seed is: "+playerSeed);

            _playerObjects = new GameObject("PlayerObjects");

            var sm = GameObject.Find("SteamManager");
            var g = ModMain._assembly.GetType("SteamStatsAndAchievements");
            var geez = sm.GetComponent(g);
            var field = Traverse.Create(geez).Field("instance");
            field.SetValue(geez);

            Object.DontDestroyOnLoad(_playerObjects);
        }

        public static void OnLobbyMemberJoined(Lobby lobby, Friend friend)
        {
            if (friend.Id == SteamClient.SteamId)
                return;
            
            MelonLogger.Msg($"Player Joined: {friend.Name}, Id: {friend.Id.ToString()}");
            
            var player = new GameObject(friend.Id.ToString());
            player.AddComponent<RemotePlayer>();
            
            player.transform.SetParent(_playerObjects.transform);

            SteamIntegration.SendObj(new PlayerSelect { seed = playerSeed }, friend, P2PSend.Reliable);
        }

        public static void SelectModel(string steamId, int seed)
        {
            foreach (Transform player in _playerObjects.transform)
            {
                if (player.name != steamId)
                    continue;
                
                foreach (Transform child in player) {
                    GameObject.Destroy(child.gameObject);
                }

                var rand = new System.Random(seed);
                var vil = _villagers[rand.Next(0, _villagers.Count)];

                var vilin = GameObject.Instantiate(vil, player, false);
                vilin.transform.position = new Vector3(0, -0.8329f, 0);
                vilin.transform.rotation = Quaternion.identity;

                var compSoundEffectsVillagerController = vilin.GetComponent<SoundEffectsVillagerController>();
                if (compSoundEffectsVillagerController != null)
                    compSoundEffectsVillagerController.enabled = false;
                
                var compRunawayVillagerController = vilin.GetComponent<RunawayVillagerController>();
                if (compRunawayVillagerController != null)
                    compRunawayVillagerController.enabled = false;
                
                var compShoutsController = vilin.GetComponent<ShoutsController>();
                if (compShoutsController != null)
                    compShoutsController.enabled = false;
                
                var compDeathVillagerController = vilin.GetComponent<DeathVillagerController>();
                if (compDeathVillagerController != null)
                    compDeathVillagerController.enabled = false;

                Object.Destroy(player);
                break;
            }
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