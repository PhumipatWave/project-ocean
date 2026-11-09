using System.Collections;
using UnityEngine;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    /// Full-screen day-transition animation driven by an Animator.
    /// Lives on GameHUD — persists across all scenes.
    public class NextDayAnimationUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Animator    animator;

        private static readonly int PlayHash = Animator.StringToHash("Play");
        private Coroutine _routine;

        /// True while the day-transition animation is on screen.
        public static bool IsPlaying { get; private set; }

        private void Awake()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha          = 0f;
                canvasGroup.blocksRaycasts = false;
            }
        }

        private void OnEnable()  => DayManager.OnDayAdvanced += PlayAnimation;
        private void OnDisable()
        {
            DayManager.OnDayAdvanced -= PlayAnimation;
            IsPlaying = false;
        }

        private void PlayAnimation()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(AnimRoutine());
        }

        private IEnumerator AnimRoutine()
        {
            IsPlaying = true;
            canvasGroup.alpha          = 1f;
            canvasGroup.blocksRaycasts = true;
            PlayerUIState.Open();

            animator.SetTrigger(PlayHash);

            // Wait one frame for the animator to enter the Play state
            yield return null;

            // Wait until the Play state finishes or transitions back to Idle
            while (true)
            {
                var info = animator.GetCurrentAnimatorStateInfo(0);
                if (info.IsName("Play") && info.normalizedTime >= 1f) break;
                if (info.IsName("Idle")) break;
                yield return null;
            }

            PlayerUIState.Close();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha          = 0f;
            IsPlaying = false;
            _routine = null;
        }
    }
}
