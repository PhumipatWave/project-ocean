using UnityEngine;

namespace Game.Testing
{
    public class ZoneUI : MonoBehaviour
    {
        [SerializeField] private int indexTargetUI;

        public void OpenUI()
        {
            GameManager.Instance.GameUI[indexTargetUI].SetActive(true);
        }

        public void CloseUI()
        {
            GameManager.Instance.GameUI[indexTargetUI].SetActive(false);
        }
    }
}
