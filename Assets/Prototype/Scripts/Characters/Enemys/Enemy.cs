using UnityEngine;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;

namespace Game.Testing
{
    public class Enemy : MonoBehaviour
    {
        [Header("Rope Component")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private LineRenderer line;
        [SerializeField] private PlayerGunHandler playerGun;
        [SerializeField] private RopeData ropeData;

        [SerializeField] private float currentRopeLength;
        [SerializeField] private float pullBackForce = 5f;

        [SerializeField] private bool isPullBack;
        public bool IsPullBack { get { return isPullBack; } }


        [Header("Status Component")]
        [SerializeField] private float currentHealth;
        public float CurrentHealth { get { return currentHealth; } }

        private float maxHealth = 100;
        public float MaxHealth { get { return maxHealth; } }

        [SerializeField] private float moveSpeed = 5f;

        private bool isHookToGun;
        public bool IsHookToGun { get { return isHookToGun; } set { isHookToGun = value; } }

        private bool isPlayerBullet;
        public bool IsPlayerBullet { get { return isPlayerBullet; } set { isPlayerBullet = value; }  }
        

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            playerGun = GameObject.FindGameObjectWithTag("PlayerGun").GetComponent<PlayerGunHandler>();
            line = GetComponent<LineRenderer>();

            line.positionCount = ropeData.ropeSegment;

            currentHealth = maxHealth;
        }
        /*private void Update()
        {
            if (currentHealth <= 0)
            {
                Die();
            }
        } // Update ===*/
        
        private void FixedUpdate()
        {
            if (playerGun.IsHooked && isHookToGun)
            {
                playerGun.AttachRope(this.gameObject, line, ropeData.ropeSegment, ref isPullBack, ref currentRopeLength, ropeData.maxRopeLength, pullBackForce);
            }
            else if (!playerGun.IsHooked)
            {
                RopeSystemUtil.ClearNormalRope(line);
            }

            Move();
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
        }

        private void Move()
        {
            if (!isHookToGun && currentHealth > 0 && !isPlayerBullet)
            {
                Player player = playerGun.GetComponentInParent<Player>();
                Vector2 dir = (player.gameObject.transform.position - transform.position).normalized;
                rb.linearVelocity = dir * moveSpeed;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isPlayerBullet)
            {
                if (collision.gameObject.tag == "Enemy")
                {
                    Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                    enemy.TakeDamage(50f);

                    Debug.Log($"Fish Bullet Shoot, Enemy Take damage {20f} => Health amount : {enemy.currentHealth}");
                    
                }
                
            }
            
        }
        
        /*private void Die()
        {
            rb.linearVelocity = Vector2.zero;
            gameObject.tag = "Enemy";
            this.enabled = false;
            GetComponent<SpriteRenderer>().color = Color.gray;
            
        } // Die ===*/

    }
}
