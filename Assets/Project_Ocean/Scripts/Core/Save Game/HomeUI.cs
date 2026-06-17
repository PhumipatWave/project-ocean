using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    public class HomeUI : MonoBehaviour
    {
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton; // เพิ่ม

        void Start()
        {
            saveButton.onClick.AddListener(OnSaveClicked);
            loadButton.onClick.AddListener(OnLoadClicked); // เพิ่ม
        }

        void OnSaveClicked()
        {
            if (SaveBridge.Instance != null)
                SaveBridge.Instance.SaveGame();
            else
                Debug.LogError("[HomeUI] SaveBridge.Instance is null");
        }

        void OnLoadClicked() // เพิ่ม
        {
            if (SaveBridge.Instance != null)
                SaveBridge.Instance.LoadGame();
            else
                Debug.LogError("[HomeUI] SaveBridge.Instance is null");
        }
    }
}