using UnityEngine;

namespace Game.Testing
{
    public class MenuToggle : MonoBehaviour
    {
        [SerializeField] private GameObject menuPanel;

        public void ToggleMenu()
        {
            menuPanel.SetActive(!menuPanel.activeSelf);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                menuPanel.SetActive(false);
            }
        }
    }
}
