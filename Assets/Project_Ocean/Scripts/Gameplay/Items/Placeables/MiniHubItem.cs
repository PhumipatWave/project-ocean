using UnityEngine;

namespace Game.Gameplay
{
    public class MiniHubItem : MonoBehaviour
    {
        // Awake runs inside Instantiate, so the rope re-anchors the same frame the hub is placed.
        private void Awake() => PlayerRegistry.Local?.OxygenTube.AttachToHub(transform.position);
    }
}
