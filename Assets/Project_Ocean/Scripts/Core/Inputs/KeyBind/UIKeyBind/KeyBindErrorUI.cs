using System.Collections;
using TMPro;
using UnityEngine;

namespace Game.Core
{
    public class KeyBindErrorUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private float displayTime = 2f;

        private Coroutine hideCoroutine;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void Show(string message)
        {
            if (messageText == null)
                return;

            messageText.text = message;
            gameObject.SetActive(true);

            if (hideCoroutine != null)
                StopCoroutine(hideCoroutine);

            hideCoroutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(displayTime);

            gameObject.SetActive(false);
            hideCoroutine = null;
        }
    }
}