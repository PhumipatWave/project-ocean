using UnityEngine;

namespace Game.Testing
{
    public class DemoScript : MonoBehaviour
    {
        public Item[] itemsToPickUp;

        public void PickUpItem(int id)
        {
            bool result = InventoryManager.Instance.AddItem(itemsToPickUp[id]);
            if (result == true)
            {
                Debug.Log("Picked up item");
            }
            else
            {
                Debug.Log("Picked up item");
            }
        }

        public void GetSelectedItem()
        {
            Item receivedItem = InventoryManager.Instance.GetSelectedItem();
            if (receivedItem != null)
            {
                Debug.Log("Received selected item" + receivedItem);
            }
            else
            {
                Debug.Log("NOT Received selected item");
            }
            
        }
        
    }
}
