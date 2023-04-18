using UnityEngine;

namespace MultiplayerMod
{
    public class RemotePlayer : MonoBehaviour
    {
        public Vector3 targetPos = Vector3.zero;
        public Quaternion targetRot = Quaternion.identity;

        private void Update()
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime*15);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime*15);
        }
    }
}