using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    /// <summary>Tips window (UIWindow type Tips); UIManager opens it, pauses the game and closes it on Esc/B.</summary>
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
        }

        private void OnEnable() => ShowTip(0);

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
            // Through UIManager so the pause and player lock it applied are lifted too.
            if (UIManager.Instance != null && UIManager.Instance.CurrentUI == UIManager.UIType.Tips)
                UIManager.Instance.CloseCurrent();
            else
                gameObject.SetActive(false);
        }
    }
}
