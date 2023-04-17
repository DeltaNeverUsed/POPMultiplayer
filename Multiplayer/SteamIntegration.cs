using System;
using MelonLoader;
using Steamworks;

namespace MultiplayerMod
{
    public static class SteamIntegration
    {
        public static uint gameAppId = 480;
        public static string currentName { get; set; } 
        public static SteamId currentId { get; set; }
        
        private static bool connectedToSteam = false;

        public static void init()
        {
            currentName = "";
            // Create client
            SteamClient.Init(gameAppId, true);

            if (!SteamClient.IsValid)
            {
                MelonLogger.Msg("Steam client not valid");
                throw new Exception();
            }

            currentName = SteamClient.Name;
            currentId = SteamClient.SteamId;
            connectedToSteam = true;
            MelonLogger.Msg("Steam initialized: " + currentName);
            //OpenNetworkChannels();
            //MelonLogger.Msg("Opened network channels");
            //MelonLogger.Instance.Start();
            SteamNetworking.AllowP2PPacketRelay(true);
        }
    }
}