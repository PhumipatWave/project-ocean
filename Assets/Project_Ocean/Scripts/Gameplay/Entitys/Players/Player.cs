using Game.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Game.Testing
{
    public class Player : Entity
    {
        [Header("Player Component")]
        public InputReader input;

        // Sounds are played through AudioManager by key (SoundKeys.Hurt / RopeMove / Shoot / Collect) —
        // the clips live in the manager's SFX list, not on the player.

        private PlayerGunHandler gunHandler;
        public PlayerGunHandler GunHandler { get { return gunHandler; } }

        private OxygenRope oxygenTube;
        public OxygenRope OxygenTube { get { return oxygenTube; } }

        private readonly List<CheckPointItem> checkpoints = new();
        private Coroutine _activePullCoroutine;
        public int CheckpointCount => checkpoints.Count;
        public bool CanPlaceCheckpoint => checkpoints.Count < 3;
        public CheckPointItem LastCheckpoint => checkpoints.Count > 0 ? checkpoints[^1] : null;

        public void RegisterCheckpoint(CheckPointItem cp)
        {
            checkpoints.Add(cp);
            InventoryManager.Instance?.RefreshBuildingHints();
        }

        public void UnregisterCheckpoint(CheckPointItem cp)
        {
            checkpoints.Remove(cp);
            if (cp.SourceSlotIndex >= 0)
                InventoryManager.Instance?.ClearBuildingSlot(cp.SourceSlotIndex);
            InventoryManager.Instance?.RefreshBuildingHints();
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
                    InventoryManager.Instance?.ClearBuildingSlot(cp.SourceSlotIndex);
                Destroy(cp.gameObject);
            }
            checkpoints.Clear();
            InventoryManager.Instance?.RefreshBuildingHints();
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

        public Rigidbody2D RB { get { return rb; } }

        [Header("Player Object")]
        [SerializeField] private GameObject playerSprite;
        public GameObject PlayerSprite { get { return playerSprite; } }


        [Header("Hit Flash")]
        [SerializeField] private float playerFlashDuration = 0.15f;
        private SpriteRenderer playerSpriteRenderer;
        private Color playerOriginalColor;
        private Coroutine playerFlashCoroutine;

        [Header("Player Feedback VFX")]
        private PlayerFeedbackVFX playerFeedbackVFX;
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

            if (playerSpriteRenderer != null)
                playerOriginalColor = playerSpriteRenderer.color;

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
            FindPlayerFeedbackVFX();

            //input.OnAimed += Aim;
            input.OnAttacked += Attack;
            input.OnSprinted += Sprint;
            input.OnInteracted += Interact;
            input.OnUseItem += HandleUseItem;
            input.OnTeleport += HandleTeleport;

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

            FindPlayerFeedbackVFX();

            if (scene.name == "Land_Map")
            {
                if (SceneLoadContext.IsLoading)
                {
                    transform.position = SceneLoadContext.Position;
                    // Flag is cleared by SaveBridge after all data is applied.
                }
                else
                {
                    transform.position = GameObject.FindGameObjectWithTag("PlayerSpawnPoint").transform.position;
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
                rb.linearVelocity = Vector2.zero;

                // Cancel any in-flight checkpoint pull and clear all checkpoint state
                if (_activePullCoroutine != null)
                {
                    StopCoroutine(_activePullCoroutine);
                    _activePullCoroutine = null;
                }
                foreach (var cp in checkpoints)
                    if (cp != null && cp.SourceSlotIndex >= 0)
                        InventoryManager.Instance?.ClearBuildingSlot(cp.SourceSlotIndex);
                checkpoints.Clear();
                InventoryManager.Instance?.RefreshBuildingHints();
            }
            else if (scene.name == "Ocean_Map")
            {
                Transform spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint").transform;

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
                var ropeAnchor = GameObject.FindGameObjectWithTag("OxygenTubePoint");
                oxygenTube.SetStartPoint(ropeAnchor != null ? ropeAnchor.transform.position : spawnPoint.position);
            }
        }

        private void Update()
        {
            gunHandler.Aimming(ref faceDir, input.MousePos);
            AimAnim();

            // Test Input
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                gunHandler.currentAmmoType = AmmoType.Normal;
            }
            /*else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                gunHandler.currentAmmoType = AmmoType.GrapplingHook;
            }*/

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

        private void UseItem(ItemType type)
        {
            var prefab = InventoryManager.Instance.PeekItem(type);
            if (prefab != null)
            {
                var effect = prefab.ItemPrefab?.GetComponent<IItemEffect>();
                if (effect != null && !effect.CanExecute(this))
                {
                    Debug.Log($"[Player] Item use blocked by CanExecute: {type}");
                    return;
                }

                // KeepInInventory: execute without consuming — item cleared later by UnregisterCheckpoint
                if (effect != null && effect.KeepInInventory)
                {
                    effect.ExecuteItem(this);
                    TutorialEvents.RaiseAction(TutorialActionId.UseItem);
                    return;
                }
            }

            Item itemData;
            bool success = InventoryManager.Instance.ConsumeItem(type, out itemData);

            if (!success)
            {
                Debug.Log("No item : " + type);
                return;
            }

            ExecuteItem(itemData);
            TutorialEvents.RaiseAction(TutorialActionId.UseItem);
        }

        private void ExecuteItem(Item item)
        {
            if (item.ItemPrefab == null)
            {
                Debug.LogWarning($"[Player] Item '{item.ItemName}' has no prefab assigned.");
                return;
            }

            var effect = item.ItemPrefab.GetComponent<IItemEffect>();
            if (effect != null)
            {
                // IItemEffect handles all spawn/teleport logic itself
                effect.ExecuteItem(this);
            }
            else
            {
                // Fallback: plain building item with no special behavior
                Instantiate(item.ItemPrefab, transform.position, Quaternion.identity);
            }
        }

        #region PlayerInputAction
        private void Aim(bool isAim)
        {
            // Bug 2: block aim entirely when any UI (e.g. inventory) is open
            // Bug 4: block aim when canAttack is false or on land
            if (isAim && (!gunHandler.CanAttack || isLand || PlayerUIState.IsLocked)) return;

            // Released while a UI is open (e.g. Pause opened mid-aim): just drop the aim state.
            // UIManager owns Time.timeScale while locked — writing it here would un-pause the game.
            if (!isAim && PlayerUIState.IsLocked)
            {
                gunHandler.IsAimming = false;
                return;
            }

            gunHandler.IsAimming = isAim;

            if (isAim)
            {
                // Bug 1: zero velocity so holding a move key doesn't keep the player moving while aiming
                rb.linearVelocity = Vector2.zero;
                Time.timeScale = focusTime;
            }
            else
            {
                Time.timeScale = originTime;
            }
        }

        private void Attack(bool isAttack)
        {
            if (isAttack)
            {
                if (!gunHandler.CanAttack || isLand || PlayerUIState.IsLocked) return;
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
                isAttacked = true;
                gunHandler.IsAimming = true;

                rb.linearVelocity = Vector2.zero;
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
                rb.linearVelocity = Vector2.zero;
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

            playerSprintVFX?.SetSprinting(shouldShowSprintVFX);

            // Edge-triggered: play once when movement starts, not every physics tick.
            if (isMoving && !wasMoving)
            {
                if (GameManager.Instance.GameState == GameState.Ocean)
                    AudioManager.Instance?.PlaySFX(SoundKeys.RopeMove);

                if (TutorialGate.RequiredAction == TutorialActionId.Move)
                    Debug.Log($"[Player] Raising Move tutorial action — dir={dir}, wasMoving was false this frame");
                TutorialEvents.RaiseAction(TutorialActionId.Move);
            }
            wasMoving = isMoving;

            MovementAnim();
            FlipSprite();
        }


        private void Interact(bool isInteract)
        {
            if (isInteract)
            {
                Debug.Log("Interact");
            }
        }

        private void HandleUseItem()
        {
            if (!FeatureUnlockService.Instance.IsUnlocked(GameFeature.UseItem)) return;
            if (GameManager.Instance.GameState == GameState.Ocean)
                UseItem(ItemType.Building);
        }

        private void HandleTeleport()
        {
            if (GameManager.Instance.GameState == GameState.Ocean && LastCheckpoint != null)
                LastCheckpoint.PullPlayerHere(this);
        }



        protected override void OnDamagedHandler()
        {
            AudioManager.Instance?.PlaySFX(SoundKeys.Hurt);
        }

        protected override void OnDamagedVisual()
        {
            playerFeedbackVFX?.PlayDamageVFX();

            if (playerSpriteRenderer == null)
                return;

            if (playerFlashCoroutine != null)
                StopCoroutine(playerFlashCoroutine);

            playerFlashCoroutine = StartCoroutine(PlayerHitFlashRoutine());
        }

        private IEnumerator PlayerHitFlashRoutine()
        {
            playerSpriteRenderer.color = entityHealth.HitColor;
            yield return new WaitForSeconds(playerFlashDuration);
            playerSpriteRenderer.color = playerOriginalColor;
            playerFlashCoroutine = null;
        }


        protected override void OnDeathHandler()
        {
            entityDeathHandler?.HandleDeath();
        }

        private void FindPlayerFeedbackVFX()
        {
            playerFeedbackVFX = FindFirstObjectByType<PlayerFeedbackVFX>();
        }
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
        // Bug 3: lets CheckPointItem start a pull coroutine that can be cancelled on scene transition
        public void StartPull(IEnumerator routine)
        {
            if (_activePullCoroutine != null) StopCoroutine(_activePullCoroutine);
            _activePullCoroutine = StartCoroutine(routine);
        }

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
