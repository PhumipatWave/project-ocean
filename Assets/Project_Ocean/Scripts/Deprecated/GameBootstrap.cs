using UnityEngine;
using Game.Core;
using Game.Gameplay;
using Game.UI;

namespace Game.Deprecated
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
