using System;
using Steamworks;
using UnityEngine;

namespace MultiplayerMod
{

    public class PlayerNetworkSender : MonoBehaviour
    {
        private void FixedUpdate()
        {
            if (!SteamIntegration.enabled)
                return;
            
            UpdateNetwork();
        }

        private void UpdateNetwork()
        {
            var playerInfo = new PlayerInfo();
            playerInfo.Position = transform.position;
            playerInfo.Rotation = transform.rotation;
            
            SteamIntegration.SendObj2All(playerInfo);
        }
    }
}