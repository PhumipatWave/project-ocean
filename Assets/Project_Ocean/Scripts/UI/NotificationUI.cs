using System.Collections;
using UnityEngine;

namespace Game.Testing
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
