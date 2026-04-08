using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;

namespace Game.Testing
{
    public class PlayerGunHandler : MonoBehaviour
    {
        [SerializeField] private AmmoWheel ammoWheel;

        [SerializeField] private Player player;

        [Header("Gun Object")]
        [SerializeField] private Transform gunSprite; 
        public Transform GunSprite {  get { return gunSprite; } }

        [SerializeField] private Transform[] bulletTypePrefab;
        [SerializeField] private GameObject firePoint;
        public GameObject FirePoint { get { return firePoint; } }

        [SerializeField] private Transform currentBulletObj;
        public Transform CurrentBulletObj { get { return currentBulletObj; } }

        [SerializeField] private GameObject currentHookObj;
        public GameObject CurrentHookObj { get { return currentHookObj; } set { currentHookObj = value; } }

        [Header("Gun Status")]
        private Vector2 aimDir;
        public Vector2 AimDir { get { return aimDir; }  }

        private bool isAimming;
        public bool IsAimming {  get { return isAimming; } set { isAimming = value; } }

        private float _switchBuffer = 10f;

        [SerializeField] private bool canAttack = true;
        public bool CanAttack { get { return canAttack; } set { canAttack = value; } }

        [SerializeField] private bool isHooked;
        public bool IsHooked { get { return isHooked; } set { isHooked = value; } }

        private void Start()
        {
            ammoWheel = GameObject.FindGameObjectWithTag("GameHUD").GetComponent<AmmoWheel>();
            player = GameObject.FindWithTag("Player").GetComponent<Player>();
        }

        private float HandlerGunRotation(ref int faceDir, float angle)
        {
            // Normalize angle (-180 to 180)
            angle = Mathf.DeltaAngle(0f, angle);

            float clampAngle;
            bool currentlyFacingRight = faceDir == 1;

            if (currentlyFacingRight)
            {
                // Only switch to LEFT if angle clearly exits the right zone
                if (angle > 90f + _switchBuffer || angle < -90f - _switchBuffer)
                    faceDir = -1;
            }
            else
            {
                // Only switch to RIGHT if angle clearly enters the right zone
                if (angle >= -90f + _switchBuffer && angle <= 90f - _switchBuffer)
                    faceDir = 1;
            }

            // RIGHT SIDE
            if (faceDir == 1)
            {
                clampAngle = Mathf.Clamp(angle, -45f, 45f);
                player.PlayerSprite.rotation = Quaternion.Euler(0f, 0f, 0f);
                gunSprite.rotation = Quaternion.Euler(0f, 0f, clampAngle);
            }
            // LEFT SIDE
            else
            {
                if (angle > 0)
                    clampAngle = Mathf.Clamp(angle, 135f, 180f);
                else
                    clampAngle = Mathf.Clamp(angle, -180f, -135f);

                player.PlayerSprite.rotation = Quaternion.Euler(0f, 180f, 0f);
                gunSprite.rotation = Quaternion.Euler(0f, 180f, clampAngle);
            }

            return clampAngle;
        }

        public void ResetGunRotation(int faceDir)
        {
            if (faceDir == 1)
            {
                gunSprite.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else
            {
                gunSprite.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }

        public void Aimming(ref int faceDir, Vector2 mousePos)
        {
            if (isAimming)
            {
                if (!canAttack)
                {
                    isAimming = false;
                    return;
                }

                // Convert screen position to world position
                Vector2 aimWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

                // Calculate direction vector pointing from player to mouse
                Vector2 rawDir = (aimWorldPos - (Vector2)transform.position).normalized;

                // Convert direction to radian and convert to degree for use in unity rotation
                float angle = Mathf.Atan2(rawDir.y, rawDir.x) * Mathf.Rad2Deg;

                float clamAngle = HandlerGunRotation(ref faceDir, angle);

                float rad = clamAngle * Mathf.Deg2Rad;
                aimDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

                gunSprite.eulerAngles = new Vector3(0, 0, clamAngle);

                Debug.DrawRay(transform.position, aimDir * 10f, Color.red);
            }
            else if (!player.IsAttacked)
            {
                ResetGunRotation(player.FaceDir);
            }
        }

        public void Shooting()
        {
            if (isHooked)
            {
                var obj = Instantiate(currentHookObj, firePoint.transform.position, Quaternion.identity);
                obj.GetComponent<Enemy>().IsPlayerBullet = true;
                obj.GetComponent<Rigidbody2D>().linearVelocity = aimDir * 20f;

                canAttack = true;
                isHooked = false;
                Destroy(currentHookObj);
            }
            else
            {
                currentBulletObj = Instantiate(bulletTypePrefab[(int)ammoWheel.currentAmmoType], firePoint.transform.position, Quaternion.identity);
                currentBulletObj.GetComponent<Bullet>().Initialize(aimDir);
            }
        }

        public void AttachRope(GameObject target, LineRenderer line,  int ropeSegment, ref bool isPullBack, ref float currentRopeLength, float maxRopeLength, float pullBackForce)
        {
            RopeSystemUtil.DrawNormalRope(line, FirePoint, target, segment: ropeSegment);
            RopeSystemUtil.LimitNormalRopeDistance(FirePoint, target, maxRopeLength);
            RopeSystemUtil.BreakNormalRopeLimit(FirePoint, target, ref isPullBack, ref currentRopeLength, maxRopeLength, pullBackForce);
        }
    }
}
