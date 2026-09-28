using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class TipPanelUI : MonoBehaviour
    {
        [SerializeField] private GameObject[] tips;
        [SerializeField] private Button arrowRight;
        [SerializeField] private Button arrowLeft;
        [SerializeField] private Button closeButton;

        private int currentIndex;
        private const string SeenKey = "HasSeenTips";

        private void Awake()
        {
            arrowRight.onClick.AddListener(() => Navigate(1));
            arrowLeft.onClick.AddListener(() => Navigate(-1));
            closeButton.onClick.AddListener(Close);
            ShowTip(0);

            if (PlayerPrefs.GetInt(SeenKey, 0) == 1)
                gameObject.SetActive(false);
        }

        private void Navigate(int dir)
        {
            ShowTip((currentIndex + dir + tips.Length) % tips.Length);
        }

        private void ShowTip(int index)
        {
            for (int i = 0; i < tips.Length; i++)
                tips[i].SetActive(i == index);
            currentIndex = index;
        }

        private void Close()
        {
            PlayerPrefs.SetInt(SeenKey, 1);
            PlayerPrefs.Save();
            gameObject.SetActive(false);
        }
    }
}
