using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

namespace Game.Testing
{
    public class DoorInteract : MonoBehaviour
    {
        [Header("Warp Logic")]
        [SerializeField] private Transform warpDestination;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip doorOpenSound;

        [Header("UI Reference")]
        [SerializeField] private CanvasGroup blackScreenGroup;
        [SerializeField] private float fadeDuration = 0.5f;

        [Header("Camera Control")]
        [SerializeField] private bool snapCamera = true;

        private CinemachineCamera vcam;
        private static bool isWarping;

        void Awake()
        {
            if (snapCamera)
                vcam = Object.FindFirstObjectByType<CinemachineCamera>();
        }

        public void Interact()
        {
            if (!isWarping)
                StartCoroutine(WarpSequence());
        }

        IEnumerator WarpSequence()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null) yield break;

            isWarping = true;

            if (audioSource != null && doorOpenSound != null)
                audioSource.PlayOneShot(doorOpenSound);

            if (blackScreenGroup != null)
            {
                blackScreenGroup.gameObject.SetActive(true);
                blackScreenGroup.alpha = 0;
            }

            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                if (blackScreenGroup != null) blackScreenGroup.alpha = timer / fadeDuration;
                yield return null;
            }
            if (blackScreenGroup != null) blackScreenGroup.alpha = 1;

            Vector3 oldPosition = player.transform.position;
            player.transform.position = warpDestination.position;

            if (snapCamera && vcam != null)
                vcam.OnTargetObjectWarped(player.transform, warpDestination.position - oldPosition);

            yield return new WaitForSeconds(0.1f);

            timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                if (blackScreenGroup != null) blackScreenGroup.alpha = 1 - (timer / fadeDuration);
                yield return null;
            }

            if (blackScreenGroup != null)
            {
                blackScreenGroup.alpha = 0;
                blackScreenGroup.gameObject.SetActive(false);
            }

            isWarping = false;
        }
    }
}
