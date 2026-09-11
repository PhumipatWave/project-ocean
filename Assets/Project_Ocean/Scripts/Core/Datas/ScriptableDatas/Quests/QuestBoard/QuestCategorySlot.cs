using UnityEngine;
using UnityEngine.UI;
using System;

namespace Game.Testing
{
    [RequireComponent(typeof(Button))]
    public class QuestCategorySlot : MonoBehaviour
    {
        private Button button;

        private QuestCategory currentCategory;
        private Action<QuestCategory> onClicked;

        private void Awake()
        {
            button = GetComponent<Button>();

            button.onClick.AddListener(HandleClick);
        }

        public void Setup(
            QuestCategory category,
            Action<QuestCategory> callback)
        {
            currentCategory = category;
            onClicked = callback;

            gameObject.SetActive(true);
        }

        public void Clear()
        {
            gameObject.SetActive(false);

            onClicked = null;
        }

        private void HandleClick()
        {
            onClicked?.Invoke(currentCategory);
        }
    }
}