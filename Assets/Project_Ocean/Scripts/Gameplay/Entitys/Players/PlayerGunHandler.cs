using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    public class PlayerGunHandler : MonoBehaviour
    {
        //[SerializeField] private AmmoWheel ammoWheel;
        [SerializeField] private Player player;

        [SerializeField] private AudioClip shootSound;

        [Header("Rope System")]
        [SerializeField] private LineRenderer line;
        public LineRenderer Line { get { return line; } }

        [SerializeField] private RopeStructureData ropeData;

        [SerializeField] private float currentRopeLength;
        [SerializeField] private float pullBackForce = 5f;

        [SerializeField] private bool isHooked;
        public bool IsHooked { get { return isHooked; } set { isHooked = value; } }

        [SerializeField] private bool isPullBack;
        public bool IsPullBack { get { return isPullBack; } }

        [Header("Gun References")]
        [SerializeField] private Transform gunSprite; 
        public Transform GunSprite {  get { return gunSprite; } }

        [SerializeField] private Transform[] bulletTypePrefab;
        [SerializeField] private GameObject firePoint;
        public GameObject FirePoint { get { return firePoint; } }

        [SerializeField] private Transform currentBulletObj;
        public Transform CurrentBulletObj { get { return currentBulletObj; } set { currentBulletObj = value; } }

        [SerializeField] private GameObject currentHookObj;
        public GameObject CurrentHookObj { get { return currentHookObj; } set { currentHookObj = value; } }

        [Header("Gun Status")]
        private Vector2 aimDir;
        public Vector2 AimDir { get { return aimDir; }  }

        private bool isAimming;
        public bool IsAimming {  get { return isAimming; } set { isAimming = value; } }

        [SerializeField] private bool canAttack = true;
        public bool CanAttack { get { return canAttack; } set { canAttack = value; } }

        public AmmoType currentAmmoType;

        [Header("Gun Stab")]
        [SerializeField] private GameObject stabHitbox;

        [SerializeField] private float stabActiveTime = 0.15f;
        [SerializeField] private float stabCooldown = 0.5f;

        private bool canStabAttack = true;
        public bool CanStabAttack { get { return canStabAttack; } set { canStabAttack = value; } }

        private void Start()
        {
            //ammoWheel = GameObject.FindGameObjectWithTag("GameHUD").GetComponent<AmmoWheel>();
            player = GameObject.FindWithTag("Player").GetComponent<Player>();

            line = GetComponent<LineRenderer>();
            line.positionCount = ropeData.ropeSegment;
        }

        private float HandlerGunRotation(ref int faceDir, float angle)
        {
            angle = Mathf.DeltaAngle(0f, angle);

            float clampAngle;
            float switchBuffer = 10f;
            bool currentlyFacingRight = faceDir == 1;

            if (currentlyFacingRight)
            {
                if (angle > 90f + switchBuffer || angle < -90f - switchBuffer)
                    faceDir = -1;
            }
            else
            {
                if (angle >= -90f + switchBuffer && angle <= 90f - switchBuffer)
                    faceDir = 1;
            }

            if (faceDir == 1)
            {
                clampAngle = Mathf.Clamp(angle, -45f, 45f);
                player.PlayerSprite.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                gunSprite.rotation = Quaternion.Euler(0f, 0f, clampAngle);
            }
            else
            {
                if (angle > 0)
                    clampAngle = Mathf.Clamp(angle, 135f, 180f);
                else
                    clampAngle = Mathf.Clamp(angle, -180f, -135f);

                player.PlayerSprite.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                gunSprite.rotation = Quaternion.Euler(0f, 180f, clampAngle);
            }

            return clampAngle;
        }

        public void ResetGunRotation(int faceDir)
        {
            if (faceDir == 1)
                gunSprite.rotation = Quaternion.Euler(0f, 0f, 0f);
            else
                gunSprite.rotation = Quaternion.Euler(0f, 180f, 0f);
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

                Vector2 aimWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
                Vector2 rawDir = (aimWorldPos - (Vector2)transform.position).normalized;

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
                currentHookObj.transform.SetParent(null);
                currentHookObj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                currentHookObj.GetComponent<Collider2D>().enabled = true;
                currentHookObj.GetComponent<Rigidbody2D>().linearVelocity = aimDir * 20f;

                canAttack = true;
                isHooked = false;
            }
            else
            {
                player.Audio.PlayOneShot(shootSound);

                currentBulletObj = Instantiate(bulletTypePrefab[(int)currentAmmoType], firePoint.transform.position, Quaternion.identity);
                currentBulletObj.GetComponent<Bullet>().Initialize(aimDir);
            }

            CameraShakeManager.Instance.SmallShake();
        }

        public void AttachRope(GameObject target)
        {
            RopeSystemUtil.DrawNormalRope(line, FirePoint, target);
            RopeSystemUtil.BreakNormalRopeLimit(FirePoint, target, ref isPullBack, ref currentRopeLength, ropeData.maxRopeLength, pullBackForce);
        }

        public IEnumerator Stab()
        {
            CanStabAttack = false;

            stabHitbox.SetActive(true);

            yield return new WaitForSeconds(stabActiveTime);

            stabHitbox.SetActive(false);

            yield return new WaitForSeconds(stabCooldown);

            CanStabAttack = true;
        }
    }
}