using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    public enum LoadingType
    {
        Menu,
        Land,
        Ocean
    }

    public class LoadingVisualController : MonoBehaviour
    {
        [System.Serializable]
        public class LoadingProfile
        {
            public LoadingType type;
            public Sprite sprite;
            public RuntimeAnimatorController animatorController;
        }

        [SerializeField] private Image loadingImage;
        [SerializeField] private Animator animator;
        [SerializeField] private LoadingProfile[] profiles;

        public void ApplyProfile(LoadingType type)
        {
            foreach (LoadingProfile profile in profiles)
            {
                if (profile.type != type)
                    continue;

                if (loadingImage != null)
                    loadingImage.sprite = profile.sprite;

                if (animator != null)
                    animator.runtimeAnimatorController =
                        profile.animatorController;

                return;
            }

            Debug.LogWarning(
                $"No Loading Profile found for {type}"
            );
        }
    }
}