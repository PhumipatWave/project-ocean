using UnityEngine;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class KeepUpright : MonoBehaviour
    {
        private void LateUpdate()
        {
            transform.rotation = Quaternion.identity;
        }
    }
}