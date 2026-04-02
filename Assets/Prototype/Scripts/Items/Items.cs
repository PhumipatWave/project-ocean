using UnityEngine;

namespace Game.Testing
{
    public class Items : MonoBehaviour
    {
        [SerializeField] private string itemName;
        [SerializeField] private int quantity;
        [SerializeField] private Sprite sprite;
        
        private inventoryManager inventoryManager;
        
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            inventoryManager = GameObject.Find("Inventory Canvas").GetComponent<inventoryManager>();
        
        } // Start

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                inventoryManager.AddItam(itemName, quantity, sprite);
                Destroy(gameObject);
            }
        } // OnCollisionEnter2D

        
    }
}
