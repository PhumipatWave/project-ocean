using UnityEngine;

namespace Game.Testing
{
    public class SaveTester : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
                SaveBridge.Instance.SaveGame();

            if (Input.GetKeyDown(KeyCode.F9))
                SaveBridge.Instance.LoadGame();
        }
    }
}