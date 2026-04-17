using UnityEngine;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;

namespace Game.Testing
{
    public class Enemy : MonoBehaviour
    {
        [Header("Rope Component")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Player player;
        public Player Player { get { return player; } }

        [Header("Status Component")]
        [SerializeField] private float currentHealth;
        public float CurrentHealth { get { return currentHealth; } }

        private float maxHealth = 100;
        public float MaxHealth { get { return maxHealth; } }

        [SerializeField] private float moveSpeed = 5f;

        [SerializeField] private bool isDeath;
        public bool IsDeath { get { return isDeath; } }

        private bool isPlayerBullet;
        public bool IsPlayerBullet { get { return isPlayerBullet; } set { isPlayerBullet = value; }  }
        
        [Header("Drop Item")]
        [SerializeField] private Item dropItem;

        [SerializeField] private bool isBullet;
        public bool IsBullet { get { return isBullet; } set { isBullet = value; } }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

            currentHealth = maxHealth;
        }
        
        private void Update()
        {
            if (!isDeath && currentHealth <= 0)
            {
                Death();
            }
        }
        
        private void FixedUpdate()
        {
            if (isBullet)
                return;

            //Move();
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;

            if (currentHealth <= 0)
            {
                isDeath = true;
            }
        }

        /*private void Move()
        {
            if (!isDeath)
            {
                Vector2 dir = (player.gameObject.transform.position - transform.position).normalized;
                rb.linearVelocity = dir * moveSpeed;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
        }*/

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (IsBullet)
            {
                if (collision.gameObject.tag == "Enemy")
                {
                    Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                    enemy.TakeDamage(50f);

                    Debug.Log($"Fish Bullet Shoot, Enemy Take damage {50f} => Health amount : {enemy.currentHealth}");
                }
                return;
            } // isPlayerBullet ===
            
            if (isDeath && collision.gameObject.tag == "Player")
            {
                Debug.Log("Tag to Enemy!!");

                InventoryManager.Instance.AddItem(dropItem);
                Debug.Log("เก็บสำเร็จ");
                Destroy(gameObject);
            } // isDead ===
            
        } // OnCollisionEnter2D ===
        
        private void Death()
        {
            isDeath = true;
            //rb.linearVelocity = Vector2.zero;
            gameObject.tag = "Items";
            GetComponent<SpriteRenderer>().color = Color.gray;
            
        }
    }
}
