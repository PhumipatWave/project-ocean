using Game.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Game.Gameplay
{
    public class Player : Entity
    {
        /// <summary>Raised when the player takes damage (visual feedback moment).</summary>
        public static event System.Action OnPlayerDamaged;

        [Header("Player Component")]
        public InputReader input;

        // Sounds are played through AudioManager by key (SoundKeys.Hurt / RopeMove / Shoot / Collect) —
        // the clips live in the manager's SFX list, not on the player.

        private PlayerGunHandler gunHandler;
        public PlayerGunHandler GunHandler { get { return gunHandler; } }

        private OxygenRope oxygenTube;
        public OxygenRope OxygenTube { get { return oxygenTube; } }

#if ENABLE_CHECKPOINT_ITEM // unused checkpoint item; re-enable: move Deprecated/CheckpointItem/CheckPointItem.cs back + add the define
        private readonly List<CheckPointItem> checkpoints = new();
        private Coroutine _activePullCoroutine;
        public int CheckpointCount => checkpoints.Count;
        public bool CanPlaceCheckpoint => checkpoints.Count < 3;
        public CheckPointItem LastCheckpoint => checkpoints.Count > 0 ? checkpoints[^1] : null;

        public void RegisterCheckpoint(CheckPointItem cp)
        {
            checkpoints.Add(cp);
            Inventory.Current?.RefreshBuildingHints();
        }

        public void UnregisterCheckpoint(CheckPointItem cp)
        {
            checkpoints.Remove(cp);
            if (cp.SourceSlotIndex >= 0)
                Inventory.Current?.ClearBuildingSlot(cp.SourceSlotIndex);
            Inventory.Current?.RefreshBuildingHints();
        }

        public void ClearAllCheckpoints()
        {
            if (_activePullCoroutine != null)
            {
                StopCoroutine(_activePullCoroutine);
                _activePullCoroutine = null;
            }
            foreach (var cp in checkpoints)
            {
                if (cp == null) continue;
                oxygenTube?.DetachFromCheckpoint(cp.transform.position);
                if (cp.SourceSlotIndex >= 0)
                    Inventory.Current?.ClearBuildingSlot(cp.SourceSlotIndex);
                Destroy(cp.gameObject);
            }
            checkpoints.Clear();
            Inventory.Current?.RefreshBuildingHints();
        }

        /// Returns true if the player can place a new checkpoint from the given building slot index.
        /// Each slot may have at most one active checkpoint at a time; total is capped at 3.
        public bool CanPlaceCheckpointFromSlot(int slotIndex)
        {
            if (checkpoints.Count >= 3) return false;
            foreach (var cp in checkpoints)
                if (cp != null && cp.SourceSlotIndex == slotIndex) return false;
            return true;
        }
#endif

        public Rigidbody2D RB { get { return rb; } }

        [Header("Player Object")]
        [SerializeField] private GameObject playerSprite;
        public GameObject PlayerSprite { get { return playerSprite; } }


        [Header("Player Feedback VFX")]
        private PlayerSprintVFX playerSprintVFX;

        [Header("Player Status")]
        private int faceDir = 1;
        public int FaceDir { get { return faceDir; } }

        [Header("Move Speed - Land")]
        [SerializeField] private float landMoveSpeed = 5f;
        [SerializeField] private float landDashSpeed = 25f;
        [SerializeField] private float landSprintSpeed = 12f;

        [Header("Move Speed - Ocean")]
        [SerializeField] private float oceanMoveSpeed = 8f;
        [SerializeField] private float oceanDashSpeed = 25f;
        [SerializeField] private float oceanSprintSpeed = 10f;

        // ชื่อ/ชนิดค่าคืนเหมือนเดิมทุกอย่าง เพื่อไม่ให้สคริปต์อื่นที่เรียกใช้อยู่ต้องแก้
        // แต่ตอนนี้จะสลับชุดค่าอัตโนมัติตาม isLand
        public float MoveSpeed => isLand ? landMoveSpeed : oceanMoveSpeed;
        public float DashSpeed => isLand ? landDashSpeed : oceanDashSpeed;
        public float SprintSpeed => isLand ? landSprintSpeed : oceanSprintSpeed;

        private bool isSprinting;
        private bool wasMoving;
        private bool moveTutorialWasRequired;

        private bool isLand;

        private bool isAttacked;
        public bool IsAttacked { get { return isAttacked; } }

        private float originTime = 1f;
        private float focusTime = .6f;

        protected override void Awake()
        {
            oxygenTube = GetComponentInChildren<OxygenRope>();

            base.Awake();

            playerSprintVFX = GetComponentInChildren<PlayerSprintVFX>(true);

            gunHandler = GetComponentInChildren<PlayerGunHandler>();

            DontDestroyOnLoad(gameObject);
        }

        protected override IEntityHealth ProvideHealthSource()
        {
            var playerHealth = GetComponent<PlayerHealth>();
            if (oxygenTube != null)
                playerHealth.SetOxygenRope(oxygenTube);
            return playerHealth;
        }

        private void Start()
        {
            input.OnAttacked += Attack;
            input.OnSprinted += Sprint;
            input.OnUseItem += HandleUseItem;
#if ENABLE_CHECKPOINT_ITEM
            input.OnTeleport += HandleTeleport;
#endif

            // Game always starts on land; sceneLoaded for Land_Map fires before the player
            // exists, so we set the initial land state here instead of relying on that event.
            isLand = true;
            gunHandler?.gameObject.SetActive(false);
            entityAnimation?.PlayAnim(AnimHash.IsLand, true);
            oxygenTube?.ResetRope();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayerUIState.Reset();

            if (scene.name == "Land_Map")
            {
                if (SceneLoadContext.IsLoading)
                {
                    transform.position = SceneLoadContext.Position;
                    // Flag is cleared by SaveBridge after all data is applied.
                }
                else
                {
                    transform.position = GameObject.FindGameObjectWithTag(Tags.PlayerSpawnPoint).transform.position;
                }

                // Reset rope max length back to full health on returning to land
                (healthSource as PlayerHealth)?.ResetToOriginal();
                oxygenTube?.ResetRope();

                gunHandler.gameObject.SetActive(false);
                isLand = true;
                entityAnimation?.PlayAnim(AnimHash.IsLand, isLand);

                // Bug 4: reset aim/gun state so canAttack is never stuck on land
                gunHandler.IsAimming = false;
                gunHandler.CanAttack = true;
                Time.timeScale = originTime;
                StopMovement();

#if ENABLE_CHECKPOINT_ITEM
                // Cancel any in-flight checkpoint pull and clear all checkpoint state
                if (_activePullCoroutine != null)
                {
                    StopCoroutine(_activePullCoroutine);
                    _activePullCoroutine = null;
                }
                foreach (var cp in checkpoints)
                    if (cp != null && cp.SourceSlotIndex >= 0)
                        Inventory.Current?.ClearBuildingSlot(cp.SourceSlotIndex);
                checkpoints.Clear();
                Inventory.Current?.RefreshBuildingHints();
#endif
            }
            else if (scene.name == "Ocean_Map")
            {
                Transform spawnPoint = GameObject.FindGameObjectWithTag(Tags.PlayerSpawnPoint).transform;

                if (SceneLoadContext.IsLoading)
                {
                    transform.position = SceneLoadContext.Position;
                    // Flag is cleared by SaveBridge after all data is applied.
                }
                else
                {
                    transform.position = spawnPoint.position;
                }

                gunHandler.gameObject.SetActive(true);
                isLand = false;
                entityAnimation?.PlayAnim(AnimHash.IsLand, isLand);

                // Rope anchor is separate from player spawn; falls back to spawn point if scene has none.
                var ropeAnchor = GameObject.FindGameObjectWithTag(Tags.OxygenTubePoint);
                oxygenTube.SetStartPoint(ropeAnchor != null ? ropeAnchor.transform.position : spawnPoint.position);
            }
        }

        private void Update()
        {
            gunHandler.Aimming(ref faceDir, input.MousePos);
            AimAnim();
        }

        // Movement is re-applied every physics step (not only when the input value changes).
        // Reason: Rigidbody2D collision resolution can zero the velocity component that's
        // pushed into a wall. If we only set velocity from the OnMoved event (which fires
        // once per input change), that zeroed axis never gets restored while the same keys
        // stay held — the player looks stuck moving on a single axis even after clearing the
        // wall. Re-asserting the full input direction every FixedUpdate fixes that.
        private void FixedUpdate()
        {
            ApplyMovement(input.MovePos);
        }

        #region PlayerInputAction
        private void Attack(bool isAttack)
        {
            if (isAttack)
            {
                if (!gunHandler.CanAttack || isLand || PlayerUIState.IsLocked) return;
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
                isAttacked = true;
                gunHandler.IsAimming = true;

                StopMovement();
                Time.timeScale = focusTime;
            }
            else
            {
                isAttacked = false;

                // Released while a UI is open: cancel the shot (no fire, no timeScale write) —
                // UIManager owns Time.timeScale while locked.
                if (PlayerUIState.IsLocked)
                {
                    gunHandler.IsAimming = false;
                    return;
                }

                if (gunHandler.IsAimming)
                {
                    gunHandler.CanAttack = false;
                    AudioManager.Instance?.PlaySFX(SoundKeys.Shoot);
                    gunHandler.Shooting();
                }
                gunHandler.IsAimming = false;
                Time.timeScale = originTime;
            }
            
        }

        private void Sprint(bool state)
        {
            isSprinting = state;

            if (!state)
            {
                playerSprintVFX?.SetSprinting(false);
            }
        }

        private void ApplyMovement(Vector2 dir)
        {
            // The instant the "press WASD" tutorial step becomes active, re-baseline the
            // edge detector to whatever is currently held. Otherwise movement already in
            // flight before the prompt appeared (e.g. the player was walking around during
            // the prior dialogue step) reads as a fresh press and completes the tutorial
            // with no new key press — only a genuine release-then-press after the prompt
            // shows should count.
            bool moveTutorialRequired = TutorialGate.RequiredAction == TutorialActionId.Move;
            if (moveTutorialRequired && !moveTutorialWasRequired)
                wasMoving = dir != Vector2.zero;
            moveTutorialWasRequired = moveTutorialRequired;

            if (gunHandler.IsAimming || PlayerUIState.IsLocked)
            {
                StopMovement();
                // Track the raw held-input state (not hardcoded false) so a key already
                // held through the lock doesn't look like a fresh press-edge the instant
                // the lock releases — e.g. holding W through dialogue would otherwise
                // instantly satisfy a "press W A S D" tutorial step with no new key press.
                playerSprintVFX?.SetSprinting(false);
                wasMoving = dir != Vector2.zero;
                return;
            }

            float speed = isSprinting ? SprintSpeed : MoveSpeed;
            rb.linearVelocity = dir * speed;

            bool isMoving = rb.linearVelocity != Vector2.zero;

            bool shouldShowSprintVFX =
                isLand && isSprinting && isMoving;

            if (shouldShowSprintVFX)
                playerSprintVFX?.SetDirection(dir);

            playerSprintVFX?.SetSprinting(shouldShowSprintVFX);

            // Edge-triggered: play once when movement starts, not every physics tick.
            if (isMoving && !wasMoving)
            {
                if (GameSession.State == GameState.Ocean)
                    AudioManager.Instance?.PlaySFX(SoundKeys.RopeMove);

                if (TutorialGate.RequiredAction == TutorialActionId.Move)
                    GameLog.Info($"[Player] Raising Move tutorial action — dir={dir}, wasMoving was false this frame");
                TutorialEvents.RaiseAction(TutorialActionId.Move);
            }
            wasMoving = isMoving;

            MovementAnim();
            FlipSprite();
        }



        private void HandleUseItem()
        {
            if (!FeatureUnlockService.Instance.IsUnlocked(GameFeature.UseItem)) return;
            if (GameSession.State == GameState.Ocean)
                PlayerItemUse.Use(this, ItemType.Building);
        }

#if ENABLE_CHECKPOINT_ITEM
        private void HandleTeleport()
        {
            if (GameSession.State == GameState.Ocean && LastCheckpoint != null)
                LastCheckpoint.PullPlayerHere(this);
        }
#endif



        protected override void OnDamagedHandler()
        {
            AudioManager.Instance?.PlaySFX(SoundKeys.Hurt);
        }

        // The flash itself runs in PlayerHealth (its flashTarget is the Visual sprite).
        protected override void OnDamagedVisual() => OnPlayerDamaged?.Invoke();

        #endregion

        #region PlayerAnimation
        private void MovementAnim()
        {
            entityAnimation?.PlayAnim(AnimHash.VelX, rb.linearVelocity.x);
            entityAnimation?.PlayAnim(AnimHash.VelY, rb.linearVelocity.y);
        }

        private void AimAnim()
        {
            entityAnimation?.PlayAnim(AnimHash.IsAim, gunHandler.IsAimming);
        }
        #endregion

        #region PlayerUtils
#if ENABLE_CHECKPOINT_ITEM
        // Bug 3: lets CheckPointItem start a pull coroutine that can be cancelled on scene transition
        public void StartPull(IEnumerator routine)
        {
            if (_activePullCoroutine != null) StopCoroutine(_activePullCoroutine);
            _activePullCoroutine = StartCoroutine(routine);
        }
#endif

        private void FlipSprite()
        {
            if (rb.linearVelocityX > .1f)
            {
                faceDir = 1;
                playerSprite.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                gunHandler.GunSprite.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else if (rb.linearVelocityX < -.1f)
            {
                faceDir = -1;
                playerSprite.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                gunHandler.GunSprite.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }
        #endregion
    }
}
