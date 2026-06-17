using System.Collections;
using UnityEngine;

namespace Game.Testing
{
    public abstract class Bullet : MonoBehaviour
    {
        [SerializeField] protected Rigidbody2D rb;
        [SerializeField] protected Player player;

        [SerializeField] protected float moveSpeed = 15f;
        [SerializeField] protected float attackDamage = 25f;

        protected bool isHit;

        protected Vector2 shootDir;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        protected virtual void Start()
        {
            StartCoroutine(ShootCooldownRoutine());
        }

        public void Initialize(Vector2 shootDir)
        {
            this.shootDir = shootDir;
        }

        protected virtual void FixedUpdate()
        {
            if (isHit) return;

            BulletDirection();
            rb.linearVelocity = shootDir * moveSpeed;
        }

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            switch (collision.tag)
            {
                case "Environment":
                    isHit = true;
                    rb.linearVelocity = Vector2.zero;
                    gameObject.GetComponent<Collider2D>().enabled = false;
                    gameObject.GetComponentInChildren<SpriteRenderer>().enabled = false;
                    break;

                case "Enemy":

                    Enemy enemy = collision.GetComponent<Enemy>();
                    enemy.TakeDamage(attackDamage);
                    (enemy.Brain as IBrainHitReaction)?.OnHit();

                    var darmagePoint = collision.GetComponentInChildren<DamagePoint>();
                    darmagePoint.ShowDamagePoint(attackDamage.ToString());

                    player.GunHandler.ResetGunRotation(player.FaceDir);
                    
                    isHit = true;
                    rb.linearVelocity = Vector2.zero;
                    gameObject.GetComponent<Collider2D>().enabled = false;
                    gameObject.GetComponentInChildren<SpriteRenderer>().enabled = false;
                    break;
            }
        }

        protected void BulletDirection()
        {
            float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0, 0, angle);
        }

        protected IEnumerator ShootCooldownRoutine()
        {
            player.GunHandler.ResetGunRotation(player.FaceDir);

            yield return new WaitForSeconds(2.25f);
            player.GunHandler.CanAttack = true;
            Destroy(gameObject);
        }
    }
}
