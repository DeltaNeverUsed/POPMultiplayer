using System.Reflection;
using MelonLoader;
using Steamworks;
using UnityEngine;
using HarmonyLib;

namespace MultiplayerMod
{
    public class ModMain : MelonMod
    {
        public static Assembly _assembly;

        public override void OnInitializeMelon()
        {
            var harmony = new HarmonyLib.Harmony("pineapple.DeltaNeverUsed.Multiplayer");
            
            _assembly = typeof(AchievementTrigger).Assembly;

            var unlockAchieventMethod = _assembly.GetType("SteamStatsAndAchievements").GetMethod("UnlockAchievent");
            
            harmony.Patch(unlockAchieventMethod, new HarmonyMethod(typeof(UnlockAchievent_Patch).GetMethod("unlock_Prefix")));

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

            if (!SteamIntegration.Enabled)
                return;
            
            SteamIntegration.ReadPackets();
            
            if (Input.GetKeyDown(KeyCode.L))
                SteamIntegration.ToggleLobby();

            if (!Input.GetMouseButtonDown(0)) return;

            if (Camera.main == null || !Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward,
                    out var hit, 1)) return;
            
            bool parent = false;
            if(hit.transform.GetComponent<VillagerCreator>() == null)
                if (hit.transform.parent.GetComponent<VillagerCreator>() == null)
                    return;
                else
                    parent = true;

            var obj = parent ? hit.transform.parent.gameObject : hit.transform.gameObject;
            var indexOf = PlayerManager.Villagers.IndexOf(obj);
            if (indexOf == -1)
                return;

            SteamIntegration.SendObj2All(new PlayerSelect { model = indexOf }, P2PSend.Reliable);
            MelonLogger.Msg("Request Model Change to: "+obj.name);
        }

        public override void OnDeinitializeMelon()
        {
            SteamClient.Shutdown();
        }
    }
    
    // make achievements work again
    public static class UnlockAchievent_Patch
    {
        public static bool unlock_Prefix(string name)
        {
            MelonLogger.Msg("Achieve: "+name);
            var id = "";
            
            switch (name)
            {
                case "PLAY":
                    id = "Play";
                    break;
                case "ESCAPE":
                    id = "Escape";
                    break;
                case "THE_FLOOR_IS_LAVA":
                    id = "The floor is lava";
                    break;
                case "BABY":
                    id = "Baby";
                    break;
                case "KIDS_PLAYING":
                    id = "Kids playing";
                    break;
                case "BENCH":
                    id = "Bench";
                    break;
                case "SKILLED_DANCER":
                    id = "Skilled dancer";
                    break;
                case "BIRD_FRIEND":
                    id = "Bird friend";
                    break;
                case "PLANTATIONS":
                    id = "Plantations";
                    break;
                case "SAILBOAT":
                    id = "Sailboat";
                    break;
                case "LIMBO":
                    id = "Limbo";
                    break;
                case "STATUE":
                    id = "Statue";
                    break;
                case "CIRCLE":
                    id = "Circle";
                    break;
                case "CLIFF":
                    id = "Cliff";
                    break;
                case "JUMP":
                    id = "Jump";
                    break;
                case "TURTLE":
                    id = "Turtle";
                    break;
            }
            
            var achievement = new Steamworks.Data.Achievement(id);
            achievement.Trigger();
            
            return false;
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
}

