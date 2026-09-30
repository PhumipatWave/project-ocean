using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class RopeBreakCountdownUI : MonoBehaviour
    {
        [SerializeField] private Image radialFill;

        private CanvasGroup canvasGroup;
        private Coroutine countdownRoutine;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            HidePanel();
        }

        private void OnEnable()
        {
            OxygenRope.OnRopeBreachStart += HandleBreachStart;
            OxygenRope.OnRopeBreachEnd   += HandleBreachEnd;
            SceneManager.sceneLoaded     += OnSceneLoaded;
        }

        private void OnDisable()
        {
            OxygenRope.OnRopeBreachStart -= HandleBreachStart;
            OxygenRope.OnRopeBreachEnd   -= HandleBreachEnd;
            SceneManager.sceneLoaded     -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Ocean_Map")
                HidePanel();
        }

        private void HidePanel()
        {
            if (countdownRoutine != null)
            {
                StopCoroutine(countdownRoutine);
                countdownRoutine = null;
            }

            canvasGroup.alpha          = 0f;
            canvasGroup.blocksRaycasts = false;

            if (radialFill != null)
                radialFill.fillAmount = 1f;
        }

        private void ShowPanel()
        {
            canvasGroup.alpha          = 1f;
            canvasGroup.blocksRaycasts = true;
        }

        private void HandleBreachStart(float duration)
        {
            if (GameSession.State != GameState.Ocean)
                return;

            if (countdownRoutine != null)
                StopCoroutine(countdownRoutine);

            ShowPanel();
            countdownRoutine = StartCoroutine(RunCountdown(duration));
        }

        private void HandleBreachEnd()
        {
            HidePanel();
        }

        private IEnumerator RunCountdown(float duration)
        {
            float elapsed = 0f;
            radialFill.fillAmount = 1f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                radialFill.fillAmount = 1f - (elapsed / duration);
                yield return null;
            }

            countdownRoutine = null;
            HidePanel();
        }
    }
}
