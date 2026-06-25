using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    public class TipPanelUI : MonoBehaviour
    {
        [SerializeField] private GameObject[] tips;
        [SerializeField] private Button arrowRight;
        [SerializeField] private Button arrowLeft;
        [SerializeField] private Button closeButton;

        private int currentIndex;

        private void Awake()
        {
            arrowRight.onClick.AddListener(() => Navigate(1));
            arrowLeft.onClick.AddListener(() => Navigate(-1));
            closeButton.onClick.AddListener(Close);
            ShowTip(0);
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
            PlayerPrefs.Save();
            gameObject.SetActive(false);
        }
    }
}
