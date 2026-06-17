using UnityEngine;

namespace Game.Testing
{
    public class Itemdrop : MonoBehaviour
    {
        [Header("Drop Item")]
        [SerializeField] private Item dropItem;

        [SerializeField] private AudioSource audio;
        [SerializeField] private AudioClip dropSound;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                Debug.Log("Keep Items to Inventory!!");

                audio.PlayOneShot(dropSound);
                InventoryManager.Instance.AddItem(dropItem);
                Debug.Log("Keep Destroy!!");
                Destroy(gameObject);
            } // collision.gameObject.tag == "Player"
            
        } // OnCollisionEnter2D ===
        
    }
}
