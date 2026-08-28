using UnityEngine;

namespace Game.Testing
{
    public class UILock : MonoBehaviour
    {
        private void LateUpdate()
        {
            transform.rotation = Quaternion.identity;
        }
    }
}