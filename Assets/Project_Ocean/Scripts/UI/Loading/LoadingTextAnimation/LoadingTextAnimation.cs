using UnityEngine;
using TMPro;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class LoadingTextAnimation : MonoBehaviour
    {
        [SerializeField] private TMP_Text loadingText;

        [SerializeField] private float dotInterval = 0.5f;

        private float timer;
        private int dotCount;

        private void Update()
        {
            timer += Time.unscaledDeltaTime;

            if (timer >= dotInterval)
            {
                timer = 0f;

                dotCount++;

                if (dotCount > 3)
                    dotCount = 0;

                loadingText.text = "Loading" + new string('.', dotCount);
            }
        }
    }
}