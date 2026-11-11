using UnityEngine;

namespace Game.Testing
{
    public class SaveTester : MonoBehaviour
    {
        void Update()
        {
            // ✅ เช็ค null ก่อน call เพื่อป้องกัน NullReferenceException
            if (Input.GetKeyDown(KeyCode.F5))
            {
                if (SaveBridge.Instance != null)
                    SaveBridge.Instance.SaveGame();
                else
                    Debug.LogError("[SaveTester] SaveBridge.Instance is null");
            }

            if (Input.GetKeyDown(KeyCode.F6))
            {
                if (SaveBridge.Instance != null)
                    SaveBridge.Instance.LoadGame();
                else
                    Debug.LogError("[SaveTester] SaveBridge.Instance is null");
            }
        }
    }
}
