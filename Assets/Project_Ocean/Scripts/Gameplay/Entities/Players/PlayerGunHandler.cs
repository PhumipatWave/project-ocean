using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Game.Core;

namespace Game.Gameplay
{
    public class PlayerGunHandler : MonoBehaviour
    {
        //[SerializeField] private AmmoWheel ammoWheel;
        [SerializeField] private Player player;


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

        [SerializeField] private LineRenderer aimLine;
        [Tooltip("Fallback laser length, used only when the current ammo has no range of its own. Normal ammo and the hook use their real reach.")]
        [SerializeField] private float aimLineLength = 10f;
        [SerializeField] private LayerMask wallLayer;

        [SerializeField] private GameObject muzzleFlash;
        private ParticleSystem muzzleFlashParticle;

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
        public bool IsAimming
        {
            get { return isAimming; }
            set
            {
                isAimming = value;
                if (!value) SetAimLineActive(false);
            }
        }

        [SerializeField] private bool canAttack = true;
        public bool CanAttack { get { return canAttack; } set { canAttack = value; } }

        private const float FocusTimeScale = .6f;

        private bool isAttacked;
        public bool IsAttacked { get { return isAttacked; } }

        public AmmoType currentAmmoType;

        [Tooltip("The gun and shooting-arm sprites stay hidden until the player aims/fires. Left empty, every SpriteRenderer under this object is used.")]
        [SerializeField] private SpriteRenderer[] gunRenderers;
        [Tooltip("Seconds the gun stays visible after a shot, so the muzzle flash isn't drawn on a hidden gun.")]
        [SerializeField] private float visibleAfterShot = 0.2f;
        private float visibleUntil;

        private void Start()
        {
            player = GetComponentInParent<Player>();

            line = GetComponent<LineRenderer>();
            line.positionCount = ropeData.ropeSegment;

            if (muzzleFlash != null)
            {
                muzzleFlashParticle = muzzleFlash.GetComponent<ParticleSystem>();
                muzzleFlash.SetActive(false);
            }

            if (aimLine != null)
            {
                aimLine.positionCount = 2;
                aimLine.enabled = false;
            }
        }

        // LateUpdate so nothing else touching the renderers this frame can override it.
        private void LateUpdate()
        {
            SetGunVisible(isAimming || isHooked || Time.unscaledTime < visibleUntil);
        }

        private void SetGunVisible(bool visible)
        {
            if (gunRenderers == null || gunRenderers.Length == 0)
                gunRenderers = GetComponentsInChildren<SpriteRenderer>(true);

            foreach (var r in gunRenderers)
                if (r != null) r.enabled = visible;
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
                    SetAimLineActive(false);
                    return;
                }

                Vector2 aimWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
                Vector2 rawDir = (aimWorldPos - (Vector2)transform.position).normalized;

                float angle = Mathf.Atan2(rawDir.y, rawDir.x) * Mathf.Rad2Deg;
                float clamAngle = HandlerGunRotation(ref faceDir, angle);

                float rad = clamAngle * Mathf.Deg2Rad;
                aimDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

                gunSprite.eulerAngles = new Vector3(0, 0, clamAngle);

                UpdateAimLine();
            }
            else if (!isAttacked)
            {
                ResetGunRotation(player.FaceDir);
                SetAimLineActive(false);
            }
        }

        public void HandleAttackInput(bool isAttack)
        {
            if (isAttack)
            {
                if (!canAttack || player.Movement.IsLand || PlayerUIState.IsLocked) return;
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
                isAttacked = true;
                IsAimming = true;

                player.Movement.Stop();
                Time.timeScale = FocusTimeScale;
            }
            else
            {
                isAttacked = false;

                // Released while a UI is open: cancel the shot. UIManager owns Time.timeScale while locked.
                if (PlayerUIState.IsLocked)
                {
                    IsAimming = false;
                    return;
                }

                if (isAimming)
                {
                    canAttack = false;
                    AudioManager.Instance?.PlaySFX(SoundKeys.Shoot);
                    Shooting();
                }
                IsAimming = false;
                Time.timeScale = 1f;
            }
        }

        /// <summary>Clears aim state and restores normal time scale.</summary>
        public void ResetAim()
        {
            IsAimming = false;
            canAttack = true;
            Time.timeScale = 1f;
        }

        private void UpdateAimLine()
        {
            if (aimLine == null || firePoint == null)
                return;

            int level = UpgradeService.Instance.GetLevel(UpgradeType.NormalBullet);

            // Level 0-1 ยังไม่ปลดล็อก Laser
            if (level < 1)
            {
                aimLine.enabled = false;
                return;
            }

            // Level 2 ขึ้นไป เปิด Laser
            aimLine.enabled = true;

            Vector3 start = firePoint.transform.position;
            float range = GetAimRange();

            RaycastHit2D hit = Physics2D.Raycast(
                start,
                aimDir,
                range,
                wallLayer
            );

            Vector3 end = hit.collider != null
                ? (Vector3)hit.point
                : start + (Vector3)aimDir * range;

            aimLine.SetPosition(0, start);
            aimLine.SetPosition(1, end);
        }

        /// <summary>How far the laser reaches = how far the CURRENT ammo actually flies. Read every
        /// frame, so it follows upgrades and ammo switches. Falls back to aimLineLength when the ammo
        /// has no range of its own.</summary>
        private float GetAimRange()
        {
            // The hook flies until the rope is at its maximum length, then pulls back.
            if (currentAmmoType == AmmoType.GrapplingHook)
                return ropeData != null ? ropeData.maxRopeLength : aimLineLength;

            int i = (int)currentAmmoType;
            if (bulletTypePrefab != null && i < bulletTypePrefab.Length && bulletTypePrefab[i] != null)
            {
                var bullet = bulletTypePrefab[i].GetComponent<Bullet>();
                if (bullet != null && bullet.MaxRange > 0f)
                    return bullet.MaxRange;
            }

            return aimLineLength;
        }

        private void SetAimLineActive(bool active)
        {
            if (aimLine != null) aimLine.enabled = active;
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
                ShowMuzzleFlash();

                currentBulletObj = Instantiate(bulletTypePrefab[(int)currentAmmoType], firePoint.transform.position, Quaternion.identity);
                currentBulletObj.GetComponent<Bullet>().Initialize(aimDir);
            }

            visibleUntil = Time.unscaledTime + visibleAfterShot;
            CameraShakeManager.Instance.SmallShake();
        }

        private void ShowMuzzleFlash()
        {
            if (muzzleFlashParticle == null) return;
            muzzleFlash.SetActive(true);
            muzzleFlashParticle.Play();
            StartCoroutine(DisableMuzzleFlashRoutine(muzzleFlashParticle.main.duration));
        }

        private IEnumerator DisableMuzzleFlashRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            muzzleFlash.SetActive(false);
        }

        public void AttachRope(GameObject target)
        {
            RopeSystemUtil.DrawNormalRope(line, FirePoint, target);
            RopeSystemUtil.BreakNormalRopeLimit(FirePoint, target, ref isPullBack, ref currentRopeLength, ropeData.maxRopeLength, pullBackForce);
        }
    }
}