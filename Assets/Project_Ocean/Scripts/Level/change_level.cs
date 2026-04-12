using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Testing
{
    public class change_level : MonoBehaviour
    {
        public string sceneName;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                SceneManager.LoadScene(sceneName);
            }
        }
    }
}
