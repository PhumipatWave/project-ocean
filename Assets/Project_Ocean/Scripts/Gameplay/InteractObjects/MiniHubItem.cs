using UnityEngine;

namespace Game.Testing
{
    public class MiniHubItem : MonoBehaviour
    {
        private PlayerOxygenTube oxygenTube;

        private void Start()
        {
            GameManager.Instance.MiniHubAvaliable = true;
            GameManager.Instance.MiniHubLocation = this.transform;

            oxygenTube = GameManager.CurPlayer.GetComponentInChildren<PlayerOxygenTube>();
            oxygenTube.SetNewStartPoint(this.transform.position);
        }
    

public bool CanExecute(Player player) => true;
}
}
