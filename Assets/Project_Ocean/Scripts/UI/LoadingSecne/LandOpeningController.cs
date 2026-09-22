using System.Collections;
using UnityEngine;

namespace Game.Testing
{
    public class LandOpeningController : MonoBehaviour
    {
        [SerializeField] private Animator cameraAnimator;
        [SerializeField] private GameObject player;

        [SerializeField] private float introDuration = 5f;

        private void Start()
        {
            StartCoroutine(PlayOpening());
        }

        private IEnumerator PlayOpening()
        {
            if (player != null)
                player.SetActive(false);

            if (cameraAnimator != null)
                cameraAnimator.enabled = true;

            yield return new WaitForSecondsRealtime(introDuration);

            FinishOpening();
        }

        public void SkipOpening()
        {
            StopAllCoroutines();
            FinishOpening();
        }

        private void FinishOpening()
        {
            if (cameraAnimator != null)
                cameraAnimator.enabled = false;

            if (player != null)
                player.SetActive(true);
        }
    }
}