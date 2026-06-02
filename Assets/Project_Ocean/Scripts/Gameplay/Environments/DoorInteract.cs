using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class DoorInteract : MonoBehaviour
{
    [Header("Warp Logic")]
    [SerializeField] private Transform warpDestination;
    [SerializeField] private bool requireInteraction = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource; // ลาก AudioSource มาใส่
    [SerializeField] private AudioClip doorOpenSound; // ลากไฟล์เสียงประตูมาใส่

    [Header("UI Reference")]
    [SerializeField] private CanvasGroup blackScreenGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Camera Control")]
    [SerializeField] private bool snapCamera = true;

    private bool isPlayerInside = false;
    private GameObject playerObject;
    private static bool isWarping = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInside = true;
            playerObject = other.gameObject;

            if (!requireInteraction && !isWarping)
            {
                StartCoroutine(WarpSequence());
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInside = false;
        } 
    }

    void Update()
    {
        // จังหวะคลิกขวา (Input.GetMouseButtonDown(1))
        if (requireInteraction && isPlayerInside && Input.GetKeyDown(KeyCode.E) && !isWarping)
        {
            StartCoroutine(WarpSequence());
        }
    }

    IEnumerator WarpSequence()
    {
        isWarping = true;

        // --- เพิ่มส่วนการเล่นเสียง ---
        if (audioSource != null && doorOpenSound != null)
        {
            audioSource.PlayOneShot(doorOpenSound);
        }

        if (blackScreenGroup != null)
        {
            blackScreenGroup.gameObject.SetActive(true);
            blackScreenGroup.alpha = 0;
        }

        // 1. Fade In
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            if (blackScreenGroup != null)
                blackScreenGroup.alpha = timer / fadeDuration;
            yield return null;
        }
        if (blackScreenGroup != null) blackScreenGroup.alpha = 1;

        // 2. Warp Player
        Vector3 oldPosition = playerObject.transform.position;
        playerObject.transform.position = warpDestination.position;

        // 3. Warp Cinemachine
        if (snapCamera)
        {
            var vcam = Object.FindFirstObjectByType<CinemachineCamera>();
            if (vcam != null)
            {
                vcam.OnTargetObjectWarped(playerObject.transform, warpDestination.position - oldPosition);
            }
        }

        yield return new WaitForSeconds(0.1f);

        // 4. Fade Out
        timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            if (blackScreenGroup != null)
                blackScreenGroup.alpha = 1 - (timer / fadeDuration);
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