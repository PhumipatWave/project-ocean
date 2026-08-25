using UnityEngine;

namespace Game.Core
{
    public class KeyBindTester : MonoBehaviour
    {
        private void Start()
        {
            KeyBindManager.Instance.StartRebind(
                KeyAction.Inventory,
                OnRebindComplete
            );
        }

        private void OnRebindComplete(string bindingPath)
        {
            Debug.Log(
                "Inventory New Binding: " +
                bindingPath
            );
        }
    }
}