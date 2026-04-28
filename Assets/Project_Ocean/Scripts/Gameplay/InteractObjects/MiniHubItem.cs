using UnityEngine;

namespace Game.Testing
{
    public class MiniHubItem : MonoBehaviour
    {
        void Start()
        {
            GameManager.Instance.MiniHubAvaliable = true;
            GameManager.Instance.MiniHubLocation = this.transform;
        }
    }
}
