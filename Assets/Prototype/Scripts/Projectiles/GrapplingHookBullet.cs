using System.Collections;
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
                        enemy.TakeDamage(attackDamage);

                        if (enemy.CurrentHealth > 0f)
                        {
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

                        Environment environment = collision.GetComponent<Environment>();
                        environment.IsHookToGun = true;

                        player.GetComponentInChildren<PlayerGunHandler>().IsHooked = true;

                        StartCoroutine(HookDashRoutine(environment.IsHookToGun));

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

        public IEnumerator HookDashRoutine(bool isHookGun)
        {
            Vector2 dir = (transform.position - player.gameObject.transform.position).normalized;

            player.RB.linearVelocity = dir * 45f;
            yield return new WaitForSeconds(0.5f);

            isHookGun = false;
            player.GunHandler.IsHooked = false;
            player.GunHandler.ResetGunRotation(player.FaceDir);
            rb.linearVelocity = Vector2.zero;

            yield return new WaitForSeconds(.75f);
            player.GunHandler.CanAttack = true;
            Destroy(gameObject);
        }
    }
}
