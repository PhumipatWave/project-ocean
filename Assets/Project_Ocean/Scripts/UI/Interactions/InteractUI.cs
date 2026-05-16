using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    public class InteractUI : MonoBehaviour
    {
        [SerializeField] private GameObject interactIcon;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                interactIcon.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                interactIcon.SetActive(false);
            }
        }
    }
}
