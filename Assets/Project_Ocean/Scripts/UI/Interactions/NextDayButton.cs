using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    public class NextDayButton : MonoBehaviour
    {
        void Start()
        {
            GetComponent<Button>().onClick.AddListener(() =>
            {
                DayManager.Instance?.AdvanceDay();
            });
        }
    }
}