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
            yield return new WaitForSeconds(2.75f);
            gameObject.SetActive(false);
        }
    }
}
