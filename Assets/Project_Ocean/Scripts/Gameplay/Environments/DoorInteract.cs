using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

namespace Game.Testing
{
    public class DoorInteract : MonoBehaviour
    {
        [Header("Warp Logic")]
        [SerializeField] private Transform warpDestination;
        [SerializeField] private bool requireInteraction = true;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip doorOpenSound;

        [Header("UI Reference")]
        [SerializeField] private CanvasGroup blackScreenGroup;
        [SerializeField] private float fadeDuration = 0.5f;

        [Header("Camera Control")]
        [SerializeField] private bool snapCamera = true;

        private bool isPlayerInside;
        private GameObject playerObject;
        private CinemachineCamera vcam;
        private static bool isWarping;

        void Awake()
        {
            if (snapCamera)
                vcam = Object.FindFirstObjectByType<CinemachineCamera>();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInside = true;
                playerObject = other.gameObject;

                if (!requireInteraction && !isWarping)
                    StartCoroutine(WarpSequence());
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                isPlayerInside = false;
        }

        void Update()
        {
            if (requireInteraction && isPlayerInside && Input.GetKeyDown(KeyCode.E) && !isWarping)
                StartCoroutine(WarpSequence());
        }

        IEnumerator WarpSequence()
        {
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

            Vector3 oldPosition = playerObject.transform.position;
            playerObject.transform.position = warpDestination.position;

            if (snapCamera && vcam != null)
                vcam.OnTargetObjectWarped(playerObject.transform, warpDestination.position - oldPosition);

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
