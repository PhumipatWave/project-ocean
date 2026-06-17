using UnityEngine;

namespace Game.Testing
{
    public class CheckPointItem : MonoBehaviour, IItemEffect
    {
        public void ExecuteItem(Player player)
        {
            Instantiate(gameObject, player.transform.position, Quaternion.identity);
        }

        public bool CanExecute(Player player) => true;
    }
}
