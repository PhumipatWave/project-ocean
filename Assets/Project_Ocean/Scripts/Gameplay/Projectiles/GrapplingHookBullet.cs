using System.Collections;
using UnityEngine;
using static Unity.Cinemachine.CinemachineSplineRoll;

namespace Game.Testing
{
    public class GrapplingHookBullet : Bullet
    {
        [SerializeField] private Player player;

        private Transform targetObj;
        private Vector2 localPoint;

        [SerializeField] private bool attachedToObject;
        public bool AttachedToObject { get { return attachedToObject; } }

        private bool isDashReachingTarget;

        protected override void Awake()
        {
            base.Awake();

            player = GameObject.FindWithTag("Player").GetComponent<Player>();
        }

        protected override void Start()
        {

        }

        protected override void FixedUpdate()
        {
            if (isDashReachingTarget)
                return;

            player.GunHandler.AttachRope(this.gameObject);

            BulletDirection();

            float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0, 0, angle);

            if (!attachedToObject)
            {
                if (!player.GunHandler.IsPullBack)
                {
                    rb.linearVelocity = shootDir * moveSpeed;
                }
                else
                {
                    Vector2 dir = (player.GunHandler.FirePoint.transform.position - transform.position).normalized;
                    rb.linearVelocity = dir * moveSpeed;

                    if (IsReachingTargetPoint(player.GunHandler.FirePoint.transform.position, transform.position, 2f))
                    {
                        player.GunHandler.CanAttack = true;
                        player.GunHandler.ResetGunRotation(player.FaceDir);

                        RopeSystemUtil.ClearNormalRope(player.GunHandler.Line);
                        Destroy(gameObject);
                    }
                }
            }
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            if (!player.GunHandler.IsPullBack)
            {
                switch (collision.tag)
                {
                    case "Enemy":
                        Enemy enemy = collision.GetComponent<Enemy>();
                        enemy.TakeDamage(attackDamage);

                        if (!enemy.IsDeath)
                        {
                            Debug.Log($"Grappling Hook Fire : {collision.name}");

                            player.GunHandler.CanAttack = true;
                            RopeSystemUtil.ClearNormalRope(player.GunHandler.Line);
                            player.GunHandler.ResetGunRotation(player.FaceDir);

                            Destroy(gameObject);
                        }
                        else
                        {
                            Debug.Log($"Grappling hook fire death enemy");
                            StartCoroutine(HookFishRoutine(collision.gameObject));
                        }

                        break;

                    case "Environment":

                        attachedToObject = true;
                        player.GunHandler.IsHooked = true;

                        rb.linearVelocity = Vector2.zero;
                        rb.bodyType = RigidbodyType2D.Kinematic;

                        Vector2 hitPoint = collision.ClosestPoint(transform.position);

                        targetObj = collision.transform;
                        localPoint = targetObj.InverseTransformPoint(hitPoint);

                        transform.position = hitPoint;

                        Vector2 dir = ((Vector2)transform.position - (Vector2)targetObj.position).normalized;
                        transform.right = dir;

                        StartCoroutine(HookDashRoutine(hitPoint));
                        break;
                }
            }
        }

        private bool IsReachingTargetPoint(Vector2 startPos, Vector2 endPos, float stopDist)
        {
            float dist = Vector2.Distance(startPos, endPos);
            return dist <= stopDist;
        }

        public IEnumerator HookDashRoutine(Vector2 hitPos)
        {
            float stopDist = 2f;
            isDashReachingTarget = false;

            int playerLayerIndex = LayerMask.NameToLayer("Player");
            int obstacleLayerIndex = LayerMask.NameToLayer("Obstacle");

            Physics2D.IgnoreLayerCollision(playerLayerIndex, obstacleLayerIndex, true);

            while (!isDashReachingTarget)
            {
                Vector2 playerPos = player.transform.position;
                Vector2 dir = (hitPos - playerPos).normalized;

                player.RB.linearVelocity = dir * player.DashSpeed;

                isDashReachingTarget = IsReachingTargetPoint(playerPos, hitPos, stopDist);

                yield return null;
            }

            attachedToObject = false;
            player.RB.linearVelocity = Vector2.zero;
            RopeSystemUtil.ClearNormalRope(player.GunHandler.Line);
            Physics2D.IgnoreLayerCollision(playerLayerIndex, obstacleLayerIndex, false);

            player.GunHandler.ResetGunRotation(player.FaceDir);

            yield return new WaitForSeconds(.75f);
            player.GunHandler.CanAttack = true;
            player.GunHandler.IsHooked = false;

            //Debug.Log("Hook Dash End");

            Destroy(gameObject);
        }

        public IEnumerator HookFishRoutine(GameObject target)
        {
            attachedToObject = true;
            player.GunHandler.IsHooked = true;

            Debug.Log($"Grappling hook return to fire point");
            while (!IsReachingTargetPoint(player.GunHandler.FirePoint.transform.position, transform.position, 2f))
            {
                Vector2 backwardDir = (player.GunHandler.FirePoint.transform.position - transform.position).normalized;
                rb.linearVelocity = backwardDir * moveSpeed;

                yield return null;
            }

            Debug.Log($"Instaniate fish bullet");
            var obj = Instantiate(target, player.GunHandler.FirePoint.transform.position, player.GunHandler.GunSprite.transform.rotation);

            Destroy(target.gameObject);

            obj.GetComponent<Enemy>().IsBullet = true;
            obj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            obj.GetComponent<Collider2D>().enabled = false;
            obj.transform.SetParent(player.GunHandler.transform, true);
            player.GunHandler.CurrentHookObj = obj;

            attachedToObject = false;
            player.GunHandler.CanAttack = true;
            player.GunHandler.ResetGunRotation(player.FaceDir);

            RopeSystemUtil.ClearNormalRope(player.GunHandler.Line);
            Destroy(gameObject);
        }
    }
}
