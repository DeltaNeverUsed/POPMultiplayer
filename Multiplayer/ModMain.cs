using MelonLoader;
using UnityEngine;

namespace MultiplayerMod
{
    public class ModMain : MelonMod
    {
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (buildIndex != 1)
                return;
        }
    }
}