using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;

namespace Game.Testing
{
    public class PlayerGunHandler : MonoBehaviour
    {
        [SerializeField] private AmmoWheel ammoWheel;

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

        [SerializeField] private bool canAttack = true;
        public bool CanAttack { get { return canAttack; } set { canAttack = value; } }

        [SerializeField] private bool isHooked;
        public bool IsHooked { get { return isHooked; } set { isHooked = value; } }

        private void Start()
        {
            ammoWheel = GameObject.FindGameObjectWithTag("GameHUD").GetComponent<AmmoWheel>();
        }

        private float HandlerGunRotation(int faceDir, float angle)
        {
            // Clamp angle by face direction
            float clampAngle;
            if (faceDir == 1)
            {
                clampAngle = Mathf.Clamp(angle, -45f, 45f);
            }
            else
            {
                if (angle > 0f)
                {
                    clampAngle = Mathf.Clamp(angle, 135f, 180f);
                }
                else
                {
                    clampAngle = Mathf.Clamp(angle, -180f, -135f);
                }
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

        public void Aimming(int faceDir, Vector2 mousePos)
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

                float clamAngle = HandlerGunRotation(faceDir, angle);

                float rad = clamAngle * Mathf.Deg2Rad;
                aimDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

                gunSprite.eulerAngles = new Vector3(0, 0, clamAngle);

                Debug.DrawRay(transform.position, aimDir * 10f, Color.red);
            }
        }

        public void Shooting()
        {
            if (isHooked)
            {
                var obj = Instantiate(currentHookObj, firePoint.transform.position, Quaternion.identity);
                obj.GetComponent<Rigidbody2D>().linearVelocity = aimDir * 20f;

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
