using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Keeps this object hidden until the lifetime dive count is reached; stays shown after that.</summary>
    public class RevealAfterTotalDives : MonoBehaviour
    {
        [SerializeField] private int requiredTotalDives = 3;

        private void Awake()
        {
            if (DayManager.Instance == null || DayManager.Instance.TotalDives < requiredTotalDives)
                gameObject.SetActive(false);
        }
    }
}
