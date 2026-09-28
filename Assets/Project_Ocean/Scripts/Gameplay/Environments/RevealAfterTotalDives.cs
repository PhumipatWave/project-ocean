using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Deactivates this GameObject until the player has reached a lifetime dive count
    /// (DayManager.TotalDives). Permanent — once revealed it stays revealed every future
    /// load, since TotalDives never resets.
    /// </summary>
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
