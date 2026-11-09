using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using Game.Core;

namespace Game.Gameplay
{
    public class DoorInteract : Interactable
    {
        [Header("Warp Logic")]
        [SerializeField] private Transform warpDestination;

        [Header("UI Reference")]
        [SerializeField] private CanvasGroup blackScreenGroup;
        [SerializeField] private float fadeDuration = 0.5f;

        private static bool isWarping;

        /// <summary>Raised right after the player is moved, while the screen is still black.</summary>
        public event System.Action Warped;

        protected override void Interact(Player player)
        {
            if (!isWarping)
                StartCoroutine(WarpSequence(player.transform));
        }

        IEnumerator WarpSequence(Transform player)
        {
            isWarping = true;

            AudioManager.Instance?.PlaySFX(SoundKeys.DoorOpen);

            if (blackScreenGroup != null)
            {
                blackScreenGroup.gameObject.SetActive(true);
                blackScreenGroup.alpha = 0;
            }

            yield return Fade(0f, 1f);

            Vector3 oldPosition = player.position;
            player.position = warpDestination.position;
            CinemachineCore.OnTargetObjectWarped(player, warpDestination.position - oldPosition);

            Warped?.Invoke();

            yield return new WaitForSeconds(0.1f);
            yield return Fade(1f, 0f);

            if (blackScreenGroup != null)
                blackScreenGroup.gameObject.SetActive(false);

            isWarping = false;
        }

        IEnumerator Fade(float from, float to)
        {
            for (float timer = 0f; timer < fadeDuration; timer += Time.deltaTime)
            {
                if (blackScreenGroup != null) blackScreenGroup.alpha = Mathf.Lerp(from, to, timer / fadeDuration);
                yield return null;
            }
            if (blackScreenGroup != null) blackScreenGroup.alpha = to;
        }
    }
}
