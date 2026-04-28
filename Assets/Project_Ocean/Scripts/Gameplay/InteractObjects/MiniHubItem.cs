using UnityEngine;

namespace Game.Testing
{
    public class MiniHubItem : MonoBehaviour
    {
        private PlayerOxygenTube oxygenTube;

        void Start()
        {
            GameManager.Instance.MiniHubAvaliable = true;
            GameManager.Instance.MiniHubLocation = this.transform;

            oxygenTube = GameManager.CurPlayer.GetComponent<PlayerOxygenTube>();
            oxygenTube.SetNewStartPoint(this.transform.position);
        }
    }
}
