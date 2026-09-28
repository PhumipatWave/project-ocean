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

            var player = GameManager.CurPlayer;
            player?.OxygenTube.AttachToHub(transform.position);
        }
    }
}
