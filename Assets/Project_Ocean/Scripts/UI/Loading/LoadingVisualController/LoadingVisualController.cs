using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class LoadingVisualController : MonoBehaviour, ILoadingVisual
    {
        [System.Serializable]
        public class LoadingProfile
        {
            public LoadingType type;
            public Sprite sprite;
            [Tooltip("State name in the icon's Animator Controller.")]
            public string stateName;
        }

        [SerializeField] private Image loadingImage;
        [SerializeField] private Animator animator;
        [SerializeField] private LoadingProfile[] profiles;

        // Must be called while the icon is active, or Animator.Play is ignored.
        public void ApplyProfile(LoadingType type)
        {
            foreach (LoadingProfile profile in profiles)
            {
                if (profile.type != type)
                    continue;

                if (loadingImage != null)
                    loadingImage.sprite = profile.sprite;

                if (animator != null)
                {
                    // Keep animating while game is paused (timeScale = 0).
                    animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                    // Hiding the canvas would otherwise rebind the Animator on next show and
                    // capture MenuToLand's full-screen size as the new "default" for other states.
                    animator.keepAnimatorStateOnDisable = true;
                    animator.Play(profile.stateName, 0, 0f);
                }

                return;
            }

            Debug.LogWarning(
                $"No Loading Profile found for {type}"
            );
        }
    }
}
