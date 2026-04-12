using UnityEngine;

namespace Game.Testing
{
    public class Itemdrop : MonoBehaviour
    {
        [Header("Drop Item")]
        [SerializeField] private Item dropItem;

        public void Start()
        {
            
        }

        public void Update()
        {
            
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                Debug.Log("Keep Items to Inventory!!");
                InventoryManager inv = FindObjectOfType<InventoryManager>();
                
                if (inv != null)
                {
                    if (inv.AddItem(dropItem))
                    {
                        Debug.Log("เก็บสำเร็จ");
                        Destroy(gameObject);
                    }
                }
            } // collision.gameObject.tag == "Player"
            
        } // OnCollisionEnter2D ===
        
    }
}
