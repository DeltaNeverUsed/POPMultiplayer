using System;
using Steamworks;
using UnityEngine;

namespace MultiplayerMod
{

    public class PlayerNetworkSender : MonoBehaviour
    {
        private void FixedUpdate()
        {
            if (!SteamIntegration.Enabled)
                return;
            
            UpdateNetwork();
        }

        private void UpdateNetwork()
        {
            var playerInfo = new PlayerInfo();
            playerInfo.position = transform.position;
            playerInfo.rotation = transform.rotation;
            
            SteamIntegration.SendObj2All(playerInfo);
        }
    }
}