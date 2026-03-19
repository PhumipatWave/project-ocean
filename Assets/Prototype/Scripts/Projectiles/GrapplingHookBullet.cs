using UnityEngine;

namespace Game.Testing
{
    public class GrapplingHookBullet : Bullet
    {
        [SerializeField] private LineRenderer line;
        [SerializeField] private RopeData ropeData;

        [SerializeField] protected float currentRopeLength;
        [SerializeField] protected float pullBackForce = 5f;

        protected bool isPullBack;
        public bool IsPullBack { get { return isPullBack; } }

        protected override void Awake()
        {
            base.Awake();

            line = GetComponent<LineRenderer>();
            line.positionCount = ropeData.ropeSegment;
        }

        protected override void Start()
        {

        }

        protected override void FixedUpdate()
        {
            BulletDirection();

            if (!isPullBack)
            {
                rb.linearVelocity = shootDir * moveSpeed;
            }
            else
            {
                rb.linearVelocity = -shootDir * moveSpeed;
            }

            float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0, 0, angle);

            player.GunHandler.AttachRope(this.gameObject, line, ropeData.ropeSegment, ref isPullBack, ref currentRopeLength, ropeData.maxRopeLength, pullBackForce);
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            if (!isPullBack)
            {
                switch (collision.tag)
                {
                    case "Enemy":

                        Debug.Log($"Fire : {collision.name}");

                        Enemy enemy = collision.GetComponent<Enemy>();
                        if (enemy.CurrentHealth > 0f && enemy.CurrentHealth != 0f)
                        {
                            enemy.TakeDamage(attackDamage);

                            player.GunHandler.CanAttack = true;
                            player.GunHandler.ResetGunRotation(player.FaceDir);

                            Destroy(gameObject);
                        }
                        else
                        {
                            enemy.IsHookToGun = true;
                            player.GetComponentInChildren<PlayerGunHandler>().IsHooked = true;
                            player.GetComponentInChildren<PlayerGunHandler>().CurrentHookObj = collision.gameObject;

                            player.GunHandler.CanAttack = true;
                            player.GunHandler.ResetGunRotation(player.FaceDir);

                            Destroy(gameObject);
                        }

                        break;

                    case "Environment":

                        player.GetComponentInChildren<PlayerGunHandler>().IsHooked = true;
                        player.GetComponentInChildren<PlayerGunHandler>().CurrentHookObj = collision.gameObject;

                        player.GunHandler.CanAttack = true;
                        player.GunHandler.ResetGunRotation(player.FaceDir);

                        Destroy(gameObject);

                        break;
                }
            }
            else
            {
                switch (collision.tag)
                {
                    case "CollectBox":
                        Debug.Log("Collect bullet");

                        player.GunHandler.CanAttack = true;
                        player.GunHandler.ResetGunRotation(player.FaceDir);

                        Destroy(gameObject);
                        break;
                }
            }
        }
    }
}
