using UnityEngine;

namespace Game.Testing
{
    public class MiniHubItem : MonoBehaviour
    {
        private OxygenRope oxygenTube;

        private void Start()
        {
            GameManager.Instance.MiniHubAvaliable = true;
            GameManager.Instance.MiniHubLocation = this.transform;

            oxygenTube = GameManager.CurPlayer.GetComponentInChildren<OxygenRope>();
            oxygenTube.SetStartPoint(this.transform.position);
        }
    }
}
