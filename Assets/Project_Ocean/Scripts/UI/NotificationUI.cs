using System.Collections;
using UnityEngine;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class NotificationUI : MonoBehaviour
    {
        void OnEnable()
        {
            StartCoroutine(FadeRoutine());
        }

        private IEnumerator FadeRoutine()
        {
            yield return new WaitForSeconds(1.25f);
            gameObject.SetActive(false);
        }
    }
}
