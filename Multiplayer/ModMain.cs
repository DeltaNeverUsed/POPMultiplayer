using System.Linq;
using MelonLoader;
using Steamworks;
using UnityEngine;
using HarmonyLib;
using MultiplayerMod;

namespace MultiplayerMod
{
    public class ModMain : MelonMod
    {
        public override void OnInitializeMelon()
        {
            var harmony = new HarmonyLib.Harmony("pineapple.DeltaNeverUsed.Multiplayer");
            
            var assembly = typeof(Fps).Assembly;
            
            var unlockAchieventMethod = assembly.GetType("SteamStatsAndAchievements").GetMethod("UnlockAchievent");
            harmony.Patch(unlockAchieventMethod, new HarmonyMethod(typeof(UnlockAchievent_Patch), "Prefix"));
            
            harmony.PatchAll();
        }

        
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (buildIndex != 1)
                return;
            
            PlayerManager.Init();
            SteamIntegration.Init();
        }

        public override void OnUpdate()
        {
            SteamClient.RunCallbacks();

            if (!SteamIntegration.enabled)
                return;
            
            SteamIntegration.ReadPackets();
            
            if (Input.GetKeyDown(KeyCode.L))
                SteamIntegration.ToggleLobby();
        }

        public override void OnDeinitializeMelon()
        {
            SteamClient.Shutdown();
        }
    }
}

public static class UnlockAchievent_Patch
{
    // make achievements work again
    private static void Prefix(string name)
    {
        MelonLogger.Msg("Achieve: "+name);
        var achievement = new Steamworks.Data.Achievement(name);
        achievement.Trigger();
        return;
    }
}

[HarmonyPatch(typeof(Scr_LavaController), "StartLavaFlow")]
public static class StartLavaFlow_Patch
{
    public static bool dontNetwork;
    private static void Prefix()
    {
        if (!dontNetwork)
            SteamIntegration.SendObj2All("fire", P2PSend.Reliable);
        
    }
}