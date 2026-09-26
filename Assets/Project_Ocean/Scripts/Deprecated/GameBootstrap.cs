using UnityEngine;

namespace Game.Testing
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private string firstScene = "Land_Map";

        private void Start()
        {
            LoadingManager.Instance.LoadScene(firstScene);
        }
    }
}
