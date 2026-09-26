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

        [SerializeField] protected GameObject vfxObject;

        protected bool isHit;

        protected Vector2 shootDir;

        /// <summary>How far this ammo flies before it stops on its own (world units) — the aim laser
        /// is drawn this long. 0 = the ammo has no range of its own (e.g. it flies until it hits).</summary>
        public virtual float MaxRange => 0f;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            player = GameManager.CurPlayer;
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
            // One shot = one hit. Overlapping fish all queue a trigger in the same physics step,
            // before the collider disable below takes effect.
            if (isHit) return;

            switch (collision.tag)
            {
                case "Environment":
                    isHit = true;
                    rb.linearVelocity = Vector2.zero;
                    GetComponent<Collider2D>().enabled = false;
                    HideVisuals();
                    break;

                case "Enemy":
                    Enemy enemy = collision.GetComponent<Enemy>();
                    enemy.TakeDamage(attackDamage);
                    (enemy.Brain as IBrainHitReaction)?.OnHit();
                    TutorialEvents.RaiseAction(TutorialActionId.HitEnemy);

                    var darmagePoint = collision.GetComponentInChildren<IDamagePopup>();
                    darmagePoint.ShowDamagePoint(attackDamage.ToString());

                    player.GunHandler.ResetGunRotation(player.FaceDir);

                    isHit = true;
                    rb.linearVelocity = Vector2.zero;
                    GetComponent<Collider2D>().enabled = false;
                    HideVisuals();
                    break;
            }
        }

        protected virtual void HideVisuals()
        {
            GetComponentInChildren<SpriteRenderer>().enabled = false;
            if (vfxObject != null) vfxObject.SetActive(false);
        }

        protected void BulletDirection()
        {
            float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0, 0, angle);
        }

        protected IEnumerator ShootCooldownRoutine()
        {
            player.GunHandler.ResetGunRotation(player.FaceDir);

            yield return new WaitForSeconds(.4f);
            player.GunHandler.CanAttack = true;
        }
    }
}
