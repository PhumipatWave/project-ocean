using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public class MiniHubItem : MonoBehaviour
    {

private void Start()
        {
            GameManager.Instance.MiniHubAvaliable = true;
            GameManager.Instance.MiniHubLocation = transform;

            var player = PlayerRegistry.Local;
            player?.OxygenTube.AttachToHub(transform.position);
        }
    }
}
