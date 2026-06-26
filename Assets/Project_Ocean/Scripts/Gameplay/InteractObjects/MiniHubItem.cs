using UnityEngine;

namespace Game.Testing
{
    public class MiniHubItem : MonoBehaviour
    {
        private OxygenRope oxygenTube;

        private void Start()
        {
            var player = GameManager.CurPlayer;
            player?.ClearAllCheckpoints();

            GameManager.Instance.MiniHubAvaliable = true;
            GameManager.Instance.MiniHubLocation = this.transform;

            oxygenTube = player?.GetComponentInChildren<OxygenRope>();
            oxygenTube?.SetStartPoint(this.transform.position);
        }
    }
}
